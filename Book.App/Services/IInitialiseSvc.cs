namespace Book.Services
{
    public interface IInitialiseSvc
    {
        public Task<bool> RefreshRequiredAsync();
    }
}
