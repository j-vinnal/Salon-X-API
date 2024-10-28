using Base.Contacts;
using Base.Contracts;
using Base.DAL.EF;
using Base.Tests.Domain;

namespace Base.Tests.DAL;

public class TestEntityRepository : EFBaseRepository<TestEntity, TestEntity, TestDbContext>
{
    public TestEntityRepository(TestDbContext dbContext, IMapper<TestEntity, TestEntity> mapper) : base(dbContext, mapper)
    {
    }
}