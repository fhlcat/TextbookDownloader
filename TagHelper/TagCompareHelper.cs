namespace TreeHelper;

public static class TagCompareHelper
{
    public enum TagCompareResult
    {
        Greater,
        Less,
        NotFound
    }

    public static TagCompareResult CompareTags(IReadonlyTreeNode<string> tagTreeRootNode, string tag1, string tag2)
    {
#if DEBUG
        if (tag1 == tag2) throw new InvalidDataException("Tags are equal");
#endif

        var tags = tagTreeRootNode.ToIEnumerableBreadthFirst();
        foreach (var tag in tags)
        {
            if (tag == tag1)
            {
                return TagCompareResult.Greater;
            }
            if (tag == tag2)
            {
                return TagCompareResult.Less;
            }
        }

        return TagCompareResult.NotFound;
    }
}