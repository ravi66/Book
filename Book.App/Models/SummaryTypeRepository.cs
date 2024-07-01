namespace Book.Models
{
    internal sealed class SummaryTypeRepository(IDbContextFactory<BookDbContext> db) : ISummaryTypeRepository
    {
        public async Task<List<SummaryType>> GetAllSummaryTypes()
        {
            using var dbContext = await db.CreateDbContextAsync();

            return
            [
                .. dbContext.SummaryTypes
                                .Select(s => new SummaryType
                                {
                                    SummaryTypeId = s.SummaryTypeId,
                                    Name = s.Name,
                                    Order = s.Order,
                                    CreateDate = s.CreateDate,
                                    ChartColour = s.ChartColour,
                                    TransactionTypes = new List<TransactionType>(s.TransactionTypes
                                        .Select(t => new TransactionType
                                        {
                                            TransactionTypeId = t.TransactionTypeId,
                                            SummaryTypeId = t.SummaryTypeId,
                                            Name = t.Name,
                                            CreateDate = t.CreateDate,
                                            ChartColour = t.ChartColour,
                                            TransactionsFound = t.Transactions.Count != 0,
                                        })
                                        .OrderBy(t => t.Name).ToList()),
                                })
                                .OrderBy(s => s.Order),
            ];
        }

        public async Task<List<SummaryType>> GetAutoCompleteList()
        {
            using var dbContext = await db.CreateDbContextAsync();

            return
            [
                .. dbContext.SummaryTypes
                                .Select(s => new SummaryType
                                {
                                    SummaryTypeId = s.SummaryTypeId,
                                    Name = s.Name,
                                    Order = s.Order,
                                })
                                .OrderBy(s => s.Order)
,
            ];
        }

        public async Task<SummaryType?> GetSummaryTypeById(int summaryTypeId)
        {
            using var dbContext = await db.CreateDbContextAsync();
            return dbContext.SummaryTypes
                .Select(s => new SummaryType
                {
                    SummaryTypeId = s.SummaryTypeId,
                    Name = s.Name,
                    Order = s.Order,
                    CreateDate = s.CreateDate,
                    ChartColour = s.ChartColour,
                    TransactionTypes = new List<TransactionType>(s.TransactionTypes
                        .Select(t => new TransactionType
                        {
                            TransactionTypeId = t.TransactionTypeId,
                        })
                        .ToList()),
                })
                .FirstOrDefault(s => s.SummaryTypeId == summaryTypeId);
        }

        public async Task<SummaryType> AddSummaryType(SummaryType summaryType)
        {
            using var dbContext = await db.CreateDbContextAsync();
            summaryType.CreateDate = DateTime.Now;
            var addedEntity = dbContext.SummaryTypes.Add(summaryType);
            await dbContext.SaveChangesAsync();
            return addedEntity.Entity;
        }

        public async Task<SummaryType?> UpdateSummaryType(SummaryType summaryType)
        {
            using var dbContext = await db.CreateDbContextAsync();
            var foundSummaryType = dbContext.SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == summaryType.SummaryTypeId);

            if (foundSummaryType != null)
            {
                foundSummaryType.Name = summaryType.Name;
                foundSummaryType.Order = summaryType.Order;
                foundSummaryType.ChartColour = summaryType.ChartColour;
                foundSummaryType.CreateDate = DateTime.Now;

                await dbContext.SaveChangesAsync();

                return foundSummaryType;
            }

            return null;
        }

        public async Task DeleteSummaryType(int summaryTypeId)
        {
            using var dbContext = await db.CreateDbContextAsync();
            var foundSummaryType = dbContext.SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == summaryTypeId);
            if (foundSummaryType == null) return;

            dbContext.SummaryTypes.Remove(foundSummaryType);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<SummaryType>> LoadSummary()
        {
            using var dbContext = await db.CreateDbContextAsync();
            return
            [
                .. dbContext.SummaryTypes
                           .Select(s => new SummaryType
                                {
                                    SummaryTypeId = s.SummaryTypeId,
                                    Name = s.Name,
                                    Order = s.Order,
                                    ChartColour = s.ChartColour,
                                    TransactionTypes = new List<TransactionType>(s.TransactionTypes
                                        .Select(t => new TransactionType
                                        {
                                            TransactionTypeId = t.TransactionTypeId,
                                        })
                                        .ToList()),
                                })
                                .OrderBy(s => s.Order)
                                .AsNoTracking(),
            ];
        }

        public async Task<string> GetColour(int summaryTypeId)
        {
            using var dbContext = await db.CreateDbContextAsync();
            return dbContext.SummaryTypes.Where(s => s.SummaryTypeId == summaryTypeId).Select(s => s.ChartColour).FirstOrDefault() ?? Utils.RandomColour();
        }

        public async Task<List<SummaryType>> Export()
        {
            using var dbContext = await db.CreateDbContextAsync();
            return [.. dbContext.SummaryTypes];
        }

        public async Task<DateTime?> GetLastUpdDt()
        {
            using var dbContext = await db.CreateDbContextAsync();
            return dbContext.SummaryTypes.Where(s => s.SummaryTypeId != -1).Max(t => (DateTime?)t.CreateDate);
        }

        public async Task<bool> IsEmptyDb()
        {
            using var dbContext = await db.CreateDbContextAsync();

            if (!dbContext.SummaryTypes.Where(s => s.SummaryTypeId != -1).Any() &&
                !dbContext.TransactionTypes.Where(s => s.TransactionTypeId != -1).Any() &&
                !dbContext.Transactions.Any()) return true;

            return false;
        }

    }
}