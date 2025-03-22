// See https://aka.ms/new-console-template for more information

using TextbookDownloaderCli;

var parseArgsAsync = CommandLineHelper.PrepareParseArgsAsync();
await parseArgsAsync(args);