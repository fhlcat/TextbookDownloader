module TextbookDownloader.SortTags

open System.Collections.Generic
open System.Text.Json
open TextbookDownloader.Book.TextbookInfo

let private toSeq (jsonElement: JsonElement) =
    let queue = Queue<JsonElement>()
    queue.Enqueue jsonElement

    seq {
        while (queue.Count > 0) do
            let element = queue.Dequeue()
            let existTagName, tagName = element.TryGetProperty("tag_name")

            if existTagName then
                yield tagName.GetString()
                
            let hierarchiesElement = element.GetProperty("hierarchies")
            if hierarchiesElement.ValueKind <> JsonValueKind.Null then
                let childrenElement = hierarchiesElement.EnumerateArray() |> Seq.head
                for child in childrenElement.GetProperty("children").EnumerateArray() do
                    queue.Enqueue(child)
    }

let private compareTags (json: string) (tag1: string) (tag2: string) =
    let document = JsonDocument.Parse json
    let rootElement = document.RootElement
    let tagsSeq = toSeq rootElement

    let result = Seq.tryFind (fun tag -> (tag = tag1 || tag = tag2)) tagsSeq

    match result with
    | None -> 0
    | Some value -> if value = tag1 then 1 else -1

let sortTags (json: string) (book: TextbookInfo) =
    let comparer = compareTags json

    { book with
        Tags = Seq.sortWith comparer book.Tags }
