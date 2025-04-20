module TextbookDownloader.Folders

open TextbookDownloader.Book.TextbookInfo

type Folder =
    { Name: string
      Children: FolderChildren
      Books: TextbookInfo seq }

and FolderChildren =
    | FolderChildren of Lazy<Folder seq>
    | EmptyFolderChildren

let rec private ofBooksInformationRec (booksInformation: TextbookInfo seq) (tagDepth: int) =
    booksInformation
    |> Seq.groupBy (fun book -> Seq.item tagDepth book.Tags)
    |> Seq.map (fun group ->
        let tag, books = group

        let children =
            if tagDepth = Seq.length (Seq.head books).Tags then
                EmptyFolderChildren
            else
                FolderChildren(lazy ofBooksInformationRec books (tagDepth + 1))

        { Name = tag
          Children = children
          Books = books })

let ofBooksInfo (booksInformation: TextbookInfo seq) =
    ofBooksInformationRec booksInformation 0
