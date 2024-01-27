using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;

namespace Book.Models
{
    public class SummaryTypeRepository : ISummaryTypeRepository
    {
        private readonly ISqliteWasmDbContextFactory<BookDbContext> _db;

        public SummaryTypeRepository(ISqliteWasmDbContextFactory<BookDbContext> db)
        {
            _db = db;
        }

        public async Task<List<SummaryType>> GetAllSummaryTypes()
        {
            using var dbContext = await _db.CreateDbContextAsync();

            return dbContext.SummaryTypes
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
            using var dbContext = await _db.CreateDbContextAsync();
            return dbContext.SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == summaryTypeId);
        }

        public async Task<SummaryType> AddSummaryType(SummaryType summaryType)
        {
            using var dbContext = await _db.CreateDbContextAsync();
            var addedEntity = dbContext.SummaryTypes.Add(summaryType);
            await dbContext.SaveChangesAsync();
            return addedEntity.Entity;
        }

        public async Task<SummaryType?> UpdateSummaryType(SummaryType summaryType)
        {
            using var dbContext = await _db.CreateDbContextAsync();
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
            using var dbContext = await _db.CreateDbContextAsync();
            var foundSummaryType = dbContext.SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == summaryTypeId);
            if (foundSummaryType == null) return;

            dbContext.SummaryTypes.Remove(foundSummaryType);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<SummaryType>> LoadSummary()
        {
            using var dbContext = await _db.CreateDbContextAsync();
            return dbContext.SummaryTypes
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

    }
}
