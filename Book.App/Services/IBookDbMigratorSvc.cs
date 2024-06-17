namespace Book.Services
{
    public interface IBookDbMigratorSvc
    {
        public Task EnsureDbCreated();
        public Task ApplyDbVersionAsync(string dbVersion);
        public Task<bool> DeleteDatabase();
    }
}
