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
                                    TransactionTypes = new List<TransactionType>(s.TransactionTypes
                                        .Select(t => new TransactionType
                                        {
                                            TransactionTypeId = t.TransactionTypeId,
                                            SummaryTypeId = t.SummaryTypeId,
                                            Name = t.Name,
                                            CreateDate = t.CreateDate,
                                            TransactionCount = t.Transactions.Count,
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

    }
}
