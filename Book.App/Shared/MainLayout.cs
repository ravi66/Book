using Microsoft.AspNetCore.Components;
using MudBlazor;
using Book.Services;

namespace Book.Shared
{
    public partial class MainLayout
    {
        private bool _isDarkMode = true;

        private string themeIcon = Icons.Material.Filled.LightMode;

        private string BookName = "Book";

        private MudThemeProvider _mudThemeProvider;

        [Inject] public NavigationManager navigationManager { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

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

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();
            _isDarkMode = await BookSettingSvc.GetDarkMode();

            themeIcon = _isDarkMode ? Icons.Material.Filled.LightMode : Icons.Material.Filled.DarkMode;
        }

    }
}
