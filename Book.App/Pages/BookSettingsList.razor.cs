using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Pages
{
    public partial class BookSettingsList
    {
        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] internal IBookSettingSvc BookSettingSvc { get; set; }

        [Inject] internal IBookSettingRepository Repo { get; set; }

        private string BookName { get; set; } = "Book";

        public List<BookSetting> BookSettings { get; set; }

        private BookSetting SelectedBookSetting { get; set; }

        private BookSetting BookSettingBeforeEdit { get; set; }

        private bool BlockSwitch { get; set; } = false;

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();
            BookSettings = (await Repo.GetAllBookSettings()).ToList();
        }

        private void BackupItem(object bookSetting)
        {
            BookSettingBeforeEdit = new()
            {
                BookSettingId = ((BookSetting)bookSetting).BookSettingId,
                SettingName = ((BookSetting)bookSetting).SettingName,
                UserAmendable = ((BookSetting)bookSetting).UserAmendable,
                SettingValue = ((BookSetting)bookSetting).SettingValue
            };
        }

        private void ResetItemToOriginalValues(object bookSetting)
        {
            ((BookSetting)bookSetting).BookSettingId = BookSettingBeforeEdit.BookSettingId;
            ((BookSetting)bookSetting).SettingName = BookSettingBeforeEdit.SettingName;
            ((BookSetting)bookSetting).UserAmendable = BookSettingBeforeEdit.UserAmendable;
            ((BookSetting)bookSetting).SettingValue = BookSettingBeforeEdit.SettingValue;

            BlockSwitch = false;
            StateHasChanged();
        }

        private async void ItemHasBeenCommitted(object bookSetting)
        {
            await Repo.UpdateBookSettings(BookSettings);
            NavigationManager.NavigateTo("/", true);
        }
    }
}
