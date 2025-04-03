open System
open TextbookDownloaderCli

let args = Environment.GetCommandLineArgs()[1..]
exit (Cli.invoke args)
