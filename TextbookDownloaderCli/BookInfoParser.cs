using System.Text.Json;

namespace TextbookDownloaderCli;

using GetStringAsyncDelegate = Func<string, Task<string>>;

public static class BookInfoParser
{
    public static async Task<Folder> GetRootFolderAsync(GetStringAsyncDelegate getStringAsync) =>
        MapFolder(await GetBooksWithSortedTagsAsync(getStringAsync)).First();

    private static IEnumerable<string> ParseBookId(string json) => JsonDocument
        .Parse(json)
        .RootElement
        .EnumerateArray()
        .Select(bookElement => bookElement.GetProperty("id").GetString())
        .Where(id => id is not null)!;

    private static (Book Book, IEnumerable<string> Tags) ParseBookInfo(string json)
    {
        var rootElement = JsonDocument
            .Parse(json)
            .RootElement;
        var name = rootElement
            .GetProperty("global_title")
            .GetProperty("zh-CN")
            .GetString()!;
        var urls = rootElement
            .GetProperty("ti_items")
            .EnumerateArray()
            .First(element => element.GetProperty("ti_format").GetString() == "pdf")
            .GetProperty("ti_storages")
            .EnumerateArray()
            .Select(urlElement => urlElement.GetString())
            .Where(url => url is not null);
        var tags = rootElement
            .GetProperty("tag_list")
            .EnumerateArray()
            .Select(tagElement => tagElement.GetProperty("tag_name").GetString())
            .Where(tag => tag is not null);
        var book = new Book(name, urls!);
        return (book, tags)!;
    }

    private static async Task<IEnumerable<(Book Book, IEnumerable<string> Tags)>> GetBooksWithSortedTagsAsync
    (
        GetStringAsyncDelegate getStringAsync
    )
    {
        var bookUrlJsonTasks = Enumerable
            .Range(1, 1)
            .Select(i => $"https://s-file-2.ykt.cbern.com.cn/zxx/ndrs/resources/tch_material/part_{i}00.json")
            .Select(async url => await getStringAsync(url));
        var bookUrls = await Task.WhenAll(bookUrlJsonTasks);
        var bookIds = bookUrls.SelectMany(ParseBookId);
        var bookInfoJsonTasks = bookIds.Select(async id =>
        {
            var url = $"https://s-file-1.ykt.cbern.com.cn/zxx/ndrv2/resources/tch_material/details/{id}.json";
            return await getStringAsync(url);
        });
        var bookInfoJson = await Task.WhenAll(bookInfoJsonTasks);
        var booksInfo = bookInfoJson.Select(ParseBookInfo);
        //todo order tags

        return booksInfo;
    }

    private static IEnumerable<Folder> MapFolder(IEnumerable<(Book Book, IEnumerable<string> Tags)> booksWithSortedTags)
    {
        return booksWithSortedTags.GroupBy(tuple => tuple.Tags.First()).Select(group =>
            new Folder(
                group.Key,
                () => MapFolder(group.Select(tuple => tuple with { Tags = tuple.Tags.Skip(0) })),
                group.Select(tuple => tuple.Book)
            ));
    }
}