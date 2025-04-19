module TextbookDownloader.DownloadBookInfo

open TextbookDownloader.Book.TextbookInfo
open TextbookDownloader.SortTags

let downloadBooksInfo auth =
    let getString, _ = TextbookDownloader.Download.PrepareDownload.prepareDownload auth

    let booksId =
        getString "https://s-file-2.ykt.cbern.com.cn/zxx/ndrs/resources/tch_material/part_100.json"
        |> parseBooksId

    let downloadSingleBookInfoJson (bookId: string) =
        getString $"https://s-file-1.ykt.cbern.com.cn/zxx/ndrv2/resources/tch_material/details/{bookId}.json"

    let sort =
        getString "https://s-file-1.ykt.cbern.com.cn/zxx/ndrs/tags/tch_material_tag.json"
        |> sortTags

    booksId |> Seq.map downloadSingleBookInfoJson |> Seq.map ofJson |> Seq.map sort
