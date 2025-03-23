open System
open System.CommandLine
open System.Threading.Tasks
open TextbookDownloaderFsharp.Cli

//todo learn how to isolate side effects

let invokeFromCli downloadFolderCommandHandler (args: string array) =
    let rootCommand = RootCommand "Textbook Downloader CLI"

    let downloadFolderCommand = Command "DownloadFolder"
    downloadFolderCommand.AddAlias "df"

    let authArgument = Argument<string>("auth", "x-nd-auth in the headers")
    downloadFolderCommand.AddArgument authArgument

    let threadsOption = Option<int>("--threads", "Number of threads to download")
    threadsOption.AddAlias "-t"
    threadsOption.SetDefaultValue 4
    downloadFolderCommand.AddOption threadsOption

    let folderOption = Option<string>("--folder", "Directory to download the books")
    folderOption.AddAlias "-f"
    downloadFolderCommand.AddOption folderOption

    downloadFolderCommand.SetHandler(downloadFolderCommandHandler, authArgument, threadsOption, folderOption)

    rootCommand.AddCommand(downloadFolderCommand)

    rootCommand.Invoke(args)

let downloadFolderCommandHandler (auth: string) (threads: int) (folder: string) =
    task { downloadCli auth threads folder |> ignore } :> Task

exit (invokeFromCli (Func<_, _, _, _>(downloadFolderCommandHandler)) (Environment.GetCommandLineArgs()))