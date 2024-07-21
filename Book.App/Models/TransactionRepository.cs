namespace Book.Models
{
    internal sealed class TransactionRepository(IDbContextFactory<BookDbContext> db) : ITransactionRepository
    {
        public async Task<Transaction?> GetTransactionById(int transactionId)
        {
            using var dbContext = await db.CreateDbContextAsync();

            return dbContext.Transactions
                .Include(t => t.TransactionType)
                .FirstOrDefault(t => t.TransactionId == transactionId);
        }

        public async Task AddTransaction(Transaction transaction)
        {
            using var dbContext = await db.CreateDbContextAsync();

            transaction.CreateDate = DateTime.Now;
            transaction.TransactionType = null;
            await dbContext.Transactions.AddAsync(transaction);
            await dbContext.SaveChangesAsync();
        }

        public async Task AddTransactions(IEnumerable<Transaction> transactions)
        {
            if (transactions == null) return;

            using var dbContext = await db.CreateDbContextAsync();

            foreach (var transaction in transactions)
            {
                await AddTransaction(transaction);
            }

            return;
        }

        public async Task UpdateTransaction(Transaction transaction)
        {
            using var dbContext = await db.CreateDbContextAsync();

            var foundTransaction = dbContext.Transactions
                .FirstOrDefault(t => t.TransactionId == transaction.TransactionId);

            if (foundTransaction != null)
            {
                foundTransaction.Value = transaction.Value;
                foundTransaction.TransactionTypeId = transaction.TransactionTypeId;
                foundTransaction.TransactionDate = transaction.TransactionDate;
                foundTransaction.Notes = transaction.Notes;
                foundTransaction.CreateDate = DateTime.Now;

                await dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteTransaction(int transactionId)
        {
            using var dbContext = await db.CreateDbContextAsync();

            var foundTransaction = dbContext.Transactions.FirstOrDefault(t => t.TransactionId == transactionId);
            if (foundTransaction == null) return;

            dbContext.Transactions.Remove(foundTransaction);
            await dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByTypeMonth(List<int>? types, int year, int month)
        {
            using var dbContext = await db.CreateDbContextAsync();

            var query = dbContext.Transactions
                    .Select(t => new Transaction
                    {
                        TransactionId = t.TransactionId,
                        TransactionTypeId = t.TransactionTypeId,
                        Value = t.Value,
                        SummaryName = t.TransactionType != null ? t.TransactionType.SummaryType.Name : string.Empty,
                        SummaryTypeId = t.TransactionType != null ? t.TransactionType.SummaryTypeId : 0,
                        Order = t.TransactionType != null ? t.TransactionType.SummaryType.Order : 0,
                        SummaryColour = t.TransactionType != null ? t.TransactionType.SummaryType.ChartColour : string.Empty,
                        TransactionTypeName = t.TransactionType != null ? t.TransactionType.Name : string.Empty,
                        TypeColour = t.TransactionType != null ? t.TransactionType.ChartColour : string.Empty,
                        TransactionDate = t.TransactionDate,
                        CreateDate = t.CreateDate,
                        Notes = t.Notes
                    });

            if (types is not null && types.Count > 0)
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

            return [.. query.OrderByDescending(t => t.TransactionDate)];
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsBySummary(List<int>? types)
        {
            using var dbContext = await db.CreateDbContextAsync();

            var query = dbContext.Transactions
                    .Select(t => new Transaction
                    {
                        TransactionId = t.TransactionId,
                        TransactionTypeId = t.TransactionTypeId,
                        Value = t.Value,
                        SummaryName = t.TransactionType != null ? t.TransactionType.SummaryType.Name : string.Empty,
                        SummaryTypeId = t.TransactionType != null ? t.TransactionType.SummaryTypeId : 0,
                        TransactionTypeName = t.TransactionType != null ? t.TransactionType.Name : string.Empty,
                        TransactionDate = t.TransactionDate,
                        CreateDate = t.CreateDate,
                        Notes = t.Notes
                    });

            if (types != null)
            {
                query = query.Where(t => types.Contains((int)t.TransactionTypeId));
            }

            return [.. query.OrderByDescending(t => t.TransactionDate)];
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByType(int typeId)
        {
            using var dbContext = await db.CreateDbContextAsync();

            var query = dbContext.Transactions
                    .Select(t => new Transaction
                    {
                        TransactionId = t.TransactionId,
                        TransactionTypeId = t.TransactionTypeId,
                        Value = t.Value,
                        TransactionTypeName = t.TransactionType != null ? t.TransactionType.Name : string.Empty,
                        TransactionDate = t.TransactionDate,
                        CreateDate = t.CreateDate,
                        Notes = t.Notes
                    });

            query = query.Where(t => t.TransactionTypeId == typeId);

            return [.. query.OrderByDescending(t => t.TransactionDate)];
        }

        public async Task<List<Transaction>> Export()
        {
            using var dbContext = await db.CreateDbContextAsync();
            return [.. dbContext.Transactions];
        }

        public async Task<DateTime?> GetLastUpdDt()
        {
            using var dbContext = await db.CreateDbContextAsync();
            return dbContext.Transactions.Max(t => (DateTime?)t.CreateDate);
        }

    }
}
