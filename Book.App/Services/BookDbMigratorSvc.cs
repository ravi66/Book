namespace Book.Services
{
    internal class BookDbMigratorSvc(IDbContextFactory<BookDbContext> dbContextFactory, IStringLocalizer<Resources.BookResources> Localizer) : IBookDbMigratorSvc
    {
        private const string CurrentDbVersion = "2.10";

        private BookDbContext _dbContext;
        private readonly IDbContextFactory<BookDbContext> _dbContextFactory = dbContextFactory;

        public async Task<string> EnsureDbCreated()
        {
            _dbContext = await _dbContextFactory.CreateDbContextAsync();

            _ = await _dbContext.Database.EnsureCreatedAsync();
            var dbVersionBookSetting = await _dbContext.BookSetting.FirstOrDefaultAsync(x => x.BookSettingId == 7);
            await EnsureDbMigratedAsync(dbVersionBookSetting?.SettingValue);

            _ = await _dbContext.Database.ExecuteSqlRawAsync("VACUUM;");
            await _dbContext.SaveChangesAsync();

            return CurrentDbVersion;
        }

        async Task EnsureDbMigratedAsync(string? dbVersion)
        {
            if (dbVersion is null)
            {
                await ApplyDbVersionAsync(CurrentDbVersion);
                return;
            }

            if (dbVersion == CurrentDbVersion) return;

            if (!float.TryParse(dbVersion, out float floatDbVersion)) floatDbVersion = 0.00F;

            await M200(floatDbVersion);
            await M210(floatDbVersion);
            
            await ApplyDbVersionAsync(CurrentDbVersion);
            return;
        }

        async Task M200(float dbVersion)
        {
            if (dbVersion >= 2.00F) return;

            const string M210_1 = @"ALTER TABLE ""SummaryTypes"" ADD ""ChartColour"" STRING;";
            _ = await _dbContext.Database.ExecuteSqlRawAsync(M210_1);

            const string M210_2 = @"ALTER TABLE ""TransactionTypes"" ADD ""ChartColour"" STRING;";
            _ = await _dbContext.Database.ExecuteSqlRawAsync(M210_2);

            return;
        }

        async Task M210(float dbVersion)
        {
            if (dbVersion >= 2.10F) return;

            const string M210_1 = @"UPDATE ""Transactions"" SET ""TransactionTypeId"" = -1 WHERE ""TransactionTypeId"" IS NULL;";
            _ = await _dbContext.Database.ExecuteSqlRawAsync(M210_1);

            const string M210_2 = @"CREATE TABLE ""TransactionsCopy"" ( ""TransactionId"" INTEGER NOT NULL CONSTRAINT ""PK_Transactions"" PRIMARY KEY AUTOINCREMENT, ""TransactionTypeId"" INTEGER NOT NULL, ""TransactionDate"" TEXT NOT NULL, ""Value"" money NOT NULL, ""CreateDate"" TEXT NOT NULL, ""Notes"" TEXT NULL, CONSTRAINT ""FK_Transactions_TransactionTypes_TransactionTypeId"" FOREIGN KEY (""TransactionTypeId"") REFERENCES ""TransactionTypes"" (""TransactionTypeId"") );";
            _ = await _dbContext.Database.ExecuteSqlRawAsync(M210_2);

            const string M210_3 = @"INSERT INTO ""TransactionsCopy"" SELECT * FROM ""Transactions"";";
            _ = await _dbContext.Database.ExecuteSqlRawAsync(M210_3);

            const string M210_4 = @"DROP TABLE ""Transactions"";";
            _ = await _dbContext.Database.ExecuteSqlRawAsync(M210_4);

            const string M210_5 = @"ALTER TABLE ""TransactionsCopy"" RENAME TO ""Transactions"";";
            _ = await _dbContext.Database.ExecuteSqlRawAsync(M210_5);

            return;
        }

        public async Task ApplyDbVersionAsync(string dbVersion)
        {
            var bookSettingDbVersion = _dbContext.BookSetting.SingleOrDefault(x => x.BookSettingId == 7);
            if (bookSettingDbVersion is not null)
            {
                bookSettingDbVersion.SettingName = Localizer["DatabaseVersion"];
                bookSettingDbVersion.SettingValue = dbVersion;
            }
            else
            {
                _dbContext.BookSetting.Add(new BookSetting { BookSettingId = 7, SettingName = Localizer["DatabaseVersion"], UserAmendable = false, SettingValue = dbVersion });
            }
            await _dbContext.SaveChangesAsync();
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
    }
}