using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Dialogs
{
    public partial class TransactionDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int SavedTransactionId { get; set; }

        [Inject] public MessageSvc MessageSvc { get; set; }

        [Inject] public TransactionTypeRepository TTypeRepo { get; set; }

        [Inject] public TransactionRepository Repo { get; set; }

        public Transaction Transaction { get; set; }

        private List<TransactionType> TransactionTypes { get; set; }

        private TransactionType _SelectedTransactionType {  get; set; }

        private DateTime? _SelectedDate {  get; set; }

        private int OriginalYear { get; set; } = 0;

        private bool validationOk { get; set; }

        public void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            TransactionTypes = (await TTypeRepo.GetAllTransactionTypes()).ToList();

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
                Transaction = await Repo.GetTransactionById(SavedTransactionId);
                OriginalYear = Transaction.TransactionDate.Year;
            }

            _SelectedTransactionType = TransactionTypes.FirstOrDefault(t => t.TransactionTypeId == Transaction.TransactionTypeId);
            _SelectedDate = Transaction.TransactionDate;
        }

        async void Save()
        {
            if (!validationOk) return;

            Transaction.TransactionTypeId = _SelectedTransactionType.TransactionTypeId;
            Transaction.TransactionDate = (DateTime)(_SelectedDate is null ? DateTime.Today : _SelectedDate);

            _ = SavedTransactionId == 0 ? await Repo.AddTransaction(Transaction) : await Repo.UpdateTransaction(Transaction);

            List<int> years = new List<int> { Transaction.TransactionDate.Year };
            if (OriginalYear != 0 && OriginalYear != Transaction.TransactionDate.Year) years.Add(OriginalYear);
            MessageSvc.ChangeTransactions(years);

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
