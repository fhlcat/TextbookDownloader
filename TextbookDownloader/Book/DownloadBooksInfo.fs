module TextbookDownloader.DownloadBookInfo

open TextbookDownloader.SortTags
open TextbookDownloaderCli.ParseBooksInfo

let downloadAllBooksInfo (getString: string -> string) =
    let booksInfoJson =
        let url =
            "https://s-file-2.ykt.cbern.com.cn/zxx/ndrs/resources/tch_material/part_100.json"

        getString url

    let bookIdSeq = parseBooksId booksInfoJson

    let downloadSingleBookInfoJson (bookId: string) =
        let url =
            $"https://s-file-1.ykt.cbern.com.cn/zxx/ndrv2/resources/tch_material/details/{bookId}.json"

        getString url

    let sortBookTags (book: BookInfoWithTag) =
        { book with
            Tags = book.Tags |> sortTags getString }

    bookIdSeq
    |> Seq.map downloadSingleBookInfoJson
    |> Seq.map parseBookWithTagsUnsorted
    |> Seq.map sortBookTags