module TextbookDownloader.DownloadBookInfo

open TextbookDownloaderCli.Book

let private downloadAllBookInfoJson (getString: string -> string) =
    getString "https://s-file-2.ykt.cbern.com.cn/zxx/ndrs/resources/tch_material/part_100.json"

let private downloadSingleBookInfoJson (getString: string -> string) (bookId: string) =
    let url =
        $"https://s-file-1.ykt.cbern.com.cn/zxx/ndrv2/resources/tch_material/details/{bookId}.json"

    getString url

let downloadBooksInfoWithTagSeq (getString: string -> string) =
    let booksInfoJson = downloadAllBookInfoJson getString
    let bookIdSeq = getBooksId booksInfoJson
    let booksInfoJsonSeq = Seq.map (downloadSingleBookInfoJson getString) bookIdSeq
    Seq.map getBook booksInfoJsonSeq