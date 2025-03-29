module TextbookDownloaderCli.Folder

open TextbookDownloaderCli.Book

type Folder =
    { Name: string
      Children: FolderChildren
      Books: BookInfoWithTag seq }

and FolderChildren =
    | FolderChildren of Lazy<Folder seq>
    | EmptyFolderChildren

let private skipFirstTag book =
    { book with
        Tags = book.Tags |> Seq.skip 1 }

let private getFirstTag book = book.Tags |> Seq.head

let private tagsAreEmpty book = Seq.isEmpty book.Tags
let private tagsAreNotEmpty = tagsAreEmpty >> not

let rec private ofTagAndBook (tagName: string, books_: BookInfoWithTag seq) =
    let textbookWithFirstTagsSkipped = books_ |> Seq.map skipFirstTag

    let booksWithFirstTagSkippedAndTagsNotEmpty =
       textbookWithFirstTagsSkipped |> Seq.filter tagsAreNotEmpty

    let children =
        if Seq.isEmpty booksWithFirstTagSkippedAndTagsNotEmpty then
            EmptyFolderChildren
        else
            let folderChildren = lazy getFolder booksWithFirstTagSkippedAndTagsNotEmpty
            FolderChildren folderChildren

    { Name = tagName
      Children = children
      Books = textbookWithFirstTagsSkipped }

/// assuming that all books have at least one tag
and getFolder (books: BookInfoWithTag seq)=
    if (Seq.exists tagsAreEmpty books) then
        failwith "There are books with empty tags"

    books |> Seq.groupBy getFirstTag |> Seq.map ofTagAndBook