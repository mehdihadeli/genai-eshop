namespace BuildingBlocks.Types;

public interface IStronglyTypedId<out T>
    where T : notnull
{
    T Value { get; }
}
