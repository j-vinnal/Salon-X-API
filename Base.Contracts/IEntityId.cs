namespace Base.Contacts;

public interface IEntityId : IEntityId<Guid>
{
}

public interface IEntityId<TKey>
    where TKey : struct, IEquatable<TKey>
{
    TKey Id { get; set; }
}