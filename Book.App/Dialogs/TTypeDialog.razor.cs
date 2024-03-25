using static MudBlazor.CategoryTypes;

namespace Book.Dialogs
{
    public partial class TTypeDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int SavedTransactionTypeId { get; set; }

        [Parameter] public int NewSummaryTypeId { get; set; }

        [Inject] internal ISummaryTypeRepository SummaryRepo { get; set; }

        [Inject] internal ITransactionTypeRepository TTypeRepo { get; set; }

        [Inject] public INotifierSvc NotifierSvc { get; set; }

        private TransactionType TransactionType { get; set; } = new TransactionType
            {
                CreateDate = DateTime.Today,
            };

        private List<SummaryType> SummaryTypes { get; set; }

        private SummaryType SelectedSummaryType { get; set; }

        private readonly TransactionTypeValidator validator = new();

        private MudForm form;

        private void Close() => MudDialog.Cancel();

        private bool ReadOnlySummary { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            await Task.Yield();

            SummaryTypes = await SummaryRepo.GetAutoCompleteList();

            if (SavedTransactionTypeId == 0)
            {
                TransactionType.SummaryTypeId = NewSummaryTypeId;
                TransactionType.SummaryType = SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == NewSummaryTypeId);
            }
            else
            {
                TransactionType = await TTypeRepo.GetTransactionTypeById(SavedTransactionTypeId);

                if (SavedTransactionTypeId == -1) ReadOnlySummary = true;
            }

            SelectedSummaryType = SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == TransactionType.SummaryTypeId);

            NotifierSvc.TransactionTypeDeleted += Close;
        }

        private async Task Save()
        {
            await form.Validate();

            if (form.IsValid)
            {
                TransactionType.SummaryTypeId = SelectedSummaryType.SummaryTypeId;

                switch (SavedTransactionTypeId)
                {
                    case 0:
                        await TTypeRepo.AddTransactionType(TransactionType);
                        break;
                    default:
                        await TTypeRepo.UpdateTransactionType(TransactionType);
                        break;
                }

                MudDialog.Close(DialogResult.Ok(true));
            }
        }

        private async Task<IEnumerable<SummaryType>> TypeSearch(string searchValue)
        {
            await Task.Yield();

            return string.IsNullOrEmpty(searchValue)
                ? (IEnumerable<SummaryType>)SummaryTypes
                : SummaryTypes.Where(s => s.Name.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Dispose()
        {
            NotifierSvc.TransactionTypeDeleted -= Close;
            GC.SuppressFinalize(this);
        }
    }
}