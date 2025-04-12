namespace TreeHelper;

public static class TreeHelper
{
    public static IEnumerable<T> ToIEnumerableBreadthFirst<T>(this IReadonlyTreeNode<T> root)
    {
        var queue = new Queue<IReadonlyTreeNode<T>>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            yield return queue.Dequeue().Value;

            foreach (var child in queue.Dequeue().Children)
            {
                queue.Enqueue(child);
            }
        }
    }
}