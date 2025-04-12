module TextbookDownloaderCli.CliHandlers

open System
open System.IO
open TextbookDownloader
open TextbookDownloader.DownloadBookInfo
open TextbookDownloader.PrepareDownload
open TextbookDownloaderCli.ParseBooksInfo
open TextbookDownloaderCli.CliTools.Folder
open TextbookDownloaderCli.DownloadBooks
open TextbookDownloaderCli.ParseFolders

//todo implementation
let private log (message: string) = Console.WriteLine message

let downloadFolderCommandHandler (auth: string) (threads: int) (folder: string) =
    let getString, getStream = prepareDownload auth

    log "Retrieving books information..."
    let booksInfo = downloadAllBooksInfo getString
    log "Retrieving books information completed."

    let sortTags_ = SortTags.sortTags getString

    let booksWithTagsSorted =
        booksInfo
        |> Seq.map (fun book ->
            { book with
                Tags = book.Tags |> sortTags_ book.Tags})

    log "Parsing folders information..."
    let folders = mapFolder booksInfo
    log "Parsing folders information completed."

    let downloadEvents =
        { DownloadStarted = Action(fun (book: BookInfoWithTag) -> log $"Downloading {book.Textbook.Name}...")
          DownloadFinished = Action(fun (book: BookInfoWithTag) -> log $"Downloaded {book.Textbook.Name} successfully.") }

    let selectedFolder = askForFolder folders

    let options =
        { MaxThreads = threads
          DownloadEvents = downloadEvents
          DirectoryInfo = DirectoryInfo(folder)
          Books = selectedFolder.Books }

    download getStream options
