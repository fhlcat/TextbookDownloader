module TextbookDownloaderCli.DownloadBooks

open System
open System.IO
open System.Threading.Tasks
open TextbookDownloader.Book.TextbookInfo
open TextbookDownloader.Download.PrepareDownload

let private saveStream (stream: Stream) (fileInfo: FileInfo) =
    Directory.CreateDirectory fileInfo.DirectoryName |> ignore
    use fileStream = File.Create(fileInfo.FullName)
    stream.CopyTo(fileStream)

let private DoParallelly maxThreads (actions: (unit -> unit) seq) =
    Parallel.Invoke(
        ParallelOptions(MaxDegreeOfParallelism = maxThreads),
        actions |> Seq.map (fun action -> Action action) |> Array.ofSeq
    )

type DownloadBookEvent =
    | Action of (TextbookInfo -> unit)
    | None

type BooksDownloadOptions =
    { MaxThreads: int
      DirectoryInfo: DirectoryInfo
      Auth: string
      DownloadStarted: DownloadBookEvent
      DownloadFinished: DownloadBookEvent }

let private createFileInfo (directory: DirectoryInfo) (book: TextbookInfo) =
    FileInfo(
        Path.Combine(
            Array.concat
                [ [| directory.FullName |]
                  (Array.ofSeq book.Tags)
                  [| (book.Name + ".pdf") |] ]
        )
    )

let downloadBooks (options: BooksDownloadOptions) (books: TextbookInfo seq) =
    let _, getStream = prepareDownload options.Auth
    let getFileInfo = createFileInfo options.DirectoryInfo

    for book in books do
        match options.DownloadStarted with
        | Action action -> action book
        | None -> ()

        saveStream (getStream book.Url) (getFileInfo book)

        match options.DownloadFinished with
        | Action action -> action book
        | None -> ()
