module TextbookDownloaderCli.DownloadBooks

open System
open System.IO
open System.Threading.Tasks
open TextbookDownloaderCli.ParseBooksInfo

let private saveStream (stream: Stream) (fileInfo: FileInfo) =
    Directory.CreateDirectory fileInfo.DirectoryName |> ignore
    use fileStream = File.Create(fileInfo.FullName)
    stream.CopyTo(fileStream)

let private DoParallelly maxThreads (actions: seq<unit -> unit>) =
    let convertToAction (action: unit -> unit) = Action(action)
    let actionsArray: Action array = actions |> Seq.map convertToAction |> Array.ofSeq

    let parallelOptions = ParallelOptions(MaxDegreeOfParallelism = maxThreads)

    Parallel.Invoke(parallelOptions, actionsArray)

type DownloadBookEvent =
    | Action of (BookInfoWithTag -> unit)
    | None

type DownloadBookEvents =
    { DownloadStarted: DownloadBookEvent
      DownloadFinished: DownloadBookEvent }

type private TextbookWithFileInfo =
    { Textbook: BookInfoWithTag
      FileInfo: FileInfo }

let rec private getBookWithFileInfo (directoryInfo: DirectoryInfo) (book: BookInfoWithTag) =
    if Seq.isEmpty book.Tags then
        let fullFileName = Path.Combine(directoryInfo.FullName, book.Textbook.Name + ".pdf")

        { Textbook = book
          FileInfo = FileInfo(fullFileName) }
    else
        let newDirectoryInfo =
            DirectoryInfo(Path.Combine(directoryInfo.FullName, book.Tags |> Seq.head))

        let newBook =
            { book with
                Tags = book.Tags |> Seq.skip 1 }

        getBookWithFileInfo newDirectoryInfo newBook

let private downloadABook
    (downloadEvents: DownloadBookEvents)
    (getStream: string -> Stream)
    (book: TextbookWithFileInfo)
    =
    match downloadEvents.DownloadStarted with
    | Action action -> action book.Textbook
    | None -> ()

    let stream = getStream book.Textbook.Textbook.Url
    saveStream stream book.FileInfo

    match downloadEvents.DownloadFinished with
    | Action action -> action book.Textbook
    | None -> ()

type DownloadOptions =
    { MaxThreads: int
      DirectoryInfo: DirectoryInfo
      DownloadEvents: DownloadBookEvents
      Books: BookInfoWithTag seq }

let download
    (getStream: string -> Stream)
    (options: DownloadOptions)
    =

    let downloadBookByInfo = downloadABook options.DownloadEvents getStream
    let downloadABook_ = getBookWithFileInfo options.DirectoryInfo >> downloadBookByInfo
    let getDownloadAction book = fun () -> downloadABook_ book
    let downloadActions = Seq.map getDownloadAction options.Books

    DoParallelly options.MaxThreads downloadActions
