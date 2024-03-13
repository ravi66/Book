using Book.Models;
using Book.Pages;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Dialogs
{
    public partial class TTypeDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int SavedTransactionTypeId { get; set; }

        [Parameter] public int NewSummaryTypeId { get; set; }

        [Parameter] public SummaryTypeList? SummaryTypeList { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public SummaryTypeRepository SummaryRepo { get; set; }

        [Inject] public TransactionTypeRepository TTypeRepo { get; set; }

        public TransactionType TransactionType { get; set; }

        private List<SummaryType> SummaryTypes { get; set; }

        private SummaryType _SelectedSummaryType {  get; set; }

        private bool ValidationOk { get; set; }

        private void Close() => MudDialog.Cancel();

        private bool ReadOnlySummary { get; set; } = false;
        
        protected override async Task OnInitializedAsync()
        {
            SummaryTypes = await SummaryRepo.GetAutoCompleteList();

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
                TransactionType = await TTypeRepo.GetTransactionTypeById(SavedTransactionTypeId);

                if (SavedTransactionTypeId == -1) ReadOnlySummary = true;
            }

            _SelectedSummaryType = SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == TransactionType.SummaryTypeId);
        }

        async void Save()
        {
            if (!ValidationOk) return;

            TransactionType.SummaryTypeId = _SelectedSummaryType.SummaryTypeId;

            if (SavedTransactionTypeId == 0)
            {
                await TTypeRepo.AddTransactionType(TransactionType);
            }
            else
            {
                await TTypeRepo.UpdateTransactionType(TransactionType);
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

            return SummaryTypes.Where(s => s.Name.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase));
        }

        public void CloseReload()
        {
            MudDialog.Close(DialogResult.Ok(true));
        }

    }
}
