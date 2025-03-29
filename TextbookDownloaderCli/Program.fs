open System
open TextbookDownloaderCli.Cli

let args = Environment.GetCommandLineArgs()[1..]
exit (invoke args)