using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;
using Book.Dialogs;
using MudBlazor;
using Book.Services;
using static MudBlazor.CategoryTypes;

namespace Book.Pages
{
    public partial class Database
    {
        [Inject] public IJSRuntime JsRuntime { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] internal BookSettingSvc BookSettingSvc { get; set; }

        [Inject] HttpClient HttpClient { get; set; }

        public IJSObjectReference? jsModule;

        public string BookDownloadUrl { get; set; } = string.Empty;

        public string BookDbFileName { get; set; }

        private string BookName { get; set; } = "Book";

        public string LastBackupDate { get; set; } = "No backup taken";

        protected override async Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();
            LastBackupDate = await BookSettingSvc.GetLastBackupDate();
            BookDbFileName = $"{await BookSettingSvc.GetDbPrefix()}-{DateTime.Now:yyyyMMddhhmm}.sqlite3";

            jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/database.js");
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
            NavigationManager.NavigateTo("/", true);
        }

        private async void DeleteDatabase()
        {
            var parameters = new DialogParameters<ConfirmDialog>
            {
                { x => x.ConfirmationMessage, "<h2 style=\"color:Crimson\">*** WARNING ***</h2><h3>Are you sure you want to delete all your saved changes?</h3><h4>If you have not backed up your your changes they will be permanently lost</h4>" },
                { x => x.CancelColorInt, 0 },
                { x => x.DoneColorInt, 1 }
            };

            var options = new DialogOptions() { NoHeader = true };

            var dialog = DialogService.Show<ConfirmDialog>("Confirm", parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                var success = await jsModule.InvokeAsync<bool>("deleteDatabase");
                if (success) NavigationManager.NavigateTo("/", true);
                NavigationManager.NavigateTo("refresh/Database");
            }
        }

        private async void SetLastBackupDate()
        {
            await BookSettingSvc.SetLastBackupDate(DateTime.Now);
            NavigationManager.NavigateTo("refresh/Database");
        }

        private async void LoadDemoData()
        {
            using HttpResponseMessage response = await HttpClient.GetAsync("Demo.bin");
            if (response.IsSuccessStatusCode)
            {
                var fileContent = await response.Content.ReadAsByteArrayAsync();
                await jsModule.InvokeVoidAsync("uploadDatabase", fileContent);
                NavigationManager.NavigateTo("/", true);
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (jsModule is not null)
            {
                await jsModule.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }

    }
}
