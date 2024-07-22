namespace Book.Services
{
    public interface IBookDbMigratorSvc
    {
        public Task EnsureDbCreated();
        public Task<bool> DeleteDatabase();
    }
}