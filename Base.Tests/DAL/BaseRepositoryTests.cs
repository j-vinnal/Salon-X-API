using AutoMapper;
using Base.DAL;
using Base.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Base.Tests.DAL;

public class BaseRepositoryTests
{
    private readonly TestDbContext _ctx;
    private readonly TestEntityRepository _testEntityRepository;
    
    public BaseRepositoryTests()
    {
        // set up mock database - inmemory
        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();

        // use random guid as db instance id
        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        _ctx = new TestDbContext(optionsBuilder.Options);

        // reset db
        _ctx.Database.EnsureDeleted();
        _ctx.Database.EnsureCreated();
        
        // mapper
        var config = new MapperConfiguration(cfg => cfg.CreateMap<TestEntity, TestEntity>());
        var mapper = config.CreateMapper();

        _testEntityRepository =
            new TestEntityRepository(
                _ctx,
                new BaseMapper<TestEntity, TestEntity>(mapper)
            );
    }


    [Fact]
    public async void Test1()
    {
        //Arrange
        _testEntityRepository.Add(new TestEntity() { Value = "Foo" });
        
        await _ctx.SaveChangesAsync();

        // act
        var data = await _testEntityRepository.GetAllAsync();

        // assert
        Assert.Single(data);
    }
}