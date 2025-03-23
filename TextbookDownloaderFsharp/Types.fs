namespace TextbookDownloaderFsharp

type Book =
    { Name: string
      Url: string
      Tags: string seq }

type Folder =
    { Name: string
      GetChildren: unit -> Folder seq
      GetBooks: unit -> Book seq }

type DownloadOptions =
    { Books: Book seq
      Directory: string
      Threads: int }

type DownloadEvents =
    { DownloadStarted: Book -> unit
      DownloadFinished: Book -> unit }
