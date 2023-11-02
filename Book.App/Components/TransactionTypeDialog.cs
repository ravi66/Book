using Book.Models;
using Microsoft.AspNetCore.Components;
using SqliteWasmHelper;

namespace Book.Components
{
    public partial class TransactionTypeDialog
    {
        public TransactionType TransactionType { get; set; }

        [Inject]
        public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        public bool ShowDialog { get; set; }

        [Parameter]
        public EventCallback<bool> CloseEventCallback { get; set; }

        public int SavedTransactionTypeId { get; set; }

        public List<SummaryType> SummaryTypeSL { get; set; } = new List<SummaryType>();

        public async Task Show(int transactionTypeId)
        {
            SavedTransactionTypeId = transactionTypeId;

            using var ctx = await Factory.CreateDbContextAsync();
            SummaryTypeSL = (await ctx.GetSummaryTypeSL()).ToList();

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
            if (SavedTransactionTypeId == 0)
            {
                TransactionType = new TransactionType { SummaryTypeId = -1, CreateDate = DateTime.Now };
            }
            else
            {
                using var ctx = await Factory.CreateDbContextAsync();
                TransactionType = await ctx.GetTransactionTypeById(SavedTransactionTypeId);
            }
        }

        protected async Task HandleValidSubmit()
        {
            using var ctx = await Factory.CreateDbContextAsync();

            if (SavedTransactionTypeId == 0)
            {
                await ctx.AddTransactionType(TransactionType);
            }
            else
            {
                await ctx.UpdateTransactionType(TransactionType);
            }

            ShowDialog = false;

            await CloseEventCallback.InvokeAsync(true);
            StateHasChanged();
        }
    }
}
