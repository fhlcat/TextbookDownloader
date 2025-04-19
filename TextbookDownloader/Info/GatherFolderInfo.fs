module TextbookDownloaderCli.GatherFolderInfo

open TextbookDownloader
open TextbookDownloader.DownloadBookInfo

let gatherFolderInfo log auth =
    log "Retrieving books information..."
    let booksInfo = downloadBooksInfo auth
    log "Retrieving books information completed."

    log "Parsing folders information..."
    let rootFolder = Folder.ofBooksInfo booksInfo
    log "Parsing folders information completed."
    ()

    rootFolder
