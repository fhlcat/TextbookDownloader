open System
open System.CommandLine
open TextbookDownloaderCli.Cli
open TextbookDownloaderCli.FetchBooks
open TextbookDownloaderCli.Folders
open TextbookDownloaderCli.GetBook

let downloadFolderCommandHandler (auth: string) (threads: int) (folder: string) =
    let getString, getStream = prepareDownload auth
    let books = fetchBooks getString
    let folders = ofBooks books

    let selectedFolder = askForFolder folders
    let books = selectedFolder.Books

    let mutable downloadingBooks: string array = [||]

    let downloadStarted (book: TextbookWithTag) =
        let updatedDownloadingBooks = (Array.append downloadingBooks [| book.Name |])
        downloadingBooks <- updatedDownloadingBooks
        Console.Clear()
        printDownloadingBooks downloadingBooks

    let downloadEnded (book: TextbookWithTag) =
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

    let downloadEvents =
        { DownloadStarted = downloadStartedLocked
          DownloadFinished = downloadEndedLocked }

    downloadBooks getStream downloadOptions downloadEvents

    let downloadOptions =
        { DownloadStarted = printDownloadingBooks
          DownloadFinished = printDownloadingBooks }

    downloadBooks getStream books threads downloadOptions

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

let args = Environment.GetCommandLineArgs()[1..]
exit (invoke args)
