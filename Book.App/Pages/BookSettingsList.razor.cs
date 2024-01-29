using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Pages
{
    public partial class BookSettingsList
    {
        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public NavigationManager navigationManager { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        [Inject] public BookSettingRepository Repo { get; set; }

        private string BookName { get; set; } = "Book";

        public List<BookSetting> BookSettings { get; set; }

        private BookSetting selectedBookSetting { get; set; }

        private BookSetting bookSettingBeforeEdit { get; set; }

        private MudTable<BookSetting> _table { get; set; }

        private bool blockSwitch { get; set; } = false;

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();
            BookSettings = (await Repo.GetAllBookSettings()).ToList();
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
            await Repo.UpdateBookSettings(BookSettings);
            navigationManager.NavigateTo("/", true);
        }
    }
}
