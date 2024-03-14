using SqliteWasmHelper;

namespace Book.Models
{
    internal sealed class BookSettingRepository(ISqliteWasmDbContextFactory<BookDbContext> db) : IBookSettingRepository
    {
        public async Task<IEnumerable<BookSetting>> GetAllBookSettings()
        {
            using var dbContext = await db.CreateDbContextAsync();

            return [.. dbContext.BookSetting
                .Select(b => new BookSetting
                {
                    BookSettingId = b.BookSettingId,
                    UserAmendable = b.UserAmendable,
                    SettingName = b.SettingName,
                    SettingValue = b.SettingValue
                })
                .Where(b => b.UserAmendable == true)
                .OrderBy(b => b.SettingName)];
        }

        public async Task<BookSetting?> GetBookSettingById(int bookSettingId)
        {
            using var dbContext = await db.CreateDbContextAsync();

            return dbContext.BookSetting
                .Select(b => new BookSetting
                {
                    BookSettingId = b.BookSettingId,
                    UserAmendable = b.UserAmendable,
                    SettingName = b.SettingName,
                    SettingValue = b.SettingValue
                })
                .FirstOrDefault(b => b.BookSettingId == bookSettingId);
        }

        public async Task<BookSetting> AddBookSetting(BookSetting bookSetting)
        {
            using var dbContext = await db.CreateDbContextAsync();
            var addedEntity = dbContext.BookSetting.Add(bookSetting);
            await dbContext.SaveChangesAsync();
            return addedEntity.Entity;
        }

        public async Task<BookSetting?> UpdateBookSetting(BookSetting bookSetting)
        {
            using var dbContext = await db.CreateDbContextAsync();

            var foundBookSetting = dbContext.BookSetting
                .FirstOrDefault(b => b.BookSettingId == bookSetting.BookSettingId);

            if (foundBookSetting != null)
            {
                foundBookSetting.UserAmendable = bookSetting.UserAmendable;
                foundBookSetting.SettingName = bookSetting.SettingName;
                foundBookSetting.SettingValue = bookSetting.SettingValue;

                await dbContext.SaveChangesAsync();

                return foundBookSetting;
            }

            return null;
        }

        public async Task UpdateBookSettings(IEnumerable<BookSetting> bookSettings)
        {
            if (bookSettings == null) return;

            using var dbContext = await db.CreateDbContextAsync();

            foreach (var bookSetting in bookSettings)
            {
                _ = await UpdateBookSetting(bookSetting);
            }

            return;
        }

    }
}
