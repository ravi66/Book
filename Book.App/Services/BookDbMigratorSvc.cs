using Book.Models;
using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;

namespace Book.Services
{
    public class BookDbMigratorSvc
    {
        private const string CurrentDbVersion = "1.00";

        private BookDbContext _dbContext;
        private readonly ISqliteWasmDbContextFactory<BookDbContext> _dbContextFactory;

        public BookDbMigratorSvc(ISqliteWasmDbContextFactory<BookDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task EnsureDbCreated()
        {
            _dbContext = await _dbContextFactory.CreateDbContextAsync();

            // uncomment the following line to see the CREATE TABLE scripts in the browser
            // console when generating a new database - this is useful when creating new
            // migrations
            //OutputDbScriptToConsole();

            _ = await _dbContext.Database.EnsureCreatedAsync();

            var dbVersionBookSetting = await _dbContext.BookSetting.FirstOrDefaultAsync(x => x.BookSettingId == 7);
            await EnsureDbMigrated(dbVersionBookSetting?.SettingValue);
        }

        private async Task EnsureDbMigrated(string dbVersion)
        {
            if (dbVersion == CurrentDbVersion)
            {
                return;
            }

            if (dbVersion is null)
            {
                await ApplyDbVersion(CurrentDbVersion);
                return;
            }

            // Uncomment the following when there are migrations

            /*
            if (currentDbVersion == "1.00")
            {
                await Migrate_101();
                await Migrate_102();
            }

            if (currentDbVersion == "1.01")
            {
                await Migrate_102();
            }

            etc...

            */

            return;
        }

        private async Task ApplyDbVersion(string dbVersion)
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
    }
}
