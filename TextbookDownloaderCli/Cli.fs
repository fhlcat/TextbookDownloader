module TextbookDownloaderCli.Cli

open System
open System.IO
open TextbookDownloader.DownloadBookInfo
open TextbookDownloader.PrepareDownload
open TextbookDownloaderCli.Book
open TextbookDownloaderCli.DownloadBooks
open TextbookDownloaderCli.Folder
open System.CommandLine
open TextbookDownloaderCli.Tools

let private monitorDownloadProgress () =
    let mutable downloadingBooks: string array = [||]

    let downloadStarted (book: BookInfoWithTag) =
        let updatedDownloadingBooks =
            (Array.append downloadingBooks [| book.Textbook.Name |])

        downloadingBooks <- updatedDownloadingBooks
        Console.Clear()
        printDownloadingBooks downloadingBooks

    let downloadEnded (book: BookInfoWithTag) =
        let filter name = not (name = book.Textbook.Name)
        let updatedDownloadingBooks = Array.filter filter downloadingBooks
        downloadingBooks <- updatedDownloadingBooks
        Console.Clear()
        printDownloadingBooks downloadingBooks

    let downloadStartedLocked book =
        let localDownloadStarted () = downloadStarted book
        lock downloadingBooks localDownloadStarted

    let downloadEndedLocked book =
        let localDownloadEnded () = downloadEnded book
        lock downloadingBooks localDownloadEnded

    { DownloadStarted = Action downloadStartedLocked
      DownloadFinished = Action downloadEndedLocked }

let downloadFolderCommandHandler (auth: string) (threads: int) (folder: string) =
    let getString, getStream = prepareDownload auth

    let books = downloadBooksInfoWithTagSeq getString
    let folders = getFolder books

    let selectedFolder = askForFolder folders
    let selectedBooks = selectedFolder.Books

    let events = monitorDownloadProgress ()
    let directoryInfo = DirectoryInfo(folder)

    downloadBooks getStream threads directoryInfo events selectedBooks

let getDownloadFolderCommand () =
    let downloadFolderCommand = Command "DownloadFolder"
    downloadFolderCommand.AddAlias "df"

    let authArgument =
        Argument<string>("authFile", "a txt file that contains x-nd-auth value in the headers")

    downloadFolderCommand.AddArgument authArgument

    let threadsOption = Option<int>("--threads", "Number of threads to download")
    threadsOption.AddAlias "-t"
    threadsOption.SetDefaultValue 4
    downloadFolderCommand.AddOption threadsOption

    let folderOption = Option<string>("--folder", "Directory to download the books")
    folderOption.AddAlias "-f"
    downloadFolderCommand.AddOption folderOption

    downloadFolderCommand.SetHandler(downloadFolderCommandHandler, authArgument, threadsOption, folderOption)

    downloadFolderCommand

let invoke (args: string array) =
    let rootCommand = RootCommand "Textbook Downloader CLI"
    rootCommand.AddCommand(getDownloadFolderCommand ())

    rootCommand.Invoke(args)
