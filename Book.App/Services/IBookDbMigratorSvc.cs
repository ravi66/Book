namespace Book.Services
{
    public interface IBookDbMigratorSvc
    {
        public Task<string> EnsureDbCreated();
        public Task<string> EnsureDbMigratedAsync(string dbVersion);
        public Task ApplyDbVersionAsync(string dbVersion);
        public Task DeleteDatabase();
    }
}
