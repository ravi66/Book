using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Dialogs
{
    public partial class TransCopyDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public string DialogTitle { get; set; }

        [Parameter] public int TransactionToCopyId { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public MessageSvc MessageSvc { get; set; }

        [Inject] public TransactionRepository Repo { get; set; }

        public Transaction TransactionToCopy { get; set; }

        public IEnumerable<Transaction> NewTransactions { get; set; } = new List<Transaction>();

        public List<Frequency> Frequencies { get; set; } = new List<Frequency>() {
                new Frequency(){ FrequencyID = 3, FrequencyName = "Yearly"},
                new Frequency(){ FrequencyID = 2, FrequencyName = "Quaterly"},
                new Frequency(){ FrequencyID = 5, FrequencyName = "Bi-Monthly"},
                new Frequency(){ FrequencyID = 1, FrequencyName = "Monthly"},
                new Frequency(){ FrequencyID = 4, FrequencyName = "Weekly"},
                new Frequency(){ FrequencyID = 6, FrequencyName = "Daily"},
            };

        public Frequency SelectedFrequency { get; set; }

        public DateTime NewDate { get; set; }

        void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            if (TransactionToCopyId == 0) MudDialog.Cancel();

            TransactionToCopy = await Repo.GetTransactionById(TransactionToCopyId);
            if (TransactionToCopy.TransactionId == 0 || TransactionToCopy.TransactionTypeId == 0) MudDialog.Cancel();

            SelectedFrequency = Frequencies.FirstOrDefault(f => f.FrequencyID == 3);

            MudDialog.Options.NoHeader = true;
            MudDialog.SetOptions(MudDialog.Options);

            await LoadCopiedTransactions();
            StateHasChanged();
        }

        public async void FrequencyChanged(Frequency selectedFrequency)
        {
            SelectedFrequency = selectedFrequency;
            await LoadCopiedTransactions();
            StateHasChanged();
        }

        private async Task LoadCopiedTransactions()
        {
            NewTransactions = new List<Transaction>();

            NewDate = TransactionToCopy.TransactionDate;
            SetNewDate();

            while (NewDate <= TransactionToCopy.TransactionDate.AddYears(1))
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
            await Repo.AddTransactions(NewTransactions);

            List<int> years = new();
            
            foreach (Transaction transaction in NewTransactions)
            {
                if (years.IndexOf(transaction.TransactionDate.Year) == -1) years.Add(transaction.TransactionDate.Year);
            }

            MessageSvc.ChangeTransactions(years);

            var parameters = new DialogParameters<PromptDialog>();
            string promptText = NewTransactions.Count() > 1 ? $"<h2>{NewTransactions.Count()} Entries created</h2>" : $"<h2>{NewTransactions.Count()} Entry created</h2>";
            parameters.Add(x => x.PromptMessage, promptText);

            var options = new DialogOptions() { NoHeader = true };

            var dialog = DialogService.Show<PromptDialog>("", parameters, options);
            var result = await dialog.Result;

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
