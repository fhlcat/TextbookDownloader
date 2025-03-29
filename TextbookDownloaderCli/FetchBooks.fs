module TextbookDownloaderCli.FetchBooks

open System.Net.Http
open TextbookDownloaderCli.DownloadBooks
open TextbookDownloaderCli.GetBook

let prepareDownload (auth: string) =
    use client = new HttpClient()
    client.DefaultRequestHeaders.Add("x-nd-auth", auth)

    let getString (url: string) = client.GetStringAsync(url).Result
    let getStream (url: string) = client.GetStreamAsync(url).Result

    (getString, getStream)

let fetchBooks getString =
    let booksInfoJson = downloadAllBookInfoJson getString
    let bookIds = getBooksId booksInfoJson
    Seq.map getBook bookIds

let