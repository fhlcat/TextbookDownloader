module TextbookDownloaderCli.Program

open System
open System.CommandLine
open TextbookDownloaderCli.CliHandlers

let authFilePathArgument =
    Argument<string>("auth-file-path", "Path to a txt file that contains x-nd-auth value in the headers")

let threadsOption =
    let option = Option<int>("--threads", "Number of threads to download")
    option.AddAlias "-t"
    option.SetDefaultValue 4
    option

let outputPathOption =
    let option = Option<string>("--output-path", "Path to the output folder")
    option.AddAlias "-o"
    option.SetDefaultValue Environment.CurrentDirectory
    option

let private downloadFolderCommand =
    let command = Command "DownloadFolder"
    command.AddAlias "df"
    command.AddArgument authFilePathArgument
    command.AddOption threadsOption
    command.AddOption outputPathOption
    command.SetHandler(downloadFolderCommandHandler, authFilePathArgument, threadsOption, outputPathOption)

    command

let private rootCommand =
    let command = RootCommand "Textbook Downloader CLI"
    command.AddCommand(downloadFolderCommand)
    command

Environment.GetCommandLineArgs()[1..] |> rootCommand.Invoke |> exit
