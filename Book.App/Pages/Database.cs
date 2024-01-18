using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Forms;
using Book.Dialogs;
using MudBlazor;
using Book.Services;

namespace Book.Pages
{
    public partial class Database
    {
        [Inject] public IJSRuntime jsRuntime { get; set; }

        [Inject] public NavigationManager navigationManager { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

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

        private async void DeleteDatabase()
        {
            var parameters = new DialogParameters<ConfirmDialog>();
            parameters.Add(x => x.ConfirmationTitle, "Delete Database");
            parameters.Add(x => x.ConfirmationMessage, "<h2 style=\"color:Crimson\">WARNING:</h2><h5>Are you sure you want to delete all your saved changes?</h5><h5>If you have not copied your your changes they will be permanently lost</h5>");
            parameters.Add(x => x.CancelColorInt, 0);
            parameters.Add(x => x.DoneColorInt, 1);

            var dialog = DialogService.Show<ConfirmDialog>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                var success = await jsModule.InvokeAsync<bool>("deleteDatabase");
                if (success) navigationManager.NavigateTo("/", true);
                navigationManager.NavigateTo("refresh/Database");
            }
        }

        private async void SetLastBackupDate()
        {
            await BookSettingSvc.SetLastBackupDate(DateTime.Now);
            navigationManager.NavigateTo("refresh/Database");
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
