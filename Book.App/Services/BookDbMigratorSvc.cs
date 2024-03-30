namespace Book.Services
{
    internal class BookDbMigratorSvc(IDbContextFactory<BookDbContext> dbContextFactory) : IBookDbMigratorSvc
    {
        private const string CurrentDbVersion = "1.00";

        private BookDbContext _dbContext;
        private readonly IDbContextFactory<BookDbContext> _dbContextFactory = dbContextFactory;

        public async Task<string> EnsureDbCreated()
        {
            _dbContext = await _dbContextFactory.CreateDbContextAsync();

            // uncomment the following (and the method) to see the CREATE TABLE scripts in the
            // browser console when generating a new database - this is useful when creating new
            // migrations
            var dbCreationSql = _dbContext.Database.GenerateCreateScript();
            Console.WriteLine("Db Creation SQL:");
            Console.WriteLine(dbCreationSql);

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

            // Uncomment the following (and the methods) when there are migrations

            /*
            if (currentDbVersion == "1.00")
            {
                await Migrate_101();
                await Migrate_102();
                dbVersion = CurrentDbVersion;
            }

            if (currentDbVersion == "1.01")
            {
                await Migrate_102();
                dbVersion = CurrentDbVersion;
            }

            etc...

            */

            return dbVersion;
        }

        public async Task DeleteDatabase()
        {
            _dbContext = await dbContextFactory.CreateDbContextAsync();
            _ = await _dbContext.Database.EnsureDeletedAsync();
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

        /*
        
        private async Task Migrate_101()
        {
            const string Alter_Table_TransactionTypes = @"ALTER TABLE ""TransactionTypes"" ADD ""Credit"" INTEGER;";

            _ = await _dbContext.Database.ExecuteSqlRawAsync(Alter_Table_TransactionTypes);
            await ApplyDbVersion("1.01");
        }

        private void OutputDbScriptToConsole()
        {
            var dbCreationSql = _dbContext.Database.GenerateCreateScript();
            Console.WriteLine("Db Creation SQL:");
            Console.WriteLine(dbCreationSql);
        }

        */
    }
}
