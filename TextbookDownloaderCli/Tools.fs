module TextbookDownloaderCli.Tools

open System
open TextbookDownloaderCli.Folder

let private singleFolderPrompt index (folder: Folder) =
    match folder.Children with
    | EmptyFolderChildren -> $"[{index}]{folder.Name}"
    | FolderChildren _ -> $"[{index}]{folder.Name}>"

let prompt (folders: Folder seq) =
    let foldersPrompt = folders |> Seq.mapi singleFolderPrompt |> String.concat "\n"
    $"Select a folder:\n{foldersPrompt}\n"

let rec askForFolder (folders: Folder seq) : Folder =
    Console.WriteLine(prompt folders)
    let input = Console.ReadLine()

    let mutable result = 0

    if Int32.TryParse(input, &result) then
        Seq.item result folders
    elif Int32.TryParse(input[.. input.Length - 1], &result) then
        let selectedFolder = Seq.item result folders
        match selectedFolder.Children with
        | EmptyFolderChildren ->
            Console.WriteLine "No subfolders available"
            askForFolder folders
        | FolderChildren children ->
            askForFolder children.Value
    else
        Console.WriteLine "Invalid input, please try again"
        askForFolder folders

let printDownloadingBooks books =
    Console.WriteLine "Downloading books:"

    for book: string in books do
        Console.WriteLine book