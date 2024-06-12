namespace Book.Services
{
    public interface IBookDbMigratorSvc
    {
        public Task<string> EnsureDbCreated();
        public Task ApplyDbVersionAsync(string dbVersion);
        public Task<bool> DeleteDatabase();
    }
}
