namespace TextbookDownloaderCli


module Cli =
    open CilHandlers

    let authArgument =
        Argument<string>("authFile", "a txt file that contains x-nd-auth value in the headers")

    let threadsOption =
        let option = Option<int>("--threads", "Number of threads to download")
        option.AddAlias "-t"
        option.SetDefaultValue 4
        option

    let folderOption =
        let option = Option<string>("--folder", "Directory to download the books")
        option.AddAlias "-f"
        option.SetDefaultValue Environment.CurrentDirectory
        option

    let private downloadFolderCommand =
        let downloadFolderCommand = Command "DownloadFolder"
        downloadFolderCommand.AddAlias "df"
        downloadFolderCommand.AddArgument authArgument
        downloadFolderCommand.AddOption threadsOption
        downloadFolderCommand.AddOption folderOption
        downloadFolderCommand.SetHandler(downloadFolderCommandHandler, authArgument, threadsOption, folderOption)

        downloadFolderCommand

    let private rootCommand =
        let rootCommand = RootCommand "Textbook Downloader CLI"
        rootCommand.AddCommand(downloadFolderCommand)
        rootCommand

    Environment.GetCommandLineArgs()[1..] |> rootCommand.Invoke |> exit
