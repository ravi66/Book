using SqliteWasmHelper;

namespace Book.Models
{
    public class TransactionTypeRepository(ISqliteWasmDbContextFactory<BookDbContext> db) : ITransactionTypeRepository
    {
        public async Task<IEnumerable<TransactionType>> GetAllTransactionTypes()
        {
            using var dbContext = await db.CreateDbContextAsync();

            return dbContext.TransactionTypes
                .Select(t => new TransactionType
                {
                    TransactionTypeId = t.TransactionTypeId,
                    SummaryTypeId = t.SummaryTypeId,
                    Name = t.Name,
                    SummaryName = t.SummaryType.Name,
                    CreateDate = t.CreateDate,
                })
                .OrderBy(t => t.Name)
                .ToList();
        }

        public async Task<TransactionType?> GetTransactionTypeById(int transactionTypeId)
        {
            using var dbContext = await db.CreateDbContextAsync();

            return dbContext.TransactionTypes
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
            using var dbContext = await db.CreateDbContextAsync();

            transactionType.SummaryType = null;
            var addedEntity = dbContext.TransactionTypes.Add(transactionType);
            await dbContext.SaveChangesAsync();
            return addedEntity.Entity;
        }

        public async Task<TransactionType?> UpdateTransactionType(TransactionType transactionType)
        {
            using var dbContext = await db.CreateDbContextAsync();

            var foundTransactionType = dbContext.TransactionTypes
                .FirstOrDefault(t => t.TransactionTypeId == transactionType.TransactionTypeId);

            if (foundTransactionType != null)
            {
                foundTransactionType.Name = transactionType.Name;
                foundTransactionType.SummaryTypeId = transactionType.SummaryTypeId;

                await dbContext.SaveChangesAsync();

                return foundTransactionType;
            }

            return null;
        }

        public async Task DeleteTransactionType(int transactionTypeId)
        {
            using var dbContext = await db.CreateDbContextAsync();

            var foundTransactionType = dbContext.TransactionTypes.FirstOrDefault(t => t.TransactionTypeId == transactionTypeId);
            if (foundTransactionType == null) return;

            dbContext.TransactionTypes.Remove(foundTransactionType);
            await dbContext.SaveChangesAsync();
        }

    }
}
