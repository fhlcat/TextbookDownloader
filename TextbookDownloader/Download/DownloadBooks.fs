module TextbookDownloaderCli.DownloadBooks

open System.IO
open TextbookDownloader.Book.TextbookInfo
open TextbookDownloader.Download.PrepareDownload

let private saveStream (stream: Stream) (fileInfo: FileInfo) =
    Directory.CreateDirectory fileInfo.DirectoryName |> ignore
    use fileStream = File.Create(fileInfo.FullName)
    stream.CopyTo(fileStream)

type DownloadBookEvents =
    { DownloadStarted: Option<TextbookInfo -> unit>
      DownloadFinished: Option<TextbookInfo -> unit>
      Waiting: Option<int -> unit> }

type BooksDownloadOptions =
    { interval: int
      DirectoryInfo: DirectoryInfo
      Auth: string
      Events: DownloadBookEvents }

let private createFileInfo (directory: DirectoryInfo) (book: TextbookInfo) =
    FileInfo(
        Path.Combine(
            Array.concat
                [ [| directory.FullName |]
                  (Array.ofSeq book.Tags)
                  [| (book.Name + ".pdf") |] ]
        )
    )
    
let callEventIfExists event arg =
    match event with
    | Some action -> action arg
    | None -> ()

let downloadBooks (options: BooksDownloadOptions) (books: TextbookInfo seq) =
    let _, getStream = prepareDownload options.Auth
    let getFileInfo = createFileInfo options.DirectoryInfo

    for book in books do
        callEventIfExists options.Events.DownloadStarted book

        saveStream (getStream book.Url) (getFileInfo book)
        
        callEventIfExists options.Events.Waiting options.interval
        if options.interval <> 0 then
            System.Threading.Thread.Sleep options.interval

        callEventIfExists options.Events.DownloadFinished book