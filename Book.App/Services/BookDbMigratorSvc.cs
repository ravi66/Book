using Book.Models;
using Microsoft.EntityFrameworkCore;
using SqliteWasmHelper;

namespace Book.Services
{
    public class BookDbMigratorSvc
    {
        private const string CurrentDbVersion = "1.0";
        private BookDbContext _dbContext;
        private readonly ISqliteWasmDbContextFactory<BookDbContext> _dbContextFactory;

        public BookDbMigratorSvc(ISqliteWasmDbContextFactory<BookDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<string> EnsureDbCreated()
        {
            string savedDbVersion = string.Empty;

            _dbContext = await _dbContextFactory.CreateDbContextAsync();

            // uncomment the following line to see the CREATE TABLE
            // scripts in the browser console when generating a new
            // database - this is useful when creating new migrations
            //OutputDbScriptToConsole();

            _ = await _dbContext.Database.EnsureCreatedAsync();
            BookSetting dbVersionBookSetting = await _dbContext.BookSetting.FirstOrDefaultAsync(x => x.BookSettingId == 7);

            if (dbVersionBookSetting != null)
            {
                savedDbVersion = dbVersionBookSetting.SettingValue;
            }
            
            return await EnsureDbMigratedAsync(savedDbVersion);
        }

        private async Task<string> EnsureDbMigratedAsync(string dbVersion)
        {
            if (dbVersion == CurrentDbVersion)
            {
                return dbVersion;
            }

            if (dbVersion == string.Empty)
            {
                await ApplyDbVersionAsync(CurrentDbVersion);
                return CurrentDbVersion;
            }

            if (dbVersion == "1.0")
            {
                //await Migrate_101_TransactionType();
                dbVersion = CurrentDbVersion;
            }

            return dbVersion;
        }

        private async Task ApplyDbVersionAsync(string dbVersion)
        {
            var bookSettingDbVersion = _dbContext.BookSetting.SingleOrDefault(x => x.BookSettingId == 7);
            if (bookSettingDbVersion is not null)
            {
                bookSettingDbVersion.SettingValue = dbVersion;
            }
            else
            {
                _dbContext.BookSetting.Add(new BookSetting { BookSettingId = 7, SettingName = "[ALL] Database Version", UserAmendable = false, SettingValue = dbVersion });
            }
            await _dbContext.SaveChangesAsync();
        }

        private async Task Migrate_101_TransactionType()
        {
            
            const string Alter_Table_TransactionTypes = @"ALTER TABLE ""TransactionTypes""
                ADD ""Credit"" INTEGER;";

            _ = await _dbContext.Database.ExecuteSqlRawAsync(Alter_Table_TransactionTypes);
            await ApplyDbVersionAsync("1.01");
            
        }

        private void OutputDbScriptToConsole()
        {
            var dbCreationSql = _dbContext.Database.GenerateCreateScript();
            Console.WriteLine("Db Creation SQL:");
            Console.WriteLine(dbCreationSql);
        }
    }
}
