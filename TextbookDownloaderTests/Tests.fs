module TextbookDownloaderCliTests.Tests

open System.Collections
open TextbookDownloaderCli.Book
open TextbookDownloaderCli.Folder
open TextbookDownloaderCli.CliTools
open Xunit

[<Fact>]
let getBookIdTest () =
    let json =
        """[{"id":"1","global_title":{"zh-CN":"Test Book 1"}},{"id":"2","global_title":{"zh-CN":"Test Book 2"}}]"""

    let expected = [| "1"; "2" |]
    let actual = getBooksId json |> Seq.toArray

    Assert.Equal<IEnumerable>(expected, actual)

[<Fact>]
let getBookTest () =
    let json =
        """{"global_title":{"zh-CN":"Test Book"},"ti_items":[{"ti_format":"pdf","ti_storages":["https://example.com/book.pdf"]}],"tag_list":[{"tag_name":"Tag1"},{"tag_name":"Tag2"}]}"""

    let expectedTextbook =
        { Name = "Test Book"
          Url = "https://example.com/book.pdf" }

    let expectedTags = [ "Tag1"; "Tag2" ] |> Seq.ofList

    let expected: BookInfoWithTag =
        { Textbook = expectedTextbook
          Tags = expectedTags }

    let actual = getBook json
    //todo solve this
    Assert.Equal(
        { expected with
            Tags = expected.Tags |> Seq.toList },
        { actual with
            Tags = actual.Tags |> Seq.toList }
    )

[<Fact>]
let promptTest () =
    let folders: Folder list =
        [ { Name = "Folder1"
            Children = EmptyFolderChildren
            Books = Seq.empty }
          { Name = "Folder2"
            Children = EmptyFolderChildren
            Books = Seq.empty } ]

    let expectedPrompt = "Select a folder:\n[0]Folder1\n[1]Folder2\n"

    let actualPrompt = foldersPrompt folders

    Assert.Equal(expectedPrompt, actualPrompt)
