module TextbookDownloaderFsharp.Cli

open System
open System.Net.Http
open TextbookDownloaderFsharp.DownloadBooks
open TextbookDownloaderFsharp.ParseBooks

let singleFolderPrompt index (folder: Folder) =
    let endString = if not (Seq.isEmpty (folder.GetChildren())) then ">" else ""
    $"[{index}]{folder.Name}{endString}"

let prompt (folders: Folder seq) =
    let foldersPrompt = (folders |> Seq.mapi singleFolderPrompt |> String.concat "\n")
    $"Select a folder:\n{foldersPrompt}\n"

let rec askForFolder (folders: Folder seq) : Folder =
    Console.WriteLine(prompt folders)
    let input = Console.ReadLine()

    let mutable result = 0

    if Int32.TryParse(input, &result) then
        Seq.item result folders
    elif Int32.TryParse(input[.. input.Length - 1], &result) then
        let selectedFolder = Seq.item result folders
        let children = selectedFolder.GetChildren()
        askForFolder children
    else
        Console.WriteLine "Invalid input, please try again"
        askForFolder folders

let printDownloadingBooks books =
    Console.WriteLine "Downloading books:"

    for book: string in books do
        Console.WriteLine book

let prepareClient (auth: string) =
    let httpClient = new HttpClient()
    httpClient.DefaultRequestHeaders.Add("x-nd-auth", auth)
    httpClient

let downloadCli (auth: string) (threads: int) (folder: string) =
    let client = prepareClient auth
    let getString (url: string) = client.GetStringAsync(url).Result
    let getStream (url: string) = client.GetStreamAsync(url).Result

    let booksInfoJson = downloadAllBookInfoJson getString
    let bookIds = parseBookIds booksInfoJson
    let bookInfoJsons = bookIds |> Seq.map (downloadSingleBookInfoJson getString)
    let books = bookInfoJsons |> Seq.map parseBook
    let folders = parseFolders books

    let selectedFolder = askForFolder folders
    let books = selectedFolder.GetBooks()

    let downloadOptions =
        { Books = books
          Directory = folder
          Threads = threads }

    let mutable downloadingBooks: string array = [||]

    let downloadStarted (book: Book) =
        let updatedDownloadingBooks = (Array.append downloadingBooks [| book.Name |])
        downloadingBooks <- updatedDownloadingBooks
        Console.Clear()
        printDownloadingBooks downloadingBooks

    let downloadEnded (book: Book) =
        let filter name = not (name = book.Name)
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
