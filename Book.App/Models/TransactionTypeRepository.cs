﻿namespace Book.Models
{
    internal sealed class TransactionTypeRepository(IDbContextFactory<BookDbContext> db) : ITransactionTypeRepository
    {
        public async Task<IEnumerable<TransactionType>> GetAllTransactionTypes()
        {
            using var dbContext = await db.CreateDbContextAsync();

            return [.. dbContext.TransactionTypes
                .Select(t => new TransactionType
                {
                    TransactionTypeId = t.TransactionTypeId,
                    SummaryTypeId = t.SummaryTypeId,
                    Name = t.Name,
                    SummaryType = t.SummaryType,
                    CreateDate = t.CreateDate,
                })
                .OrderBy(t => t.Name)];
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
                    TransactionsFound = t.Transactions.Count != 0,
                    SummaryType = t.SummaryType,
                    ChartColour = t.ChartColour,
                })
                .FirstOrDefault(t => t.TransactionTypeId == transactionTypeId);
        }

        public async Task<TransactionType> AddTransactionType(TransactionType transactionType)
        {
            using var dbContext = await db.CreateDbContextAsync();

            transactionType.SummaryType = default!;
            transactionType.CreateDate = DateTime.Now;
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
                foundTransactionType.ChartColour = transactionType.ChartColour;
                foundTransactionType.CreateDate = DateTime.Now;

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

        public async Task<string> GetColour(int transactionTypeId)
        {
            using var dbContext = await db.CreateDbContextAsync();
            return dbContext.TransactionTypes.Where(s => s.TransactionTypeId == transactionTypeId).Select(s => s.ChartColour).FirstOrDefault() ?? Utils.RandomColour();
        }

        public async Task<List<TransactionType>> Export()
        {
            using var dbContext = await db.CreateDbContextAsync();
            return [.. dbContext.TransactionTypes];
        }

        public async Task<DateTime?> GetLastUpdDt()
        {
            using var dbContext = await db.CreateDbContextAsync();
            return dbContext.TransactionTypes.Max(t => (DateTime?)t.CreateDate);
        }

        public async Task<int> GetSummaryTypeId(int transactionTypeId)
        {
            using var dbContext = await db.CreateDbContextAsync();
            return dbContext.TransactionTypes.Where(t => t.TransactionTypeId == transactionTypeId).Select(t => t.SummaryTypeId).FirstOrDefault();
        }
    }
}