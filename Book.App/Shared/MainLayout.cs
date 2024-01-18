using Microsoft.AspNetCore.Components;
using MudBlazor;
using Book.Services;
using Book.Dialogs;

namespace Book.Shared
{
    public partial class MainLayout
    {
        [Inject] public NavigationManager navigationManager { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        private bool _isDarkMode = true;

        private string themeIcon = Icons.Material.Filled.LightMode;

        private string BookName = "Book";

        private MudThemeProvider _mudThemeProvider;

        private bool drawerOpen = false;

        void ToggleDrawer()
        {
            drawerOpen = !drawerOpen;
        }

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();
            _isDarkMode = await BookSettingSvc.GetDarkMode();

            themeIcon = _isDarkMode ? Icons.Material.Filled.LightMode : Icons.Material.Filled.DarkMode;
        }

        private async void ToggleThemeMode()
        {
            _isDarkMode = !_isDarkMode;

            if (_isDarkMode)
            {
                themeIcon = Icons.Material.Filled.LightMode;
            }
            else
            {
                themeIcon = Icons.Material.Filled.DarkMode;
            }

            await BookSettingSvc.SetDarkMode(_isDarkMode);
        }

        private async Task AddTransaction()
        {
            var parameters = new DialogParameters<TransactionDialog>();
            parameters.Add(x => x.SavedTransactionId, 0);

            DialogService.Show<TransactionDialog>("New Entry", parameters);
        }

    }
}
