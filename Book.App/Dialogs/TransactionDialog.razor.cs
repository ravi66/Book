using Book.Shared;

namespace Book.Dialogs
{
    public partial class TransactionDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int SavedTransactionId { get; set; }

        [Inject] public INotifierSvc NotifierSvc { get; set; }

        [Inject] internal ITransactionTypeRepository TTypeRepo { get; set; }

        [Inject] internal ITransactionRepository Repo { get; set; }

        [Inject] IBrowserViewportService BrowserViewportService { get; set; }

        private Transaction Transaction { get; set; } = new Transaction
        {
            Value = 0.00M,
            TransactionTypeId = -1,
            TransactionDate = DateTime.Today,
            CreateDate = DateTime.Today,
        };

        private readonly TransactionValidator validator = new();

        private MudForm form;

        private List<TransactionType> TransactionTypes { get; set; }

        private TransactionType SelectedTransactionType { get; set; } = new();

        private DateTime? PickerDate { get; set; }

        private int OriginalYear { get; set; } = 0;

        public void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            await Task.Yield();

            TransactionTypes = (await TTypeRepo.GetAllTransactionTypes()).ToList();

            if (SavedTransactionId == 0)
            {
                Transaction.TransactionType = TransactionTypes.FirstOrDefault(tt => tt.TransactionTypeId == -1);
            }
            else
            {
                Transaction = await Repo.GetTransactionById(SavedTransactionId);
                OriginalYear = Transaction.TransactionDate.Year;
            }

            SelectedTransactionType = TransactionTypes.FirstOrDefault(t => t.TransactionTypeId == Transaction.TransactionTypeId);
            PickerDate = Transaction.TransactionDate;

            NotifierSvc.TransactionsChanged += TransactionsChanged;

            await HelperMethods.MaximiseIfMobile(BrowserViewportService, MudDialog);
        }

        private async Task Save()
        {
            await form.Validate();

            if (!form.IsValid) return;

            Transaction.TransactionTypeId = SelectedTransactionType.TransactionTypeId;
            Transaction.TransactionDate = (DateTime)(PickerDate is null ? DateTime.Today : PickerDate);

            _ = SavedTransactionId == 0 ? await Repo.AddTransaction(Transaction) : await Repo.UpdateTransaction(Transaction);

            List<int> years = [Transaction.TransactionDate.Year];
            if (OriginalYear != 0 && OriginalYear != Transaction.TransactionDate.Year) years.Add(OriginalYear);
            NotifierSvc.OnTransactionsChanged(this, years);

            MudDialog.Close(DialogResult.Ok(true));
        }

        private async Task<IEnumerable<TransactionType>> TypeSearch(string searchValue)
        {
            await Task.Yield();

            return string.IsNullOrEmpty(searchValue) ? (IEnumerable<TransactionType>)TransactionTypes : TransactionTypes.Where(t => t.Name.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase));
        }

        private void TransactionsChanged(object? sender, TransactionsChangedEventArgs args)
        {
            if (sender != this)
            {
                // Wasn't me so must have been a delete!
                Close();
            }
        }

        public void Dispose()
        {
            NotifierSvc.TransactionsChanged -= TransactionsChanged;
            GC.SuppressFinalize(this);
        }
    }
}