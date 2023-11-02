using Book.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SqliteWasmHelper;

namespace Book.Components
{
    public partial class TransactionDialog
    {
        public Transaction Transaction { get; set; }

        private bool FocusApplied { get; set; }
        private InputText inputTextForFocus;

        [Inject]
        public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        public bool ShowDialog { get; set; }

        [Parameter]
        public EventCallback<bool> CloseEventCallback { get; set; }

        public int SavedTransactionId { get; set; }

        public List<TransactionType> TransactionTypeSL { get; set; } = new List<TransactionType>();

        public async Task Show(int transactionId)
        {
            SavedTransactionId = transactionId;

            FocusApplied = false;

            using var ctx = await Factory.CreateDbContextAsync();
            TransactionTypeSL = (await ctx.GetTransactionTypeSL()).ToList();

            await ResetDialogAsync();
            ShowDialog = true;
            StateHasChanged();
        }

        public void Close()
        {
            ShowDialog = false;
            StateHasChanged();
        }

        private async Task ResetDialogAsync()
        {
            if (SavedTransactionId == 0)
            {
                Transaction = new Transaction { Value = 0.00M, TransactionTypeId = -1, TransactionDate = DateTime.Now, CreateDate = DateTime.Now };
            }
            else
            {
                using var ctx = await Factory.CreateDbContextAsync();
                Transaction = await ctx.GetTransactionById(SavedTransactionId);
            }

            Transaction.ValueAsString = Transaction.Value.ToString("F2");
        }

        protected async Task HandleValidSubmit()
        {
            decimal value;

            if (Decimal.TryParse(Transaction.ValueAsString, out value))
                Transaction.Value = value;
            else
                Transaction.Value = 0;

            if (Transaction.Value == 0)
                Close();

            using var ctx = await Factory.CreateDbContextAsync();

            if (SavedTransactionId == 0)
            {
                await ctx.AddTransaction(Transaction);
            }
            else
            {
                await ctx.UpdateTransaction(Transaction);
            }
            
            await ctx.SaveChangesAsync();

            ShowDialog = false;
            await CloseEventCallback.InvokeAsync(true);
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            /*
             * Only set focus to Value once
            */
            if (ShowDialog && inputTextForFocus.Element != null && !FocusApplied)
            {
                FocusApplied = true;
                await inputTextForFocus.Element.Value.FocusAsync();
            }
        }

    }
}
