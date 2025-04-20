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

let sortTags (json: string) =
    let document = JsonDocument.Parse json
    let rootElement = document.RootElement
    let tagsArray = toSeq rootElement |> Array.ofSeq |> Array.distinct

    let sort (tags: string seq) =
        tags
        |> Seq.filter (fun tag -> Seq.exists ((=) tag) tagsArray)
        |> Seq.map (fun tag -> (tag, Array.findIndex ((=) tag) tagsArray))
        |> Seq.sortBy snd
        |> Seq.map fst

    fun (book: TextbookInfo) ->
        { book with
            Tags = sort book.Tags |> Array.ofSeq }
