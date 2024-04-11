using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Book.Models
{
    internal class BookDbContext(DbContextOptions<BookDbContext> options, IStringLocalizer<Resources.BookResources> Localizer) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
             * Summaries
             */
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = -1, Order = 99, Name = Localizer["Unknown"], CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 1, Order = 1, Name = Localizer["Food"], CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 2, Order = 2, Name = Localizer["Fun"], CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 3, Order = 3, Name = Localizer["Property"], CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 4, Order = 9, Name = Localizer["Work"], CreateDate = DateTime.Today });

            /*
             * Entry Types
             */

            // Unknown
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = -1, TransactionTypeId = -1,  Name = Localizer["Unknown"], CreateDate = DateTime.Today });

            // Food
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 1, TransactionTypeId = 1, Name = Localizer["Groceries"], CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 1, TransactionTypeId = 2, Name = Localizer["Takeaway"], CreateDate = DateTime.Today });

            // Fun
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 3, Name = Localizer["Alcohol"], CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 4, Name = Localizer["Entertainment"], CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 5, Name = Localizer["Cash"], CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 6, Name = Localizer["Clothing"], CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 7, Name = Localizer["Holiday"], CreateDate = DateTime.Today });

            // Property
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 8, Name = Localizer["Rent"], CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 9, Name = Localizer["Mortgage"], CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 10, Name = Localizer["Utilities"], CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 11, Name = Localizer["Household"], CreateDate = DateTime.Today });

            // Work
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 4, TransactionTypeId = 12, Name = Localizer["Wages"], CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 4, TransactionTypeId = 13, Name = Localizer["WorkExpenses"], CreateDate = DateTime.Today });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SummaryType> SummaryTypes { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BookSetting> BookSetting { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // SQLite does not support expressions of type 'DateTimeOffset' in ORDER BY clauses. Convert the values to a supported type:
            configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetToBinaryConverter>();
            configurationBuilder.Properties<DateTimeOffset?>().HaveConversion<DateTimeOffsetToBinaryConverter>();
        }
    }
}