using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SqliteWasmHelper;

namespace Book.Dialogs
{
    public partial class TransactionDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int SavedTransactionId { get; set; }

        [Inject] public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Inject] public MessageSvc MessageSvc { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        public Transaction Transaction { get; set; }

        private List<TransactionType> TransactionTypes { get; set; }

        private TransactionType _SelectedTransactionType {  get; set; }

        private DateTime? _SelectedDate {  get; set; }

        private int OriginalYear { get; set; } = 0;

        private bool validationOk { get; set; }

        void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            using var ctx = await Factory.CreateDbContextAsync();
            TransactionTypes = (await ctx.GetAllTransactionTypes()).ToList();

            if (SavedTransactionId == 0)
            {
                Transaction = new Transaction
                {
                    Value = 0.00M,
                    TransactionTypeId = -1,
                    TransactionDate = DateTime.Today,
                    CreateDate = DateTime.Today,
                    TransactionType = TransactionTypes.FirstOrDefault(tt => tt.TransactionTypeId == -1),
                };
            }
            else
            {
                Transaction = await ctx.GetTransactionById(SavedTransactionId);
                OriginalYear = Transaction.TransactionDate.Year;
            }

            _SelectedTransactionType = TransactionTypes.FirstOrDefault(t => t.TransactionTypeId == Transaction.TransactionTypeId);
            _SelectedDate = Transaction.TransactionDate;
            Transaction.ValueAsString = Transaction.Value.ToString("F2");
        }

        async void Save()
        {
            if (!validationOk) return;

            if (Decimal.TryParse(Transaction.ValueAsString, out decimal value))
                Transaction.Value = value;
            else
                Transaction.Value = 0;

            if (Transaction.Value == 0) Close();

            Transaction.TransactionTypeId = _SelectedTransactionType.TransactionTypeId;
            Transaction.TransactionDate = (DateTime)(_SelectedDate is null ? DateTime.Today : _SelectedDate);

            using var ctx = await Factory.CreateDbContextAsync();

            if (SavedTransactionId == 0)
            {
                await ctx.AddTransaction(Transaction);
            }
            else
            {
                await ctx.UpdateTransaction(Transaction);
            }

            List<int> years = new List<int> { Transaction.TransactionDate.Year };
            if (OriginalYear != 0 && OriginalYear != Transaction.TransactionDate.Year) years.Add(OriginalYear);
            MessageSvc.ChangeTransactions(years);

            MudDialog.Close(DialogResult.Ok(true));
        }

        async Task DeleteTransaction()
        {
            var parameters = new DialogParameters<ConfirmDialog>();
            parameters.Add(x => x.ConfirmationTitle, $"Delete {Transaction.TransactionTypeName} Transaction");
            parameters.Add(x => x.ConfirmationMessage, $"Are you sure you want to delete this transaction for {Transaction.Value:C2}?");
            parameters.Add(x => x.CancelColorInt, 0);
            parameters.Add(x => x.DoneColorInt, 1);

            var dialog = DialogService.Show<ConfirmDialog>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                if (Transaction.TransactionId != 0)
                {
                    using var ctx = await Factory.CreateDbContextAsync();

                    await ctx.DeleteTransaction(Transaction.TransactionId);

                    MessageSvc.ChangeTransactions(new List<int> { Transaction.TransactionDate.Year });

                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    Close();
                }
            }

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

        async Task CopyTransaction()
        {
            var parameters = new DialogParameters<TransCopyDialog>
            {
                { c => c.DialogTitle, $"Copy {Transaction.Value:C2} {_SelectedTransactionType.Name} entry" },
                { c => c.TransactionToCopyId, Transaction.TransactionId }
            };

            DialogService.Show<TransCopyDialog>("Copy", parameters);
        }

    }
}
