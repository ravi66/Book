using System.Runtime.CompilerServices;

namespace Book.Services
{
    public interface IBookSettingSvc
    {
        public Task EnsureUserAmendableSettingsCreated();
        public Task<string> GetBookName();
        public Task<bool> GetDarkMode();
        public Task<int> GetStartYear();
        public Task<int> GetEndYear();
        public Task<string> GetLastBackupDate();
        public Task<string> GetDbPrefix();
        public Task<string> GetDbVersion();
        public Task SetDarkMode(bool isDarkMode);
        public Task SetLastBackupDate(DateTime lastBackupDate);
    }
}
