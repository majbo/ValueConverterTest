using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Xunit;

namespace TestProject1
{
    public class TestContext : DbContext
    {
        private readonly bool _convertPk;

        public TestContext(bool convertPk)
        {
            _convertPk = convertPk;
        }

        public DbSet<SomeEntity> IntegerEntities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("DataSource=:memory:");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (_convertPk)
            {
                var intConverer = new ValueConverter<int, int>(v => v, v => v + 1);
                modelBuilder.Entity<SomeEntity>().Property(p => p.Id).HasConversion(intConverer);
            }

            var dateTimeConverter =
                new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
            modelBuilder.Entity<SomeEntity>().Property(p => p.SomeDate).HasConversion(dateTimeConverter);
        }
    }

    public class SomeEntity
    {
        public int Id { get; set; }
        public DateTime SomeDate { get; set; }
    }

    public class UnitTestClass
    {
        [Fact]
        public void ConvertDateTimeKindTest()
        {
            var testContext = new TestContext(convertPk: false);
            testContext.Database.OpenConnection();
            testContext.Database.EnsureCreated();

            testContext.IntegerEntities.Add(new SomeEntity {Id = 1, SomeDate = DateTime.Now});
            testContext.SaveChanges();

            var first = testContext.IntegerEntities.First();
            Assert.Equal(DateTimeKind.Utc, first.SomeDate.Kind);
        }

        [Fact]
        public void ConvertDateTimeKindAndPkTest()
        {
            var testContext = new TestContext(convertPk: true);
            testContext.Database.OpenConnection();
            testContext.Database.EnsureCreated();

            testContext.IntegerEntities.Add(new SomeEntity {Id = 1, SomeDate = DateTime.Now});
            testContext.SaveChanges();

            var first = testContext.IntegerEntities.First();
            Assert.Equal(DateTimeKind.Utc, first.SomeDate.Kind);
        }
    }
}