module TextbookDownloaderFsharp.ParseBooks

open System.Text.Json

let isNotNull x = not (isNull x)

let parseBookIds (json: string) =
    let document = JsonDocument.Parse json
    let getBookId (element: JsonElement) = element.GetProperty("id").GetString()
    let ids = document.RootElement.EnumerateArray() |> Seq.map getBookId

    Seq.filter isNotNull ids

let parseBook (json: string) =
    let document = JsonDocument.Parse json
    let rootElement = document.RootElement

    let name = rootElement.GetProperty("global_title").GetProperty("zh-CN").GetString()

    let isPdf (element: JsonElement) =
        element.GetProperty("ti_format").GetString() = "pdf"

    let pdfElement =
        rootElement.GetProperty("ti_items").EnumerateArray() |> Seq.find isPdf

    let urlElement =
        pdfElement.GetProperty("ti_storages").EnumerateArray() |> Seq.item 0

    let url = urlElement.GetString()
    let getStringOfElement (element: JsonElement) = element.GetString()

    let tags =
        rootElement.GetProperty("tag_list").EnumerateArray()
        |> Seq.map getStringOfElement
        |> Seq.filter isNotNull

    { Name = name; Url = url; Tags = tags }

//todo a danger of infinite recursion
let rec parseFolders (books: Book seq) =
    let mapFolder tag =
        let removeFirstTag book = { book with Tags = book.Tags |> Seq.skip 1 }
        let filterByTag book = Seq.exists (fun tag -> tag = tag) book.Tags
        let getBooks () = books |> Seq.filter filterByTag |> Seq.map removeFirstTag
        let getChildren () = parseFolders (getBooks())
        { Name = tag; GetChildren = getChildren; GetBooks = getBooks }

    let firstTag book = Seq.head book.Tags
    books |> Seq.map firstTag |> Set.ofSeq |> Seq.map mapFolder
