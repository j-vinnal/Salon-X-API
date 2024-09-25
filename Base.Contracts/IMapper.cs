namespace Base.Contacts;

public interface IMapper<TSource, TDest>
    where TSource : class
    where TDest : class
{
    TSource? Map(TDest? entity);
    TDest? Map(TSource? entity);
}