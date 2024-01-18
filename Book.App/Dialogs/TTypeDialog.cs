using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SqliteWasmHelper;

namespace Book.Dialogs
{
    public partial class TTypeDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int SavedTransactionTypeId { get; set; }

        [Parameter] public int NewSummaryTypeId { get; set; }

        [Inject] public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Inject] public MessageSvc MessageSvc { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        public TransactionType TransactionType { get; set; }

        private List<SummaryType> SummaryTypes { get; set; }

        private SummaryType _SelectedSummaryType {  get; set; }

        private bool validationOk {  get; set; }

        void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            using var ctx = await Factory.CreateDbContextAsync();
            SummaryTypes = (await ctx.GetAllSummaryTypes()).ToList();

            if (SavedTransactionTypeId == 0)
            {
                TransactionType = new TransactionType
                {
                    SummaryTypeId = NewSummaryTypeId,
                    CreateDate = DateTime.Today,
                    SummaryType = SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == NewSummaryTypeId),
                };
            }
            else
            {
                TransactionType = await ctx.GetTransactionTypeById(SavedTransactionTypeId);
            }

            _SelectedSummaryType = SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == TransactionType.SummaryTypeId);
        }

        async void Save()
        {
            if (!validationOk) return;

            TransactionType.SummaryTypeId = _SelectedSummaryType.SummaryTypeId;

            using var ctx = await Factory.CreateDbContextAsync();

            if (SavedTransactionTypeId == 0)
            {
                await ctx.AddTransactionType(TransactionType);
            }
            else
            {
                await ctx.UpdateTransactionType(TransactionType);
            }

            MudDialog.Close(DialogResult.Ok(true));
        }

        private async Task<IEnumerable<SummaryType>> TypeSearch(string searchValue)
        {
            await Task.Yield();

            if (string.IsNullOrEmpty(searchValue))
            {
                return SummaryTypes;
            }

            return SummaryTypes
                .Where(s => s.Name.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase));
        }

        async Task DeleteTType()
        {
            var parameters = new DialogParameters<ConfirmDialog>();
            parameters.Add(x => x.ConfirmationTitle, $"Delete {TransactionType.Name} Entry Type");
            parameters.Add(x => x.ConfirmationMessage, "Are you sure you want to delete this Entry Type?");
            parameters.Add(x => x.CancelColorInt, 0);
            parameters.Add(x => x.DoneColorInt, 1);

            var dialog = DialogService.Show<ConfirmDialog>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                if (TransactionType.TransactionTypeId != 0)
                {
                    using var ctx = await Factory.CreateDbContextAsync();

                    await ctx.DeleteTransactionType(TransactionType.TransactionTypeId);

                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    Close();
                }
            }
        }

    }
}
