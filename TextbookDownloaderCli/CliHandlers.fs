module TextbookDownloaderCli.CliHandlers

open System
open System.IO
open Serilog
open TextbookDownloader.Book.TextbookInfo
open TextbookDownloaderCli.GatherFolderInfo
open TextbookDownloaderCli.CliTools.Folder
open TextbookDownloaderCli.DownloadBooks

let downloadFolderCommandHandler (authFilePath: string) (interval: int) (outputPath: string) =
    Log.Logger <- 
        LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("log.txt")
            .CreateLogger()
    let folders = gatherFoldersInfo Log.Information authFilePath
    let selectedFolder = askForFolder folders
    let auth = File.ReadAllText authFilePath

    downloadBooks
        { interval = interval
          Events =
            { Waiting = Some(fun interval -> Log.Information $"Waiting {interval}ms...")
              DownloadStarted = Some(fun book -> Log.Information $"Downloading {book.Name}...")
              DownloadFinished = Some(fun book -> Log.Information $"Downloaded {book.Name} successfully.") }
          Auth = auth
          DirectoryInfo = DirectoryInfo(outputPath) }
        selectedFolder.Books
