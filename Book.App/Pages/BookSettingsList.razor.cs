using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace Book.Pages
{
    public partial class BookSettingsList
    {
        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] internal IBookSettingSvc BookSettingSvc { get; set; }

        [Inject] internal IBookSettingRepository Repo { get; set; }

        private string BookName { get; set; } = Constants.BookName;

        public List<BookSetting> BookSettings { get; set; }

        private MudTable<BookSetting> Table;

        private readonly BookSettingValidator validator = new();

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();
            await BookSettingSvc.EnsureUserAmendableSettingsCreated();
            BookSettings = (await Repo.GetAllBookSettings()).ToList();
        }

        private async void Save()
        {
            if (!Table.Validator.IsValid) return;

            await Repo.UpdateBookSettings(BookSettings);
            NavigationManager.NavigateTo("/", true);
        }

        private void Back() => NavigationManager.NavigateTo("/", false);
    }
}