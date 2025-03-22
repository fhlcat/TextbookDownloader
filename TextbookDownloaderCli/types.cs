namespace TextbookDownloaderCli;

public record struct Book(string Name, IEnumerable<string> Urls);

public record struct Folder(string Name, Func<IEnumerable<Folder>> GetChildren, IEnumerable<Book> Books);