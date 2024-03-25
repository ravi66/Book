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
        private string themeText = "Light Mode";

        private string BookName = Constants.BookName;

        private bool drawerOpen = false;

        protected async override Task OnInitializedAsync()
        {
            await DbMigrator.EnsureDbCreated();

            BookName = await BookSettingSvc.GetBookName();

            _isDarkMode = await BookSettingSvc.GetDarkMode();
            SetTheme();

            VisitsCounter visitsCounter = new(HttpClient, new Uri(NavigationManager.Uri).Host);
            await visitsCounter.UpdateVisitsCount();
        }

        void ToggleDrawer() => drawerOpen = !drawerOpen;

        private async void ToggleThemeMode()
        {
            _isDarkMode = !_isDarkMode;
            SetTheme();

            await BookSettingSvc.SetDarkMode(_isDarkMode);
        }

        private void SetTheme()
        {
            themeIcon = _isDarkMode ? Icons.Material.Filled.LightMode : Icons.Material.Filled.DarkMode;
            themeText = _isDarkMode ? "Light Mode" : "Dark Mode";
        }
    }
}