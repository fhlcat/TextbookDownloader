module TextbookDownloader.DownloadBookInfo

open TextbookDownloader.Book.TextbookInfo
open TextbookDownloader.SortTags

let downloadBooksInfo auth =
    let getString, _ = TextbookDownloader.Download.PrepareDownload.prepareDownload auth

    let booksId =
        [| for i in 1..3 ->
               getString @$"https://s-file-2.ykt.cbern.com.cn/zxx/ndrs/resources/tch_material/part_10{i}.json"
               |> parseBooksId |]
        |> Seq.concat
        |> Array.ofSeq

    let tryDownloadSingleBookInfoJson (bookId: string) =
        try 
            getString $"https://s-file-1.ykt.cbern.com.cn/zxx/ndrv2/resources/tch_material/details/{bookId}.json" |> Some
        with
        | _ -> None

    let sort =
        getString "https://s-file-1.ykt.cbern.com.cn/zxx/ndrs/tags/tch_material_tag.json"
        |> sortTags

    booksId |> Seq.choose tryDownloadSingleBookInfoJson |> Seq.choose tryOfJson |> Seq.map sort
