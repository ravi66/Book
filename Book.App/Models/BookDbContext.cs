using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Book.Models
{
    public class BookDbContext(DbContextOptions<BookDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
             * Summaries
             */
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = -1, Order = 9999, Name = "Unknown", CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 1, Order = 1, Name = "Food", CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 2, Order = 2, Name = "Fun", CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 3, Order = 3, Name = "Property", CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 4, Order = 4, Name = "Personal", CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 5, Order = 5, Name = "Health", CreateDate = DateTime.Today });
            modelBuilder.Entity<SummaryType>().HasData(new SummaryType { SummaryTypeId = 6, Order = 9, Name = "Work", CreateDate = DateTime.Today });

            /*
             * Entry Types
             */

            // Unknown
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = -1, TransactionTypeId = -1,  Name = "Unknown", CreateDate = DateTime.Today });

            // Food
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 1, TransactionTypeId = 1, Name = "Groceries", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 1, TransactionTypeId = 2, Name = "Takeaway", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 1, TransactionTypeId = 3, Name = "Restaurant", CreateDate = DateTime.Today });

            // Fun
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 4, Name = "Alcohol", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 5, Name = "Going Out", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 6, Name = "Games", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 2, TransactionTypeId = 7, Name = "Music", CreateDate = DateTime.Today });

            // Property
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 8, Name = "Rent", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 9, Name = "Council Tax", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 10, Name = "Electricity", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 11, Name = "Gas", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 12, Name = "Broadband", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 13, Name = "Mobile Phone", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 14, Name = "Water Rates", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 15, Name = "TV Licence", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 3, TransactionTypeId = 16, Name = "Household", CreateDate = DateTime.Today });

            // Personal
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 4, TransactionTypeId = 17, Name = "Cash", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 4, TransactionTypeId = 18, Name = "Clothing", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 4, TransactionTypeId = 19, Name = "Holiday", CreateDate = DateTime.Today });

            // Health
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 5, TransactionTypeId = 20, Name = "Prescription", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 5, TransactionTypeId = 21, Name = "Optician", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 5, TransactionTypeId = 22, Name = "Dentist", CreateDate = DateTime.Today });

            // Work
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 6, TransactionTypeId = 23, Name = "Wages", CreateDate = DateTime.Today });
            modelBuilder.Entity<TransactionType>().HasData(new TransactionType { SummaryTypeId = 6, TransactionTypeId = 24, Name = "Work Expenses", CreateDate = DateTime.Today });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SummaryType> SummaryTypes { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BookSetting> BookSetting { get; set; }

    }
}
