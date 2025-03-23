module TextbookDownloaderFsharp.DownloadBooks

open System.IO
open System.Threading.Tasks

let downloadSingleBook getStream (directory: string) (book: Book) =
    let (stream: Stream) = getStream book.Url
    let fileName = book.Name + ".pdf"

    let targetDirectory =
        (Array.insertAt 0 directory (Array.ofSeq book.Tags)) |> Path.Combine

    let fullFileNameParts = [| targetDirectory; fileName |]
    let fullFileName = Path.Combine fullFileNameParts
    let fileStream = File.Create fullFileName

    stream.CopyTo(fileStream)

let downloadBooks getStream (options: DownloadOptions) (events: DownloadEvents) =
    let localDownloadSingleBook = downloadSingleBook getStream options.Directory

    let localDownloadSingleBookWithCallback book =
        events.DownloadStarted book
        localDownloadSingleBook book
        events.DownloadFinished book

    let parallelOptions = ParallelOptions(MaxDegreeOfParallelism = options.Threads)
    Parallel.ForEach(options.Books, parallelOptions, localDownloadSingleBookWithCallback)

let downloadAllBookInfoJson (getString: string -> string) =
    getString "https://s-file-2.ykt.cbern.com.cn/zxx/ndrs/resources/tch_material/part_100.json"

let downloadSingleBookInfoJson (getString: string -> string) (bookId: string) =
    let url =
        $"https://s-file-2.ykt.cbern.com.cn/zxx/ndrs/resources/tch_material/{bookId}.json"

    getString url
