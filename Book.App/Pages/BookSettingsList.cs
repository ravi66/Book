using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SqliteWasmHelper;

namespace Book.Pages
{
    public partial class BookSettingsList
    {
        [Inject] public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public NavigationManager navigationManager { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        private string BookName { get; set; } = "Book";

        public List<BookSetting> BookSettings { get; set; }

        private BookSetting selectedBookSetting { get; set; }

        private BookSetting bookSettingBeforeEdit { get; set; }

        private MudTable<BookSetting> _table { get; set; }

        private bool blockSwitch { get; set; } = false;

        private bool DisableSave { get; set; } = true;

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();

            using var ctx = await Factory.CreateDbContextAsync();
            BookSettings = (await ctx.GetAllBookSettings()).ToList();
        }

        private void BackupItem(object bookSetting)
        {
            bookSettingBeforeEdit = new()
            {
                BookSettingId = ((BookSetting)bookSetting).BookSettingId,
                SettingName = ((BookSetting)bookSetting).SettingName,
                UserAmendable = ((BookSetting)bookSetting).UserAmendable,
                SettingValue = ((BookSetting)bookSetting).SettingValue
            };
        }

        private void ResetItemToOriginalValues(object bookSetting)
        {
            ((BookSetting)bookSetting).BookSettingId = bookSettingBeforeEdit.BookSettingId;
            ((BookSetting)bookSetting).SettingName = bookSettingBeforeEdit.SettingName;
            ((BookSetting)bookSetting).UserAmendable = bookSettingBeforeEdit.UserAmendable;
            ((BookSetting)bookSetting).SettingValue = bookSettingBeforeEdit.SettingValue;

            blockSwitch = false;
            StateHasChanged();
        }

        private async void ItemHasBeenCommitted(object bookSetting)
        {
            DisableSave = false;
            StateHasChanged();
        }

        private async Task Save()
        {
            using var ctx = await Factory.CreateDbContextAsync();

            await ctx.UpdateBookSettings(BookSettings);

            navigationManager.NavigateTo("/", true);
        }
    }
}
