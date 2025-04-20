module TextbookDownloaderCli.CliHandlers

open System
open System.IO
open TextbookDownloader.Book.TextbookInfo
open TextbookDownloaderCli.GatherFolderInfo
open TextbookDownloaderCli.CliTools.Folder
open TextbookDownloaderCli.DownloadBooks

let downloadFolderCommandHandler (authFilePath: string) (threads: int) (outputPath: string) =
    let log (message: string) = Console.WriteLine message
    let folders = gatherFoldersInfo log authFilePath
    let selectedFolder = askForFolder folders
    let auth = File.ReadAllText authFilePath

    downloadBooks
        { MaxThreads = threads
          DownloadStarted = Action(fun (book: TextbookInfo) -> log $"Downloading {book.Name}...")
          DownloadFinished = Action(fun (book: TextbookInfo) -> log $"Downloaded {book.Name} successfully.")
          Auth = auth
          DirectoryInfo = DirectoryInfo(outputPath) }
        selectedFolder.Books
