module TextbookDownloaderCli.DownloadBooks

open System
open System.IO
open System.Threading.Tasks
open TextbookDownloaderCli.GetBook

let downloadAllBookInfoJson (getString: string -> string) =
    getString "https://s-file-2.ykt.cbern.com.cn/zxx/ndrs/resources/tch_material/part_100.json"

let downloadSingleBookInfoJson (getString: string -> string) (bookId: string) =
    let url =
        $"https://s-file-1.ykt.cbern.com.cn/zxx/ndrv2/resources/tch_material/details/{bookId}.json"

    getString url

let private saveStream (stream: Stream) (fileInfo: FileInfo) =
    use fileStream = File.Create(fileInfo.FullName)
    stream.CopyTo(fileStream)

let private DoParallelly maxThreads (actions: seq<unit -> unit>) =
    let convertToAction (action: unit -> unit) = Action(action)
    let actionsArray: Action array = actions |> Seq.map convertToAction |> Array.ofSeq

    let parallelOptions = ParallelOptions(MaxDegreeOfParallelism = maxThreads)

    Parallel.Invoke(parallelOptions, actionsArray)

type DownloadBookEvent =
    | Action of (TextbookWithTag -> unit)
    | None

type DownloadBookEvents =
    { DownloadStarted: DownloadBookEvent
      DownloadFinished: DownloadBookEvent }

type private TextbookWithFileInfo =
    { Textbook: TextbookWithTag
      FileInfo: FileInfo }

let rec private getBookWithFileInfo (directoryInfo: DirectoryInfo) (book: TextbookWithTag) =
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

let private downloadABook (downloadEvents: DownloadBookEvents) getStream (book: TextbookWithFileInfo) =
    match downloadEvents.DownloadStarted with
    | Action action -> action book.Textbook
    | None -> ()

    let stream = getStream book.Textbook.Textbook.Url
    saveStream stream book.FileInfo

    match downloadEvents.DownloadFinished with
    | Action action -> action book.Textbook
    | None -> ()

let downloadBooks
    (getStream: string -> Stream)
    (maxThreads: int)
    (directoryInfo: DirectoryInfo)
    (downloadEvents: DownloadBookEvents)
    (books: TextbookWithTag seq)
    =

    let downloadBookByInfo = downloadABook downloadEvents getStream
    let getBookWithFileInfo_ = getBookWithFileInfo directoryInfo
    let downloadBook = getBookWithFileInfo_ >> downloadBookByInfo
    let getDownloadAction book = fun () -> downloadBook book
    let downloadActions = Seq.map getDownloadAction books

    DoParallelly maxThreads downloadActions
