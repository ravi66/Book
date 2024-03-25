using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Book.Pages
{
    public partial class Database
    {
        [Inject] public IJSRuntime JsRuntime { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] internal IBookSettingSvc BookSettingSvc { get; set; }

        [Inject] HttpClient HttpClient { get; set; }

        public IJSObjectReference? jsModule;

        public string BookDownloadUrl { get; set; } = string.Empty;

        public string BookDbFileName { get; set; }

        private string BookName { get; set; } = Constants.BookName;

        public string LastBackupDate { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();
            LastBackupDate = await BookSettingSvc.GetLastBackupDate();
            BookDbFileName = $"{await BookSettingSvc.GetDbPrefix()}-{DateTime.Now:yyyyMMddhhmm}.sqlite3";

            jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/database.js");
            BookDownloadUrl = await GetDownloadUrl();
        }

        public async ValueTask<string?> GetDownloadUrl() => jsModule is not null ? await jsModule.InvokeAsync<string>("generateDownloadUrl") : null;

        private async void UploadDb(InputFileChangeEventArgs e)
        {
            var fileContent = new byte[e.File.Size];
            await e.File.OpenReadStream().ReadAsync(fileContent);
            await jsModule.InvokeVoidAsync("uploadDatabase", fileContent);
            NavigationManager.NavigateTo("/", true);
        }

        private async void DeleteDatabase()
        {
            var dialog = DialogService.Show<ConfirmDialog>("", new DialogParameters<ConfirmDialog>
            {
                { x => x.AcceptColour, Color.Error },
                { x => x.AcceptToolTip, "Delete saved changes" },
                { x => x.CancelColour, Color.Success },
                { x => x.Warning, true },
                { x => x.WarningMessage, LastBackupDate != "No backup recorded" ? $"All changes made after {LastBackupDate} will be permanently lost" : "All changes will be permanently lost" },
            });

            if (!(await dialog.Result).Canceled)
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
            var dialog = DialogService.Show<ConfirmDialog>("", new DialogParameters<ConfirmDialog>
            {
                { x => x.AcceptColour, Color.Error },
                { x => x.AcceptToolTip, "Overwrite saved changes with Demonstration data" },
                { x => x.CancelColour, Color.Success },
                { x => x.Warning, true },
                { x => x.WarningMessage, LastBackupDate != "No backup recorded" ? $"All changes made after {LastBackupDate} will be permanently lost" : "All changes will be permanently lost" },
            });

            if (!(await dialog.Result).Canceled)
            {
                using HttpResponseMessage response = await HttpClient.GetAsync("Demo.bin");
                if (response.IsSuccessStatusCode)
                {
                    await jsModule.InvokeVoidAsync("uploadDatabase", (byte[]?)await response.Content.ReadAsByteArrayAsync());
                    NavigationManager.NavigateTo("/", true);
                }
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