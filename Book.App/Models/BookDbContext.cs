using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Book.Models
{
    public class BookDbContext : DbContext
    {
        /// <summary>
        /// FIXME: This is required for EF Core 6.0 as it is not compatible with trimming.
        /// </summary>
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        private static Type _keepDateOnly = typeof(DateOnly);

        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = -1, Name = "Unknown", Order = 9999, CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 1, Name = "Food", Order = 1, CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 2, Name = "Household", Order = 3, CreateDate = DateTime.Today });


            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { TransactionTypeId = -1, SummaryTypeId = -1, Name = "Unknown", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { TransactionTypeId = 1, SummaryTypeId = 1, Name = "Groceries", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { TransactionTypeId = 2, SummaryTypeId = 2, Name = "Rent", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { TransactionTypeId = 3, SummaryTypeId = 2, Name = "Council Tax", CreateDate = DateTime.Today });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SummaryType> SummaryTypes { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BookSetting> BookSetting { get; set; }

    }
}
