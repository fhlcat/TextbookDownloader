module TextbookDownloader.Download.PrepareDownload

open System.Net.Http

let prepareDownload (auth: string) =
    let client = new HttpClient()
    client.DefaultRequestHeaders.Add("x-nd-auth", auth)

    let getString (url: string) = client.GetStringAsync(url).Result
    let getStream (url: string) = client.GetStreamAsync(url).Result

    (getString, getStream)
