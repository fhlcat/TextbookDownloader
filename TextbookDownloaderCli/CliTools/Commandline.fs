module TextbookDownloaderCli.CliTools.Commandline

open System

let printDownloadingBooks books =
    Console.WriteLine "Downloading books:"

    for book: string in books do
        Console.WriteLine book
