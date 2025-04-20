module TextbookDownloaderCli.CliTools.Folder

open System
open System.Text.RegularExpressions
open TextbookDownloader.Folders

let private folderPrompt index (folder: Folder) =
    match folder.Children with
    | EmptyFolderChildren -> $"[{index}]{folder.Name}"
    | FolderChildren _ -> $"[{index}]{folder.Name}>"

let private foldersPrompt (folders: Folder seq) =
    let foldersPromptLines = Seq.mapi folderPrompt folders
    let foldersPrompt = String.concat "\n" foldersPromptLines
    let longestLineLength = foldersPromptLines |> Seq.map String.length |> Seq.max
    let divider = String.replicate longestLineLength "-"
    $"{divider}\n{foldersPrompt}\nSelect a folder:"

let rec askForFolder (folders: Folder seq) : Folder =
    Console.WriteLine(foldersPrompt folders)
    let input = Console.ReadLine()

    let isDirectParseSuccessful, directParseResult = Int32.TryParse(input)

    if isDirectParseSuccessful then
        Seq.item directParseResult folders
    else
        let matchObject = Regex.Match(input, @"^(\d+)>$")
        if matchObject.Success then
            let selectedFolderIndex = Int32.Parse(matchObject.Groups[1].Value)
            let selectedFolder = Seq.item selectedFolderIndex folders

            match selectedFolder.Children with
            | EmptyFolderChildren ->
                Console.WriteLine "No subfolders available"
                askForFolder folders
            | FolderChildren children -> askForFolder children.Value
        else
            Console.WriteLine "Invalid input, please try again"
            askForFolder folders
