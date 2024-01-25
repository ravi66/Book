using Book.Models;
using SqliteWasmHelper;

namespace Book.Services
{
    public class BookSettingSvc
    {
        private BookSetting? BookSetting { get; set; }

        private BookDbContext _dbContext;
        private readonly ISqliteWasmDbContextFactory<BookDbContext> _dbContextFactory;

        public BookSettingSvc(ISqliteWasmDbContextFactory<BookDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        // Gets

        public async Task<string> GetBookName()
        {
            return await GetSettingValue(1, "[ALL] Book Name", true, "Book");
        }

        public async Task<bool> GetDarkMode()
        {
            return await GetSettingValue(2, "[ALL] Dark Mode", false, "true") == "true" ? true : false;
        }

        public async Task<int> GetStartYear()
        {
            string settingValue = await GetSettingValue(3, "[SUMMARY] Start Year", true, "2017");

            if (Int32.TryParse(settingValue, out int numValue))
            {
                return numValue;
            }
            else
            {
                await SetSettingValue(3, "[SUMMARY] Start Year", true, "2017");
                return 2017;
            }
        }

        public async Task<int> GetEndYear()
        {
            string endValue = await GetSettingValue(4, "[SUMMARY] End Year", true, $"{DateTime.Today.Year + 3}");
            int startValue = await GetStartYear();

            if (Int32.TryParse(endValue, out int numValue))
            {
                if (numValue < startValue)
                {
                    numValue = startValue;
                    await SetSettingValue(4, "[SUMMARY] End Year", true, $"{numValue}");
                }

                return (numValue);
            }
            else
            {
                await SetSettingValue(4, "[SUMMARY] End Year", true, $"{DateTime.Today.Year + 3}");
                return DateTime.Today.Year + 3;
            }
        }

        public async Task<string> GetLastBackupDate()
        {
            return await GetSettingValue(5, "[DATABASE] Last backup date", false, "No backup recorded");
        }

        public async Task<string> GetDbPrefix()
        {
            return await GetSettingValue(6, "[DATABASE] Backup Prefix", true, "Book");
        }

        public async Task<string> GetDbVersion()
        {
            return await GetSettingValue(7, "[ALL] Database Version", false, "1.0");
        }

        // Sets

        public async Task SetDarkMode(bool isDarkMode)
        {
            await SetSettingValue(2, "[ALL] Dark Mode", false, isDarkMode.ToString().ToLowerInvariant());
            return;
        }

        public async Task SetLastBackupDate(DateTime lastBackupDate)
        {
            await SetSettingValue(5, "[ALL] Last backup date", false, lastBackupDate.ToString("g"));
            return;
        }

        public async Task SetDbVersion(string version)
        {
            // Always set in BookDbMigratorSvc
            return;
        }

        // Privates

        private async Task<string> GetSettingValue (int settingId, string settingName, bool userAmendable, string defaultValue)
        {
            _dbContext = await _dbContextFactory.CreateDbContextAsync();

            BookSetting = await _dbContext.GetBookSettingById(settingId);

            if (BookSetting == null)
            {
                BookSetting = new BookSetting();
                BookSetting.BookSettingId = settingId;
                BookSetting.SettingName = settingName;
                BookSetting.UserAmendable = userAmendable;
                BookSetting.SettingValue = defaultValue;

                BookSetting = await _dbContext.AddBookSetting(BookSetting);
            }

            return BookSetting.SettingValue;
        }

        private async Task SetSettingValue(int settingId, string settingName, bool userAmendable, string newValue)
        {
            _dbContext = await _dbContextFactory.CreateDbContextAsync();

            BookSetting = await _dbContext.GetBookSettingById(settingId);

            if (BookSetting == null)
            {
                BookSetting = new BookSetting();
                BookSetting.BookSettingId = settingId;
                BookSetting.SettingName = settingName;
                BookSetting.UserAmendable = userAmendable;
                BookSetting.SettingValue = newValue;

                BookSetting = await _dbContext.AddBookSetting(BookSetting);
            }
            else
            {
                BookSetting.SettingValue = newValue;

                BookSetting = await _dbContext.UpdateBookSetting(BookSetting);
            }

            return;
        }

    }
}
