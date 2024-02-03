using Book.Dialogs;
using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Pages
{
    public partial class SummaryTypeList
    {
        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        [Inject] public SummaryTypeRepository Repo { get; set; }

        private string BookName { get; set; } = "Book";

        public List<SummaryType> SummaryTypes { get; set; }

        private SummaryType selectedSummaryType { get; set; }

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();

            await LoadSummaryTypes();
        }

        public async Task LoadSummaryTypes()
        {
            SummaryTypes = await Repo.GetAllSummaryTypes();
            StateHasChanged();
        }

        protected async void ListTransactionsSummary(SummaryType summary)
        {
            string typesString = (summary.Types != null && summary.Types.Count > 0) ? typesString = string.Join(",", summary.Types) : String.Empty;

            var parameters = new DialogParameters<TransListDialog>
            {
                { x => x.Mode, 2 },
                { x => x.Name, summary.Name },
                { x => x.TypesString, typesString },
                { x => x.SummaryTypeId, summary.SummaryTypeId }
            };

            DialogService.Show<TransListDialog>("Transaction List", parameters);
        }

        private async Task AddSummaryType()
        {
            var dialog = DialogService.Show<STypeDialog>("New Summary Type");
            var result = await dialog.Result;

            if (!result.Canceled) await LoadSummaryTypes();
        }

        private async Task EditSType(int summaryTypeId)
        {
            var parameters = new DialogParameters<STypeDialog>
            {
                { x => x.SavedSummaryTypeId, summaryTypeId }
            };

            var dialog = DialogService.Show<STypeDialog>("Edit Summary Type", parameters);
            var result = await dialog.Result;

            if (!result.Canceled) await LoadSummaryTypes();
        }

        private void ShowTransactionTypeBtnPress(int summaryTypeId)
        {
            SummaryType tmpSummaryType = SummaryTypes.First(s => s.SummaryTypeId == summaryTypeId);
            tmpSummaryType.ShowTransactionTypes = !tmpSummaryType.ShowTransactionTypes;
        }

        private async Task AddTransactionType(int summaryTypeId)
        {
            var parameters = new DialogParameters<TTypeDialog>
            {
                { x => x.NewSummaryTypeId, summaryTypeId }
            };

            var dialog = DialogService.Show<TTypeDialog>("New Entry Type", parameters);
            var result = await dialog.Result;

            if (!result.Canceled) await LoadSummaryTypes();
        }

        protected async Task ListTransactionsTType(TransactionType transactionType)
        {
            var parameters = new DialogParameters<TransListDialog>
            {
                { x => x.Mode, 3 },
                { x => x.Name, transactionType.Name },
                { x => x.TransactionTypeId, transactionType.TransactionTypeId }
            };

            DialogService.Show<TransListDialog>("Entries List", parameters);
        }

        protected async Task EditTType(int transactionTypeId)
        {
            var parameters = new DialogParameters<TTypeDialog>
            {
                { x => x.SavedTransactionTypeId, transactionTypeId },
            };

            var dialog = DialogService.Show<TTypeDialog>("Edit Entry Type", parameters);
            var result = await dialog.Result;

            if (!result.Canceled) await LoadSummaryTypes();
        }
    }
}
