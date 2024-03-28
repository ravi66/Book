using Microsoft.Extensions.Localization;

namespace Book.Shared
{
    public partial class MainLayout
    {
        [Inject] internal IBookSettingSvc BookSettingSvc { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public IBookDbMigratorSvc DbMigrator { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] HttpClient HttpClient { get; set; }

        private bool _isDarkMode = true;
        private string themeIcon = Icons.Material.Filled.LightMode;
        private string themeText = string.Empty;

        private string BookName = string.Empty;

        private bool drawerOpen = false;
        private string menuIcon = @Icons.Material.Filled.Menu;
        private string menuToolTip = string.Empty;

        protected async override Task OnInitializedAsync()
        {
            await DbMigrator.EnsureDbCreated();

            BookName = await BookSettingSvc.GetBookName();

            themeText = Localizer["LightMode"];
            menuToolTip = Localizer["OpenMenu"];

            _isDarkMode = await BookSettingSvc.GetDarkMode();
            SetTheme();

            VisitsCounter visitsCounter = new(HttpClient, new Uri(NavigationManager.Uri).Host);
            await visitsCounter.UpdateVisitsCount();
        }

        void ToggleDrawer()
        {
            drawerOpen = !drawerOpen;
            menuIcon = drawerOpen ? Icons.Material.Filled.MenuOpen : Icons.Material.Filled.Menu;
            menuToolTip = drawerOpen ? Localizer["CloseMenu"] : Localizer["OpenMenu"];
        }

        private async void ToggleThemeMode()
        {
            _isDarkMode = !_isDarkMode;
            SetTheme();

            await BookSettingSvc.SetDarkMode(_isDarkMode);
        }

        private void SetTheme()
        {
            themeIcon = _isDarkMode ? Icons.Material.Filled.LightMode : Icons.Material.Filled.DarkMode;
            themeText = _isDarkMode ? Localizer["LightMode"] : Localizer["DarkMode"];
        }
    }
}