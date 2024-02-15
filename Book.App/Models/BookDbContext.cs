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
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 1, Name = "Fun", Order = 1, CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 2, Name = "Life", Order = 2, CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 3, Name = "Work", Order = 3, CreateDate = DateTime.Today });

            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = -1, TransactionTypeId = -1,  Name = "Unknown", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 1, TransactionTypeId = 1, Name = "Cash", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 1, TransactionTypeId = 2, Name = "Beer", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 3, Name = "Groceries", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 4, Name = "Takeaway", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 5, Name = "Rent", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 6, Name = "Council Tax", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 7, Name = "Wages", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 8, Name = "Work Expenses", CreateDate = DateTime.Today });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SummaryType> SummaryTypes { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BookSetting> BookSetting { get; set; }

    }
}
