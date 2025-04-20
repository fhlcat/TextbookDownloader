module TextbookDownloaderCli.CliHandlers

open System
open System.IO
open TextbookDownloader.Book.TextbookInfo
open TextbookDownloaderCli.GatherFolderInfo
open TextbookDownloaderCli.CliTools.Folder
open TextbookDownloaderCli.DownloadBooks

let downloadFolderCommandHandler (authFilePath: string) (interval: int) (outputPath: string) =
    let log (message: string) = Console.WriteLine message
    let folders = gatherFoldersInfo log authFilePath
    let selectedFolder = askForFolder folders
    let auth = File.ReadAllText authFilePath

    downloadBooks
        { interval = interval
          Events =
            { Waiting = Some(fun _ -> ())
              DownloadStarted = Some(fun book -> log $"Downloading {book.Name}...")
              DownloadFinished = Some(fun book -> log $"Downloaded {book.Name} successfully.") }
          Auth = auth
          DirectoryInfo = DirectoryInfo(outputPath) }
        selectedFolder.Books
