module TextbookDownloader.SortTags

open System.Text.Json
open TreeHelper

type TagTreeNode(name, children) =
    interface IReadonlyTreeNode<string> with
        member this.Value = name
        member this.Children = children

let private mapTagTree (rootElement: JsonElement) : TagTreeNode =
    ()

let private getTagTree (getString: string -> string) =
    let url = "https://s-file-1.ykt.cbern.com.cn/zxx/ndrs/tags/tch_material_tag.json"
    let jsonString = getString url
    let document = JsonDocument.Parse jsonString
    let rootElement = document.RootElement
    mapTagTree rootElement

let sortTags (getString: string -> string) (tags: string seq) =
    let tagTreeRootNode = getTagTree getString :> IReadonlyTreeNode<string>
    let compare (tag1: string) (tag2: string) =
        match TagCompareHelper.CompareTags(tagTreeRootNode, tag1, tag2) with
        | TagCompareHelper.TagCompareResult.Greater -> 1
        | TagCompareHelper.TagCompareResult.Less -> 0
        | TagCompareHelper.TagCompareResult.NotFound -> -1
        | _ -> System.ArgumentOutOfRangeException() |> raise
    tags |> Seq.sortWith compare