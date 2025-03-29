module TextbookDownloaderCliTests.Tests

open System.Collections
open TextbookDownloaderCli.Book
open TextbookDownloaderCli.Folder
open TextbookDownloaderCli.Tools
open Xunit

[<Fact>]
let getBookIdTest() =
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

    let expected: BookInfoWithTag =
        { Textbook = expectedTextbook
          Tags = [| "Tag1"; "Tag2" |] }

    let actual = getBook json
    Assert.Equal(expected, actual)

// [<Fact>]
// let foldersTest () =
//     let books =
//         [ { Textbook =
//               { Name = "Book1"
//                 Url = "https://example.com/book1.pdf" }
//             Tags = [| "Tag1"; "Tag2" |] }
//           { Textbook =
//               { Name = "Book2"
//                 Url = "https://example.com/book2.pdf" }
//             Tags = [| "Tag1"; "Tag3" |] }
//           { Textbook =
//               { Name = "Book3"
//                 Url = "https://example.com/book3.pdf" }
//             Tags = [| "Tag2" |] } ]
//
//     let f2a =
//         { Name = "Tag2"
//           Books =
//             [ { Textbook =
//                   { Name = "Book1"
//                     Url = "https://example.com/book1.pdf" }
//                 Tags = Seq.empty } ]
//           Children = EmptyFolderChildren }
//
//     let f2b =
//               { Name = "Tag3"
//                 Books =
//                   [ { Textbook =
//                         { Name = "Book2"
//                           Url = "https://example.com/book2.pdf" }
//                       Tags = Seq.empty } ]
//                 Children = EmptyFolderChildren }
//
//     let f1a: Folder seq = [|f2a;  f2b |]
//
//     let expectedFolders: IEnumerable<Folder> =
//         [
//           { Name = "Tag1"
//             Books = []
//             Children = FolderChildren( lazy f1a ) }
//           { Name = "Tag2"
//             Books =
//               [ { Textbook =
//                     { Name = "Book3"
//                       Url = "https://example.com/book3.pdf" }
//                   Tags = Seq.empty } ]
//             Children = EmptyFolderChildren } ]
//
//     let actualFolders: IEnumerable<Folder> = ofBooks books
//
//     // You might need to force lazy evaluations before comparing
//     let rec forceFolders folders =
//         folders
//         |> List.map (fun f ->
//             let children =
//                 match f.Children with
//                 | FolderChildren lazyFolders -> FolderChildren(lazy (forceFolders (lazyFolders.Force())))
//                 | EmptyFolderChildren -> EmptyFolderChildren
//
//             { f with Children = children })
//
//     Assert.Equal<IEnumerable<Folder>>(expectedFolders, actualFolders)

[<Fact>]
let promptTest () =
    let folders: Folder list =
        [ { Name = "Folder1"
            Children = EmptyFolderChildren
            Books = Seq.empty }
          { Name = "Folder2"
            Children = EmptyFolderChildren
            Books = Seq.empty } ]

    let expectedPrompt =
        "Select a folder:\n[0]Folder1\n[1]Folder2\n"

    let actualPrompt = prompt folders

    Assert.Equal(expectedPrompt, actualPrompt)