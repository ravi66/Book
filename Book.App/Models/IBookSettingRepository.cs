namespace Book.Models
{
    interface IBookSettingRepository
    {
        public Task<IEnumerable<BookSetting>> GetAllBookSettings();
        public Task<BookSetting?> GetBookSettingById(int bookSettingId);
        public Task<BookSetting> AddBookSetting(BookSetting bookSetting);
        public Task<BookSetting?> UpdateBookSetting(BookSetting bookSetting);
        public Task UpdateBookSettings(IEnumerable<BookSetting> bookSettings);
    }
}
