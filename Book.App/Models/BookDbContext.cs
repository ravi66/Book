using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
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

        /*
         * SummaryType Start
        */

        public async Task<List<SummaryType>> GetAllSummaryTypes()
        {
            return SummaryTypes
                .Select(s => new SummaryType
                {
                    SummaryTypeId = s.SummaryTypeId,
                    Name = s.Name,
                    Order = s.Order,
                    CreateDate = s.CreateDate,
                    TransactionTypeList = new List<TransactionType>(s.TransactionTypes.OrderBy(t => t.Name).ToList()),
                    Types = new List<int>(s.TransactionTypes.Select(t => t.TransactionTypeId).ToList())
                })
                .OrderBy(s => s.Order)
                .ToList();
        }

        public async Task<SummaryType> GetSummaryTypeById(int summaryTypeId)
        {
            return SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == summaryTypeId);
        }

        public async Task<SummaryType> AddSummaryType(SummaryType summaryType)
        {
            var addedEntity = SummaryTypes.Add(summaryType);
            await SaveChangesAsync();
            return addedEntity.Entity;
        }

        public async Task<SummaryType> UpdateSummaryType(SummaryType summaryType)
        {
            var foundSummaryType = SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == summaryType.SummaryTypeId);

            if (foundSummaryType != null)
            {
                foundSummaryType.Name = summaryType.Name;
                foundSummaryType.Order = summaryType.Order;

                await SaveChangesAsync();

                return foundSummaryType;
            }

            return null;
        }

        public async Task DeleteSummaryType(int summaryTypeId)
        {
            var foundSummaryType = SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == summaryTypeId);
            if (foundSummaryType == null) return;

            SummaryTypes.Remove(foundSummaryType);
            await SaveChangesAsync();
        }

        public async Task<List<SummaryType>> LoadSummary()
        {
            return SummaryTypes
                .Select(s => new SummaryType
                {
                    SummaryTypeId = s.SummaryTypeId,
                    Name = s.Name,
                    Order = s.Order,
                    Types = new List<int>(s.TransactionTypes.Select(t => t.TransactionTypeId).ToList())
                })
                .OrderBy(s => s.Order)
                .AsNoTracking()
                .ToList();
        }

        /**********************************************************************************/

        /*
         * TransactionType Start
        */

        public async Task <IEnumerable<TransactionType>> GetAllTransactionTypes()
        {
            return TransactionTypes
                .Select(t => new TransactionType
                {
                    TransactionTypeId = t.TransactionTypeId,
                    SummaryTypeId = t.SummaryTypeId,
                    Name = t.Name,
                    SummaryName = t.SummaryType.Name,
                    CreateDate = t.CreateDate,
                    //TransactionCount = t.Transactions.Count()
                })
                .OrderBy(t => t.Name)
                .ToList();
        }

        public async Task<TransactionType> GetTransactionTypeById(int transactionTypeId)
        {
            return TransactionTypes
                .Select(t => new TransactionType
                {
                    TransactionTypeId = t.TransactionTypeId,
                    SummaryTypeId = t.SummaryTypeId,
                    Name = t.Name,
                    CreateDate = t.CreateDate,
                    TransactionCount = t.Transactions.Count(),
                    SummaryType = t.SummaryType
                })
                .FirstOrDefault(t => t.TransactionTypeId == transactionTypeId);
        }


        public async Task<TransactionType> AddTransactionType(TransactionType transactionType)
        {
            transactionType.SummaryType = null;
            var addedEntity = TransactionTypes.Add(transactionType);
            await SaveChangesAsync();
            return addedEntity.Entity;
        }

        public async Task<TransactionType> UpdateTransactionType(TransactionType transactionType)
        {
            var foundTransactionType = TransactionTypes
                .FirstOrDefault(t => t.TransactionTypeId == transactionType.TransactionTypeId);

            if (foundTransactionType != null)
            {
                foundTransactionType.Name = transactionType.Name;
                foundTransactionType.SummaryTypeId = transactionType.SummaryTypeId;

                await SaveChangesAsync();

                return foundTransactionType;
            }

            return null;
        }

        public async Task DeleteTransactionType(int transactionTypeId)
        {
            var foundTransactionType = TransactionTypes.FirstOrDefault(t => t.TransactionTypeId == transactionTypeId);
            if (foundTransactionType == null) return;

            TransactionTypes.Remove(foundTransactionType);
            await SaveChangesAsync();
        }

        /***********************************************************************************/

        /*
         * Transaction Start
        */ 

        public async Task<Transaction> GetTransactionById(int transactionId)
        {
            return Transactions
                .Include(t => t.TransactionType)
                .FirstOrDefault(t => t.TransactionId == transactionId);
        }

        public async Task<Transaction> AddTransaction(Transaction transaction)
        {
            transaction.TransactionType = null;
            var addedEntity = Transactions.Add(transaction);
            await SaveChangesAsync();
            return addedEntity.Entity;
        }

        public async Task AddTransactions(IEnumerable<Transaction> transactions)
        {
            if (transactions == null)
                return;

            foreach (var transaction in transactions)
            {
                _ = Transactions.Add(transaction);
                await SaveChangesAsync();
            }

            return;
        }

        public async Task<Transaction> UpdateTransaction(Transaction transaction)
        {
            var foundTransaction = Transactions
                .FirstOrDefault(t => t.TransactionId == transaction.TransactionId);

            if (foundTransaction != null)
            {
                foundTransaction.Value = transaction.Value;
                foundTransaction.TransactionTypeId = transaction.TransactionTypeId;
                foundTransaction.TransactionDate = transaction.TransactionDate;
                foundTransaction.Notes = transaction.Notes;

                await SaveChangesAsync();

                return foundTransaction;
            }

            return null;
        }

        public async Task DeleteTransaction(int transactionId)
        {
            var foundTransaction = Transactions.FirstOrDefault(t => t.TransactionId == transactionId);
            if (foundTransaction == null) return;

            Transactions.Remove(foundTransaction);
            await SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByTypeMonth(List<int>? types, int year, int month)
        {
            var query = Transactions
                    .Select(t => new Transaction
                    {
                        TransactionId = t.TransactionId,
                        TransactionTypeId = t.TransactionTypeId,
                        Value = t.Value,
                        TransactionTypeName = t.TransactionType.Name,
                        TransactionDate = t.TransactionDate,
                        CreateDate = t.CreateDate,
                        Notes = t.Notes
                    });

            if (types.Count > 0)
            {
                query = query.Where(t => types.Contains((int)t.TransactionTypeId));
            }

            DateTime startDate;
            DateTime endDate;

            if (month > 0)
            {
                startDate = new DateTime(year, month, 1);
                endDate = startDate.AddMonths(1);
            }
            else
            {
                startDate = new DateTime(year, 1, 1);
                endDate = new DateTime(year + 1, 1, 1);
            }

            query = query.Where(t => t.TransactionDate >= startDate && t.TransactionDate < endDate);

            return query
                .OrderByDescending(t => t.TransactionDate)
                .ToList();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsBySummary(List<int>? types, DateTime startDate, DateTime endDate)
        {
            var query = Transactions
                    .Select(t => new Transaction
                    {
                        TransactionId = t.TransactionId,
                        TransactionTypeId = t.TransactionTypeId,
                        Value = t.Value,
                        TransactionTypeName = t.TransactionType.Name,
                        TransactionDate = t.TransactionDate,
                        CreateDate = t.CreateDate,
                        Notes = t.Notes
                    });

            if (types != null)
            {
                query = query.Where(t => types.Contains((int)t.TransactionTypeId));
            }

            query = query.Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);

            return query
                .OrderByDescending(t => t.TransactionDate)
                .ToList();
        }

        public async Task <IEnumerable<Transaction>> GetTransactionsByType(int typeId, DateTime startDate, DateTime endDate)
        {
            var query = Transactions
                    .Select(t => new Transaction
                    {
                        TransactionId = t.TransactionId,
                        TransactionTypeId = t.TransactionTypeId,
                        Value = t.Value,
                        TransactionTypeName = t.TransactionType.Name,
                        TransactionDate = t.TransactionDate,
                        CreateDate = t.CreateDate,
                        Notes = t.Notes
                    });

            query = query.Where(t => t.TransactionTypeId == typeId && t.TransactionDate >= startDate && t.TransactionDate <= endDate);

            return query
                .OrderByDescending(t => t.TransactionDate)
                .ToList();
        }

        /***********************************************************************************/

        /*
         * BookSetting Start
        */

        public async Task<IEnumerable<BookSetting>> GetAllBookSettings()
        {
            return BookSetting
                .Select(b => new BookSetting
                {
                    BookSettingId = b.BookSettingId,
                    UserAmendable = b.UserAmendable,
                    SettingName = b.SettingName,
                    SettingValue = b.SettingValue
                })
                .Where(b => b.UserAmendable == true)
                .OrderBy(b => b.SettingName)
                .ToList();
        }

        public async Task<BookSetting> GetBookSettingById(int bookSettingId)
        {
            return BookSetting
                .Select(b => new BookSetting
                {
                    BookSettingId = b.BookSettingId,
                    UserAmendable = b.UserAmendable,
                    SettingName = b.SettingName,
                    SettingValue = b.SettingValue
                })
                .FirstOrDefault(b => b.BookSettingId == bookSettingId);
        }

        public async Task<BookSetting> AddBookSetting(BookSetting bookSetting)
        {
            var addedEntity = BookSetting.Add(bookSetting);
            await SaveChangesAsync();
            return addedEntity.Entity;
        }

        public async Task<BookSetting> UpdateBookSetting(BookSetting bookSetting)
        {
            var foundBookSetting = BookSetting
                .FirstOrDefault(b => b.BookSettingId == bookSetting.BookSettingId);

            if (foundBookSetting != null)
            {
                foundBookSetting.UserAmendable = bookSetting.UserAmendable;
                foundBookSetting.SettingName = bookSetting.SettingName;
                foundBookSetting.SettingValue = bookSetting.SettingValue;

                await SaveChangesAsync();

                return foundBookSetting;
            }

            return null;
        }

        public async Task UpdateBookSettings(IEnumerable<BookSetting> bookSettings)
        {
            if (bookSettings == null)
                return;

            foreach (var bookSetting in bookSettings)
            {
                _ = await UpdateBookSetting(bookSetting);
                await SaveChangesAsync();
            }

            return;
        }

        /***********************************************************************************/

    }
}
