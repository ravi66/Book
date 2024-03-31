using Blazored.LocalStorage;

namespace Book.Services
{
    internal class InitialiseSvc(ILocalStorageService LocalStorage) : IInitialiseSvc
    {
        public async Task<bool> RefreshRequiredAsync()
        {
            bool versionRefreshed = await LocalStorage.GetItemAsync<bool>($"{Constants.IndexHtmlVersion}_Refreshed");
            if (!versionRefreshed) await LocalStorage.SetItemAsync<bool>($"{Constants.IndexHtmlVersion}_Refreshed", true);
            return versionRefreshed;
        }
    }
}
