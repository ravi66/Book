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

        [Inject] internal BookSettingSvc BookSettingSvc { get; set; }

        [Inject] internal ISummaryTypeRepository Repo { get; set; }

        [Inject] public MessageSvc MessageSvc { get; set; }

        private string BookName { get; set; } = "Book";

        public List<SummaryType> SummaryTypes { get; set; }

        private SummaryType SelectedSummaryType { get; set; }

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();

            MessageSvc.TransactionsChanged += () => TransactionsChanged(MessageSvc.TransactionYears);

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
            if (!(await DialogService.Show<STypeDialog>("Edit Summary Type", new DialogParameters<STypeDialog>{ { x => x.SavedSummaryTypeId, summaryTypeId } }).Result).Canceled) await LoadSummaryTypes();
        }

        private void ShowTransactionTypeBtnPress(int summaryTypeId)
        {
            SummaryType tmpSummaryType = SummaryTypes.First(s => s.SummaryTypeId == summaryTypeId);
            tmpSummaryType.ShowTransactionTypes = !tmpSummaryType.ShowTransactionTypes;
        }

        private async Task AddTransactionType(int summaryTypeId)
        {
            if (!(await DialogService.Show<TTypeDialog>("New Entry Type", new DialogParameters<TTypeDialog>{ { x => x.NewSummaryTypeId, summaryTypeId } }).Result).Canceled) await LoadSummaryTypes();
        }

        protected async Task ListTransactionsTType(TransactionType transactionType)
        {
            DialogService.Show<TransListDialog>("Entries List", new DialogParameters<TransListDialog>{ { x => x.Mode, 3 }, { x => x.Name, transactionType.Name }, { x => x.TransactionTypeId, transactionType.TransactionTypeId } });
        }

        protected async Task EditTType(int transactionTypeId)
        {
            if (!(await DialogService.Show<TTypeDialog>("Edit Entry Type", new DialogParameters<TTypeDialog>{ { x => x.SavedTransactionTypeId, transactionTypeId }, }).Result).Canceled) await LoadSummaryTypes();
        }

        private void TransactionsChanged(List<int> _1)
        {
            // Reload regardless of year
            LoadSummaryTypes();
        }

        public void Dispose()
        {
            MessageSvc.TransactionsChanged -= () => TransactionsChanged(MessageSvc.TransactionYears);
            GC.SuppressFinalize(this);
        }

    }
}
