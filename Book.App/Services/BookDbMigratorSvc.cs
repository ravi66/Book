namespace Book.Services
{
    internal class BookDbMigratorSvc(IDbContextFactory<BookDbContext> dbContextFactory) : IBookDbMigratorSvc
    {
        private const string CurrentDbVersion = "2.00";

        private BookDbContext _dbContext;
        private readonly IDbContextFactory<BookDbContext> _dbContextFactory = dbContextFactory;

        public async Task<string> EnsureDbCreated()
        {
            _dbContext = await _dbContextFactory.CreateDbContextAsync();

            _ = await _dbContext.Database.EnsureCreatedAsync();
            var dbVersionBookSetting = await _dbContext.BookSetting.FirstOrDefaultAsync(x => x.BookSettingId == 7);
            var dbVersion = await EnsureDbMigratedAsync(dbVersionBookSetting?.SettingValue);

            _ = await _dbContext.Database.ExecuteSqlRawAsync("VACUUM;");
            await _dbContext.SaveChangesAsync();

            return dbVersion;
        }

        public async Task<string> EnsureDbMigratedAsync(string dbVersion)
        {
            if (dbVersion == CurrentDbVersion)
            {
                return dbVersion;
            }

            if (dbVersion is null)
            {
                await ApplyDbVersionAsync(CurrentDbVersion);
                return CurrentDbVersion;
            }

            if (CurrentDbVersion == "2.00")
            {
                await M200();
                dbVersion = CurrentDbVersion;
            }

            return dbVersion;
        }

        public async Task<bool> DeleteDatabase()
        {
            _dbContext = await dbContextFactory.CreateDbContextAsync();

            if (await _dbContext.Database.EnsureDeletedAsync())
            {
                if (await _dbContext.Database.EnsureCreatedAsync()) return true;
            }

            return false;
        }

        public async Task ApplyDbVersionAsync(string dbVersion)
        {
            var bookSettingDbVersion = _dbContext.BookSetting.SingleOrDefault(x => x.BookSettingId == 7);
            if (bookSettingDbVersion is not null)
            {
                bookSettingDbVersion.SettingValue = dbVersion;
            }
            else
            {
                _dbContext.BookSetting.Add(new BookSetting { BookSettingId = 7, SettingName = "[DATABASE] Database Version", UserAmendable = false, SettingValue = dbVersion });
            }
            await _dbContext.SaveChangesAsync();
        }

        private async Task M200()
        {
            const string M200_1 = @"ALTER TABLE ""SummaryTypes"" ADD ""ChartColour"" STRING;";
            _ = await _dbContext.Database.ExecuteSqlRawAsync(M200_1);

            const string M200_2 = @"ALTER TABLE ""TransactionTypes"" ADD ""ChartColour"" STRING;";
            _ = await _dbContext.Database.ExecuteSqlRawAsync(M200_2);

            await ApplyDbVersionAsync("2.00");
        }
    }
}