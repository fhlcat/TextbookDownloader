namespace TreeHelper;

public interface IReadonlyTreeNode<out T>
{
    public T Value { get; }
    public IReadOnlyCollection<IReadonlyTreeNode<T>> Children { get; }
}