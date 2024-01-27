using Microsoft.AspNetCore.Components;
using MudBlazor;
using Book.Services;
using Book.Dialogs;

namespace Book.Shared
{
    public partial class MainLayout
    {
        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public BookDbMigratorSvc DbMigrator { get; set; }

        private bool _isDarkMode = true;
        private string themeIcon = Icons.Material.Filled.LightMode;
        private string themeIconText = "Light Mode";

        private string BookName = "Book";

        private MudThemeProvider _mudThemeProvider;

        private bool drawerOpen = false;

        protected async override Task OnInitializedAsync()
        {
            await DbMigrator.EnsureDbCreated();

            BookName = await BookSettingSvc.GetBookName();
            _isDarkMode = await BookSettingSvc.GetDarkMode();

            SetTheme();
        }

        void ToggleDrawer()
        {
            drawerOpen = !drawerOpen;
        }

        private async void ToggleThemeMode()
        {
            _isDarkMode = !_isDarkMode;

            SetTheme();

            await BookSettingSvc.SetDarkMode(_isDarkMode);
        }

        private async Task AddTransaction()
        {
            var parameters = new DialogParameters<TransactionDialog>();
            parameters.Add(x => x.SavedTransactionId, 0);

            DialogService.Show<TransactionDialog>("New Entry", parameters);
        }

        private void SetTheme()
        {
            themeIcon = _isDarkMode ? Icons.Material.Filled.LightMode : Icons.Material.Filled.DarkMode;
            themeIconText = _isDarkMode ? "Light Mode" : "Dark Mode";
        }
    }
}
