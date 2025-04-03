module TextbookDownloaderCli.Book

open System.Text.Json

let private isNotNull = isNull >> not
let private isNotNullFilter = Seq.filter isNotNull

let getBooksId (json: string) =
    let document = JsonDocument.Parse json
    let elements = document.RootElement.EnumerateArray()
    let getIdFromElement (element: JsonElement) = element.GetProperty("id").GetString()

    Seq.map getIdFromElement elements |> isNotNullFilter

let private getBookName (bookElement: JsonElement) =
    bookElement.GetProperty("global_title").GetProperty("zh-CN").GetString()

let private formatIsPdf (tiItemElement: JsonElement) =
    tiItemElement.GetProperty("ti_format").GetString() = "pdf"

let private getBookUrl (bookElement: JsonElement) =
    let pdfTiItemElement =
        bookElement.GetProperty("ti_items").EnumerateArray() |> Seq.find formatIsPdf

    let urlTiStorageElement =
        pdfTiItemElement.GetProperty("ti_storages").EnumerateArray() |> Seq.head

    urlTiStorageElement.GetString()

let private getTagName (tagElement: JsonElement) =
    tagElement.GetProperty("tag_name").GetString()

type BookInfo = { Name: string; Url: string }

type BookInfoWithTag =
    { Textbook: BookInfo; Tags: string seq }

let getBook (json: string) =
    let document = JsonDocument.Parse json
    let rootElement = document.RootElement
    let tagElements = rootElement.GetProperty("tag_list").EnumerateArray()

    let textbook =
        { Name = getBookName rootElement
          Url = getBookUrl rootElement }

    { Textbook = textbook
      Tags = Seq.map getTagName tagElements |> isNotNullFilter }
