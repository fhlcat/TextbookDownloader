module TextbookDownloaderCli.CliHandlers

open System
open System.IO
open TextbookDownloader.Book.TextbookInfo
open TextbookDownloaderCli.GatherFolderInfo
open TextbookDownloaderCli.CliTools.Folder
open TextbookDownloaderCli.DownloadBooks

let downloadFolderCommandHandler (auth: string) (threads: int) (outputPath: string) =
    let log (message: string) = Console.WriteLine message
    let folders = gatherFolderInfo log auth
    let selectedFolder = askForFolder folders

    downloadBooks
        { MaxThreads = threads
          DownloadStarted = Action(fun (book: TextbookInfo) -> log $"Downloading {book.Name}...")
          DownloadFinished = Action(fun (book: TextbookInfo) -> log $"Downloaded {book.Name} successfully.")
          Auth = auth
          DirectoryInfo = DirectoryInfo(outputPath) }
        selectedFolder.Books
