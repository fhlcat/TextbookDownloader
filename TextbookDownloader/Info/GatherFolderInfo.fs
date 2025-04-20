module TextbookDownloaderCli.GatherFolderInfo

open TextbookDownloader
open TextbookDownloader.DownloadBookInfo

let gatherFoldersInfo log auth =
    log "Retrieving books information..."
    let booksInfo = downloadBooksInfo auth
    log "Retrieving books information completed."

    log "Parsing folders information..."
    let folders = Folders.ofBooksInfo booksInfo |> Array.ofSeq
    log "Parsing folders information completed."
    ()

    folders
