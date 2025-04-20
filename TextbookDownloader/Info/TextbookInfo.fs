module TextbookDownloader.Book.TextbookInfo

open System.Text.Json

type TextbookInfo =
    { Name: string
      Url: string
      Tags: string seq }

let tagsAreEmpty (book: TextbookInfo) = Seq.isEmpty book.Tags

let tagsAreNotEmpty = tagsAreEmpty >> not

let parseBooksId (json: string) =
    let document = JsonDocument.Parse json
    let elements = document.RootElement.EnumerateArray()
    let getIdFromElement (element: JsonElement) = element.GetProperty("id").GetString()

    Seq.map getIdFromElement elements |> (isNull >> not |> Seq.filter)

let private getBookName (bookElement: JsonElement) = bookElement

let private formatIsPdf (tiItemElement: JsonElement) =
    tiItemElement.GetProperty("ti_format").GetString() = "pdf"

let private ofJson (json: string) =
    let document = JsonDocument.Parse json
    let rootElement = document.RootElement
    let tagElements = rootElement.GetProperty("tag_list").EnumerateArray()

    let parseBookUrl (bookElement: JsonElement) =
        let pdfTiItemElement =
            bookElement.GetProperty("ti_items").EnumerateArray() |> Seq.find formatIsPdf

        let urlTiStorageElement =
            pdfTiItemElement.GetProperty("ti_storages").EnumerateArray() |> Seq.head

        urlTiStorageElement.GetString()

    let parseTagName (tagElement: JsonElement) =
        tagElement.GetProperty("tag_name").GetString()

    { Name = rootElement.GetProperty("global_title").GetProperty("zh-CN").GetString()
      Url = parseBookUrl rootElement
      Tags = tagElements |> Seq.map parseTagName |> Seq.filter (isNull >> not) }    

/// <summary>
/// Remind that tags are not sorted.
/// </summary>
let tryOfJson (json: string) =
    try
       Some (ofJson json) 
    with _ ->
        None