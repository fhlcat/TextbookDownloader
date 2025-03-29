module TextbookDownloaderCli.Cli

open System
open TextbookDownloaderCli.Folders

let singleFolderPrompt index (folder: Folder) =
    let endString = if not (Seq.isEmpty folder.Children) then ">" else ""
    $"[{index}]{folder.Name}{endString}"

let prompt (folders: Folder seq) =
    let foldersPrompt = (folders |> Seq.mapi singleFolderPrompt |> String.concat "\n")
    $"Select a folder:\n{foldersPrompt}\n"

let rec askForFolder (folders: Folder seq) : Folder =
    Console.WriteLine(prompt folders)
    let input = Console.ReadLine()

    let mutable result = 0

    if Int32.TryParse(input, &result) then
        Seq.item result folders
    elif Int32.TryParse(input[.. input.Length - 1], &result) then
        let selectedFolder = Seq.item result folders
        let children = selectedFolder.Children
        askForFolder children
    else
        Console.WriteLine "Invalid input, please try again"
        askForFolder folders

let printDownloadingBooks books =
    Console.WriteLine "Downloading books:"

    for book: string in books do
        Console.WriteLine book