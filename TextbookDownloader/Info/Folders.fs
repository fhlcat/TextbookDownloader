module TextbookDownloader.Folders

open TextbookDownloader.Book.TextbookInfo

type Folder =
    { Name: string
      Children: FolderChildren
      Books: TextbookInfo seq }

and FolderChildren =
    | FolderChildren of Lazy<Folder seq>
    | EmptyFolderChildren

let rec private tryOfBooksInformationRec (booksInformation: TextbookInfo seq) (tagDepth: int) =
    booksInformation
    |> Seq.filter (fun book -> Seq.length book.Tags > tagDepth)
    |> Seq.groupBy (fun book -> Seq.item tagDepth book.Tags)
    |> Seq.map (fun group ->
        let tag, books = group

        let children =
            if tagDepth = Seq.length (Seq.head books).Tags then
                EmptyFolderChildren
            else
                let children = tryOfBooksInformationRec books (tagDepth + 1)
                if Seq.isEmpty children then
                    EmptyFolderChildren
                else
                    FolderChildren (lazy children)

        { Name = tag
          Children = children
          Books = books })

let ofBooksInfo (booksInformation: TextbookInfo seq) =
    tryOfBooksInformationRec booksInformation 0
