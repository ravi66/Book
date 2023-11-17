using Book.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SqliteWasmHelper;

namespace Book.Components
{
    public partial class TransactionDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int SavedTransactionId { get; set; }

        [Inject] public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        public Transaction Transaction { get; set; }

        private List<TransactionType> TransactionTypes { get; set; }

        private TransactionType _SelectedTransactionType {  get; set; }

        private DateTime? _SelectedDate {  get; set; }

        void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            using var ctx = await Factory.CreateDbContextAsync();
            TransactionTypes = (await ctx.GetAllTransactionTypes()).ToList();

            await ResetDialogAsync();
            StateHasChanged();
        }

        private async Task ResetDialogAsync()
        {
            if (SavedTransactionId == 0)
            {
                Transaction = new Transaction { Value = 0.00M, TransactionTypeId = -1, TransactionDate = DateTime.Today, CreateDate = DateTime.Now };
            }
            else
            {
                using var ctx = await Factory.CreateDbContextAsync();
                Transaction = await ctx.GetTransactionById(SavedTransactionId);
            }

            _SelectedTransactionType = TransactionTypes.FirstOrDefault(t => t.TransactionTypeId == Transaction.TransactionTypeId);
            _SelectedDate = Transaction.TransactionDate;
            Transaction.ValueAsString = Transaction.Value.ToString("F2");
        }

        async void HandleValidSubmit()
        {
            decimal value;

            if (Decimal.TryParse(Transaction.ValueAsString, out value))
                Transaction.Value = value;
            else
                Transaction.Value = 0;

            if (Transaction.Value == 0) Close();

            Transaction.TransactionTypeId = _SelectedTransactionType.TransactionTypeId;
            if (_SelectedDate != null) Transaction.TransactionDate = (DateTime)_SelectedDate;

            using var ctx = await Factory.CreateDbContextAsync();

            if (SavedTransactionId == 0)
            {
                await ctx.AddTransaction(Transaction);
            }
            else
            {
                await ctx.UpdateTransaction(Transaction);
            }

            MudDialog.Close(DialogResult.Ok(true));
        }

        private async Task<IEnumerable<TransactionType>> TypeSearch(string searchValue)
        {
            await Task.Yield();

            if (string.IsNullOrEmpty(searchValue))
            {
                return TransactionTypes;
            }

            return TransactionTypes
                .Where(t => t.Name.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase));
        }

    }
}
