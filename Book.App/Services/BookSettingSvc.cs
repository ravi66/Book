namespace Book.Services
{
    internal class BookSettingSvc(IBookSettingRepository repo, IStringLocalizer<Resources.BookResources> Localizer) : IBookSettingSvc
    {
        private BookSetting? BookSetting { get; set; }

        public async Task EnsureUserAmendableSettingsCreated()
        {
            _ = await GetBookName();
            _ = await GetStartYear();
            _ = await GetEndYear();
            _ = await GetDbPrefix();
        }

        // Gets

        public async Task<string> GetBookName()
        {
            return await GetSettingValue(1, Localizer["BookName"], true, Localizer["Book"]);
        }

        public async Task<bool> GetDarkMode()
        {
            return await GetSettingValue(2, Localizer["DarkMode"], false, "true") == "true";
        }

        public async Task<int> GetStartYear()
        {
            string settingValue = await GetSettingValue(3, Localizer["StartYear"], true, "2017");

            if (Int32.TryParse(settingValue, out int numValue))
            {
                return numValue;
            }
            else
            {
                await SetSettingValue(3, Localizer["StartYear"], true, "2017");
                return 2017;
            }
        }

        public async Task<int> GetEndYear()
        {
            string endValue = await GetSettingValue(4, Localizer["EndYear"], true, $"{DateTime.Today.Year + 3}");
            int startValue = await GetStartYear();

            if (Int32.TryParse(endValue, out int numValue))
            {
                if (numValue < startValue)
                {
                    numValue = startValue;
                    await SetSettingValue(4, Localizer["EndYear"], true, $"{numValue}");
                }

                return (numValue);
            }
            else
            {
                await SetSettingValue(4, Localizer["EndYear"], true, $"{ DateTime.Today.Year + 3}");
                return DateTime.Today.Year + 3;
            }
        }

        public async Task<string> GetLastBackupDate()
        {
            return await GetSettingValue(5, Localizer["LastBackupDate"], false, Localizer["NoBackupRecorded"]);
        }

        public async Task<string> GetDbPrefix()
        {
            return await GetSettingValue(6, Localizer["BackupPrefix"], true, Localizer["Book"]);
        }

        public async Task<string> GetDbVersion()
        {
            return await GetSettingValue(7, Localizer["DatabaseVersion"], false, "1.0");
        }

        // Sets

        public async Task SetDarkMode(bool isDarkMode)
        {
            await SetSettingValue(2, Localizer["DarkMode"], false, isDarkMode.ToString().ToLowerInvariant());
            return;
        }

        public async Task SetLastBackupDate(DateTime lastBackupDate)
        {
            await SetSettingValue(5, Localizer["LastBackupDate"], false, lastBackupDate.ToString("g"));
            return;
        }

        // Privates

        private async Task<string> GetSettingValue (int settingId, string settingName, bool userAmendable, string defaultValue)
        {
            BookSetting = await repo.GetBookSettingById(settingId);

            if (BookSetting == null)
            {
                BookSetting = new BookSetting
                {
                    BookSettingId = settingId,
                    SettingName = settingName,
                    UserAmendable = userAmendable,
                    SettingValue = defaultValue
                };

                BookSetting = await repo.AddBookSetting(BookSetting);
            }
            else
            {
                if (BookSetting.SettingName != settingName)
                {
                    BookSetting.SettingName = settingName;
                    BookSetting = await repo.UpdateBookSetting(BookSetting);
                }
            }

            return BookSetting.SettingValue;
        }

        private async Task SetSettingValue(int settingId, string settingName, bool userAmendable, string newValue)
        {
            BookSetting = await repo.GetBookSettingById(settingId);

            if (BookSetting == null)
            {
                BookSetting = new BookSetting
                {
                    BookSettingId = settingId,
                    SettingName = settingName,
                    UserAmendable = userAmendable,
                    SettingValue = newValue
                };

                BookSetting = await repo.AddBookSetting(BookSetting);
            }
            else
            {
                BookSetting.SettingValue = newValue;

                BookSetting = await repo.UpdateBookSetting(BookSetting);
            }

            return;
        }
    }
}