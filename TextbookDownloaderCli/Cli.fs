module TextbookDownloaderCli.Cli

open System
open System.IO
open TextbookDownloader.DownloadBookInfo
open TextbookDownloader.PrepareDownload
open TextbookDownloaderCli.Book
open TextbookDownloaderCli.CliTools.Folder
open TextbookDownloaderCli.DownloadBooks
open TextbookDownloaderCli.Folder
open System.CommandLine

//todo implementation
let private log (message: string) = Console.WriteLine message

let private events =
    let downloadStarted (book: BookInfoWithTag) = log $"Downloading {book.Textbook.Name}..."
    let downloadFinished (book: BookInfoWithTag) = log $"Downloaded {book.Textbook.Name} successfully."

    {
        DownloadStarted = Action downloadStarted
        DownloadFinished = Action downloadFinished
    }

let downloadFolderCommandHandler (auth: string) (threads: int) (folder: string) =
    let getString, getStream = prepareDownload auth

    log "Retrieving books information..."
    let allBooks = downloadBooksInfoWithTagSeq getString
    log "Retrieving books information completed."

    log "Parsing folders information..."
    let folders = getFolder allBooks
    log "Parsing folders information completed."

    let selectedFolder = askForFolder folders
    let directoryInfo = DirectoryInfo(folder)

    let options =
        { MaxThreads = threads
          DownloadEvents = events
          DirectoryInfo = directoryInfo
          Books = selectedFolder.Books }

    downloadBooks getStream options

let private downloadFolderCommand =
    let downloadFolderCommand = Command "DownloadFolder"
    downloadFolderCommand.AddAlias "df"

    let authArgument =
        Argument<string>("authFile", "a txt file that contains x-nd-auth value in the headers")

    downloadFolderCommand.AddArgument authArgument

    let threadsOption =
        let option = Option<int>("--threads", "Number of threads to download")
        option.AddAlias "-t"
        option.SetDefaultValue 4
        option

    downloadFolderCommand.AddOption threadsOption

    let folderOption =
        let option = Option<string>("--folder", "Directory to download the books")
        option.AddAlias "-f"
        option

    downloadFolderCommand.AddOption folderOption

    downloadFolderCommand.SetHandler(downloadFolderCommandHandler, authArgument, threadsOption, folderOption)

    downloadFolderCommand

let private rootCommand =
    let command = RootCommand "Textbook Downloader CLI"
    command.AddCommand(downloadFolderCommand)
    command

let invoke: (string array -> int) = rootCommand.Invoke
