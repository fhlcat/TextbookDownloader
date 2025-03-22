using System.CommandLine;
using System.Net.Http.Headers;

namespace TextbookDownloaderCli;

public record struct DownloadOptions(string Auth, int Threads);

public record DownloadEvents(Action<string> DownloadBegin, Action<string> DownloadCompleted);

public static class CommandLineHelper
{
    public static Func<string[], Task> PrepareParseArgsAsync()
    {
        var rootCommand = new RootCommand("Textbook Downloader CLI");

        var downloadFolderCommand = new Command("DownloadFolder");
        downloadFolderCommand.AddAlias("df");
        var authArgument = new Argument<string>("auth", "x-nd-auth in the headers");
        downloadFolderCommand.AddArgument(authArgument);
        var threadsOption = new Option<int>("--threads", "Number of threads to download");
        threadsOption.SetDefaultValue(4);
        downloadFolderCommand.AddOption(threadsOption);
        downloadFolderCommand.SetHandler(
            (auth, threads) =>
                DownloadFolder(new DownloadOptions(auth, threads)),
            authArgument,
            threadsOption
        );

        rootCommand.AddCommand(downloadFolderCommand);

        return async args => await rootCommand.InvokeAsync(args);
    }


    private static void DownloadFolder(DownloadOptions options)
    {
        var chooseFolder = BookDownloadHelper.PrepareDownloadFolder(options);
        var downloadAsync = chooseFolder(ChooseFolderCli);
        downloadAsync(new DownloadEvents(DownloadBegin, DownloadCompleted)).Wait();

        var consoleLock = new Lock();
        var downloadingBooks = new string?[options.Threads];

        return;

        void DownloadBegin(string book)
        {
            lock (downloadingBooks)
            {
                downloadingBooks[Array.IndexOf(downloadingBooks, null)] = book;
            }

            lock (consoleLock)
            {
                PrintDownloadingBooks();
            }
        }

        void DownloadCompleted(string book)
        {
            lock (downloadingBooks)
            {
                downloadingBooks[Array.IndexOf(downloadingBooks, book)] = null;
            }

            lock (consoleLock)
            {
                PrintDownloadingBooks();
            }
        }

        void PrintDownloadingBooks()
        {
            Console.Clear();
            Console.WriteLine("Downloading books:");
            foreach (var book in downloadingBooks)
            {
                Console.WriteLine(book);
            }
        }
    }

    private static async Task DownloadFolderCliAsync(HttpClient client, int threads = DefaultThreads)
    {
        var rootFolder = await BookInfoParser.GetRootFolderAsync(GetStringAsync);
        var folder = PickFolder(rootFolder);

        //todo use a thread pool
        DownloadFolder(folder, GetStreamAsync, DownloadBegin, DownloadCompleted).Wait();

        return;

        async Task<string> GetStringAsync(string url) => await client.GetStringAsync(url);

        async Task<Stream> GetStreamAsync(string url) => await client.GetStreamAsync(url);
    }

    private static void PrintChildrenFolder(IEnumerable<Folder> folders, int startNumber = 0)
    {
        var folderArray = folders.ToArray();
        if (folderArray.Length == 0) return;

        var folder = folderArray[0];
        var line = $"[{startNumber}] {folder.Name}";
        line = folder.GetChildren().Any() ? line + '>' : line;
        Console.WriteLine(line);

        PrintChildrenFolder(folderArray.Skip(1), startNumber + 1);
    }

    private static (int, bool) ChooseFolder(string prompt)
    {
        Console.Write(prompt);
        //todo check for null
        var input = Console.ReadLine();

        if (input![^1] == '>')
        {
            try
            {
                return (int.Parse(input[..^1]), true);
            }
            catch
            {
                Console.WriteLine("Invalid input, please try again.");
                return ChooseFolder(prompt);
            }
        }

        try
        {
            return (int.Parse(input), false);
        }
        catch
        {
            Console.WriteLine("Invalid input, please try again.");
            return ChooseFolder(prompt);
        }
    }

    private static Folder PickFolder(Folder folder)
    {
        Console.WriteLine(folder.Name);
        PrintChildrenFolder(folder.GetChildren());

        var (index, enter) = ChooseFolder("Choose a folder: ");
        return enter ? PickFolder(folder.GetChildren().ElementAt(index)) : folder.GetChildren().ElementAt(index);
    }

    private static async Task DownloadFolder(
        Folder folder,
        Func<string, Task<Stream>> getStreamAsync,
        Action<Book>? downloadBegin = null,
        Action<Book>? downloadCompleted = null,
        IEnumerable<Folder>? parentFolders = null
    )
    {
        var parentFoldersArray = parentFolders is not null ? parentFolders.ToArray() : [];
        foreach (var book in folder.Books)
        {
            downloadBegin?.Invoke(book);

            var parentFolderNames = parentFoldersArray.Select(folder1 => folder1.Name).ToArray();
            var path = Path.Combine([..parentFolderNames, book.Name]);
            //todo load balance
            await using var fileStream = File.Create(path);
            var stream = await getStreamAsync(book.Urls.First());
            await stream.CopyToAsync(fileStream);

            downloadCompleted?.Invoke(book);
        }

        foreach (var childFolder in folder.GetChildren())
        {
            await DownloadFolder(
                childFolder,
                getStreamAsync,
                downloadBegin,
                downloadCompleted,
                parentFoldersArray.Append(folder)
            );
        }
    }
}