using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.EntityFrameworkCore;
using Book.Components;
using SqliteWasmHelper;
using Book.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace Book.Pages
{
    public partial class Database
    {
        [Inject]
        public IJSRuntime jsRuntime { get; set; }

        private IJSObjectReference? jsModule;

        public string BookDownloadUrl { get; set; } = string.Empty;

        public string BookDbFileName { get; set; }

        [Inject]
        public NavigationManager navigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var dateStamp = DateTime.Now.ToString("yyyyMMddHHmm");
            BookDbFileName = $"book-{dateStamp}.sqlite3";

            jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/database.js");
            BookDownloadUrl = await GetDownloadUrl();
        }

        public async ValueTask<string?> GetDownloadUrl() =>
            jsModule is not null ?
                await jsModule.InvokeAsync<string>("generateDownloadUrl") : null;

        private async void UploadDb(InputFileChangeEventArgs e)
        {
            var fileContent = new byte[e.File.Size];
            await e.File.OpenReadStream().ReadAsync(fileContent);
            await jsModule.InvokeVoidAsync("uploadDatabase", fileContent);
            navigationManager.NavigateTo("/", true);
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (jsModule is not null)
            {
                await jsModule.DisposeAsync();
            }
        }

    }
}
