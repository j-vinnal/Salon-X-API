namespace App.Tests;

using Xunit;

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<SharedContext>, IDisposable
{
    private readonly SharedContext _context;

    public DatabaseCollection(SharedContext context)
    {
        _context = context;
    }

    public void Dispose()
    {
        _context.EnsureDeleted();
    }
}