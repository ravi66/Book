using Blazored.LocalStorage;

namespace Book.Services
{
    internal class InitialiseSvc(ILocalStorageService LocalStorage) : IInitialiseSvc
    {
        public async Task<bool> RefreshRequiredAsync()
        {
            bool versionRefreshed = await LocalStorage.GetItemAsync<bool>($"{Constants.BookVersion}_Refreshed");
            if (!versionRefreshed) await LocalStorage.SetItemAsync<bool>($"{Constants.BookVersion}_Refreshed", true);
            return versionRefreshed;
        }
    }
}
