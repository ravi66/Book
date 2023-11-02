using Book.Models;
using Book.Components;
using Microsoft.AspNetCore.Components;
using SqliteWasmHelper;

namespace Book.Pages
{
    public partial class TransactionTypeList
    {
        public IEnumerable<TransactionType> TransactionTypes { get; set; }

        [Inject]
        public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        protected TransactionTypeDialog TransactionTypeDialog { get; set; }

        protected TransListDialog TransListDialog { get; set; }

        protected ConfirmDialog ConfirmDelete { get; set; }
        public int DeleteId { get; set; }

        protected async override Task OnInitializedAsync()
        {
            using var ctx = await Factory.CreateDbContextAsync();
            TransactionTypes = (await ctx.GetAllTransactionTypes()).ToList();

            isSortedAscending = true;
            activeSortColumn = "Name";
        }

        protected void EditTransactionType(int transactionTypeId)
        {
            TransactionTypeDialog.Show(transactionTypeId);
        }

        public async void TransactionTypeDialog_OnDialogClose()
        {
            await OnInitializedAsync();
            StateHasChanged();
        }

        protected void ListTransactions(int transactionTypeId, string transactionTypeName)
        {
            TransListDialog.ShowType(transactionTypeId, transactionTypeName);
        }

        public async void TransListDialog_OnDialogClose()
        {
            await OnInitializedAsync();
            StateHasChanged();
        }

        async Task DeleteTransactionType(int id, string name)
        {
            DeleteId = id;
            ConfirmDelete.Show("Delete Transaction Type", $"Are you sure you want to delete Transaction Type: {name}");
        }

        protected async Task ConfirmDelete_Click(bool deleteConfirmed)
        {
            if (deleteConfirmed && DeleteId != 0)
            {
                using var ctx = await Factory.CreateDbContextAsync();
                await ctx.DeleteTransactionType(DeleteId);
                await ctx.SaveChangesAsync();

                await OnInitializedAsync();
                StateHasChanged();
            }
        }

        private bool isSortedAscending;
        private string activeSortColumn;
        private void SortTable(string columnName)
        {
            if (columnName != activeSortColumn)
            {
                TransactionTypes = TransactionTypes.OrderBy(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                isSortedAscending = true;
                activeSortColumn = columnName;
            }
            else
            {
                if (isSortedAscending)
                {
                    TransactionTypes = TransactionTypes.OrderByDescending(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                }
                else
                {
                    TransactionTypes = TransactionTypes.OrderBy(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                }

                isSortedAscending = !isSortedAscending;
            }
        }

        private string SetSortIcon(string columnName)
        {
            if (activeSortColumn != columnName)
            {
                return string.Empty;
            }
            if (isSortedAscending)
            {
                return "oi-arrow-circle-top";
            }
            else
            {
                return "oi-arrow-circle-bottom";
            }
        }

    }
}
