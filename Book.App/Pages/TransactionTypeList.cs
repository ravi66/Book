using Book.Models;
using Book.Components;
using Microsoft.AspNetCore.Components;
using SqliteWasmHelper;
using MudBlazor;
using Book.Services;

namespace Book.Pages
{
    public partial class TransactionTypeList
    {
        [Inject] public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        public List<TransactionType> TransactionTypes { get; set; }

        private List<SummaryType> SummaryTypes { get; set; }

        private SummaryType _SelectedSummaryType { get; set; }


        private TransactionType selectedTransactionType { get; set; }

        private TransactionType transactionTypeBeforeEdit { get; set; }

        private string BookName { get; set; } = "Book";

        private MudTable<TransactionType> _table { get; set; }

        private bool blockSwitch { get; set; } = false;

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();

            using var ctx = await Factory.CreateDbContextAsync();
            SummaryTypes = (await ctx.GetAllSummaryTypes()).ToList();

            await LoadTransactionTypes();
        }

        private async Task LoadTransactionTypes()
        {
            using var ctx = await Factory.CreateDbContextAsync();
            TransactionTypes = (await ctx.GetAllTransactionTypes()).ToList();
        }

        protected async Task ListTransactions(int transactionTypeId, string transactionTypeName)
        {
            _table.SetEditingItem(null);

            var parameters = new DialogParameters<TransListDialog>();
            parameters.Add(x => x.Mode, 3);
            parameters.Add(x => x.Name, transactionTypeName);
            parameters.Add(x => x.TransactionTypeId, transactionTypeId);

            var dialog = DialogService.Show<TransListDialog>("Transaction List", parameters);

            var result = await dialog.Result;

            await LoadTransactionTypes();
            StateHasChanged();
        }

        async Task DeleteTransactionType(int transactionTypeId, string transactionTypeName)
        {
            _table.SetEditingItem(null);

            var parameters = new DialogParameters<ConfirmDialog>();
            parameters.Add(x => x.ConfirmationTitle, $"Delete {transactionTypeName} Transaction Type");
            parameters.Add(x => x.ConfirmationMessage, "Are you sure you want to delete this Transaction Type?");
            parameters.Add(x => x.CancelColorInt, 0);
            parameters.Add(x => x.DoneColorInt, 1);

            var dialog = DialogService.Show<ConfirmDialog>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled && transactionTypeId != 0)
            {
                using var ctx = await Factory.CreateDbContextAsync();

                await ctx.DeleteTransactionType(transactionTypeId);

                await LoadTransactionTypes();
                StateHasChanged();
            }
        }

        private void BackupItem(object transactionType)
        {
            transactionTypeBeforeEdit = new()
            {
                TransactionTypeId = ((TransactionType)transactionType).TransactionTypeId,
                SummaryTypeId = ((TransactionType)transactionType).SummaryTypeId,
                SummaryType = ((TransactionType)transactionType).SummaryType,
                SummaryName = ((TransactionType)transactionType).SummaryName,
                Name = ((TransactionType)transactionType).Name,
                CreateDate = ((TransactionType)transactionType).CreateDate,
            };

            _SelectedSummaryType = SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == ((TransactionType)transactionType).SummaryTypeId);
        }

        private void ResetItemToOriginalValues(object transactionType)
        {
            if (((TransactionType)transactionType).TransactionTypeId != 0)
            {
                ((TransactionType)transactionType).TransactionTypeId = transactionTypeBeforeEdit.TransactionTypeId;
                ((TransactionType)transactionType).SummaryTypeId = transactionTypeBeforeEdit.SummaryTypeId;
                ((TransactionType)transactionType).SummaryType = transactionTypeBeforeEdit.SummaryType;
                ((TransactionType)transactionType).SummaryName = transactionTypeBeforeEdit.SummaryName;
                ((TransactionType)transactionType).Name = transactionTypeBeforeEdit.Name;
                ((TransactionType)transactionType).CreateDate = transactionTypeBeforeEdit.CreateDate;
            }
            else
            {
                TransactionTypes.RemoveAt(0);
            }

            blockSwitch = false;
            StateHasChanged();
        }

        private async void ItemHasBeenCommitted(object transactionType)
        {
            ((TransactionType)transactionType).SummaryTypeId = _SelectedSummaryType.SummaryTypeId;
            ((TransactionType)transactionType).SummaryName = _SelectedSummaryType.Name;

            using var ctx = await Factory.CreateDbContextAsync();

            if (((TransactionType)transactionType).TransactionTypeId == 0)
            {
                await ctx.AddTransactionType((TransactionType)transactionType);
            }
            else
            {
                await ctx.UpdateTransactionType((TransactionType)transactionType);
            }

            blockSwitch = false;
        }

        private async Task AddTransactionType()
        {
            if (_table.IsEditRowSwitchingBlocked) return;

            TransactionType newTransactionType = new TransactionType();
            newTransactionType.Name = String.Empty;
            newTransactionType.CreateDate = DateTime.Today;

            TransactionTypes.Insert(0, newTransactionType);
            await Task.Delay(50);
            _table.SetSelectedItem(newTransactionType);
            _table.SetEditingItem(newTransactionType);
            blockSwitch = true;
        }

        private async Task<IEnumerable<SummaryType>> SummarySearch(string searchValue)
        {
            await Task.Yield();

            if (string.IsNullOrEmpty(searchValue))
            {
                return SummaryTypes;
            }

            return SummaryTypes
                .Where(s => s.Name.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}
