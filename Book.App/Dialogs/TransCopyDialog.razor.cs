using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Dialogs
{
    public partial class TransCopyDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int TransactionToCopyId { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public MessageSvc MessageSvc { get; set; }

        [Inject] public TransactionRepository Repo { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        private Transaction TransactionToCopy { get; set; }

        private IEnumerable<Transaction> NewTransactions { get; set; } = new List<Transaction>();

        private List<Frequency> Frequencies { get; set; } = [
                new Frequency(){ FrequencyID = 3, FrequencyName = "Yearly"},
                new Frequency(){ FrequencyID = 2, FrequencyName = "Quaterly"},
                new Frequency(){ FrequencyID = 5, FrequencyName = "Bi-Monthly"},
                new Frequency(){ FrequencyID = 1, FrequencyName = "Monthly"},
                new Frequency(){ FrequencyID = 4, FrequencyName = "Weekly"},
                new Frequency(){ FrequencyID = 6, FrequencyName = "Daily"},
            ];

        private string DialogTitle { get; set; }

        private Frequency SelectedFrequency { get; set; }

        private DateTime? EndDate { get; set; }

        private DateTime NewDate { get; set; }

        void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            TransactionToCopy = await Repo.GetTransactionById(TransactionToCopyId);
            if (TransactionToCopy.TransactionId == 0 || TransactionToCopy.TransactionTypeId == 0) MudDialog.Cancel();

            MudDialog.Options.NoHeader = true;
            MudDialog.SetOptions(MudDialog.Options);

            SelectedFrequency = Frequencies.FirstOrDefault(f => f.FrequencyID == 3);
            EndDate = new DateTime(await BookSettingSvc.GetEndYear(), 12, 31);
            DialogTitle = $"Copy {TransactionToCopy.Value:C2} {TransactionToCopy.TransactionType.Name} entry";

            await LoadCopiedTransactions();
            StateHasChanged();
        }

        public async void FrequencyChanged(Frequency selectedFrequency)
        {
            SelectedFrequency = selectedFrequency;
            await LoadCopiedTransactions();
            StateHasChanged();
        }

        public async Task EndDateChanged(DateTime? newEndDate)
        {
            EndDate = newEndDate;
            await LoadCopiedTransactions();
            StateHasChanged();
        }

        private async Task LoadCopiedTransactions()
        {
            NewTransactions = new List<Transaction>();

            NewDate = TransactionToCopy.TransactionDate;
            SetNewDate();

            while (NewDate <= EndDate)
            {
                NewTransactions = NewTransactions.Append(
                    new Transaction
                    {
                        TransactionTypeId = TransactionToCopy.TransactionTypeId,
                        Value = TransactionToCopy.Value,
                        TransactionDate = NewDate,
                        CreateDate = DateTime.Today,
                    }
                );

                SetNewDate();
            }
        }

        protected async Task HandleSubmit()
        {
            if (NewTransactions.Count() < 1) return;

            string confirmText = NewTransactions.Count() > 1 ? $"<h4>Create {NewTransactions.Count()} Entries?</h4>" : $"<h4>Create 1 Entry?</h4>";

            var parameters = new DialogParameters<ConfirmDialog>
            {
                { x => x.ConfirmationTitle, "Copy Entry" },
                { x => x.ConfirmationMessage, confirmText },
                { x => x.CancelColorInt, 0 },
                { x => x.DoneColorInt, 1 }
            };

            var dialog = DialogService.Show<ConfirmDialog>("Confirm", parameters);
            var result = await dialog.Result;

            if (result.Canceled) return;
            
            await Repo.AddTransactions(NewTransactions);

            List<int> years = new();
            
            foreach (Transaction transaction in NewTransactions)
            {
                if (years.IndexOf(transaction.TransactionDate.Year) == -1) years.Add(transaction.TransactionDate.Year);
            }

            MessageSvc.ChangeTransactions(years);

            MudDialog.Cancel();
        }

        async Task DeleteTransaction(DateTime transDate)
        {
            NewTransactions = NewTransactions.Where(ct => ct.TransactionDate != transDate);

            if (NewTransactions.Count() == 0) MudDialog.Cancel();
        }

        private void SetNewDate()
        {
            switch (SelectedFrequency.FrequencyID)
            {
                case 1:
                    NewDate = NewDate.AddMonths(1);
                    break;

                case 2:
                    NewDate = NewDate.AddMonths(3);
                    break;

                case 3:
                    NewDate = NewDate.AddYears(1);
                    break;

                case 4:
                    NewDate = NewDate.AddDays(7);
                    break;

                case 5:
                    NewDate = NewDate.AddMonths(2);
                    break;

                case 6:
                    NewDate = NewDate.AddDays(1);
                    break;

                default:
                    NewDate = NewDate.AddMonths(1);
                    break;
            }
        }

    }

    public class Frequency
    {
        public int FrequencyID { get; set; }

        public string FrequencyName { get; set; }

    }
}
