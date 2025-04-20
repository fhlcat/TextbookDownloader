module TextbookDownloaderCli.Program

open System
open System.CommandLine
open TextbookDownloaderCli.CliHandlers

let authFilePathArgument =
    Argument<string>("auth-file-path", "Path to a txt file that contains x-nd-auth value in the headers")

let intervalOption =
    let option = Option<int>("--interval", "Time to wait between each download in milliseconds")
    option.AddAlias "-i"
    option.SetDefaultValue 500
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
    command.AddOption intervalOption
    command.AddOption outputPathOption
    command.SetHandler(downloadFolderCommandHandler, authFilePathArgument, intervalOption, outputPathOption)

    command

let private rootCommand =
    let command = RootCommand "Textbook Downloader CLI"
    command.AddCommand(downloadFolderCommand)
    command

Environment.GetCommandLineArgs()[1..] |> rootCommand.Invoke |> exit
