using Book.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SqliteWasmHelper;
using static MudBlazor.CategoryTypes;

namespace Book.Components
{
    public partial class TransCopyDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int TransactionToCopyId { get; set; }

        [Inject] public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        public Transaction TransactionToCopy { get; set; }

        public IEnumerable<Transaction> NewTransactions { get; set; } = new List<Transaction>();

        public int CopyCount { get; set; } = 0;

        public List<Frequency> Frequencies { get; set; } = new List<Frequency>() {
                new Frequency(){ FrequencyID = 3, FrequencyName = "Yearly"},
                new Frequency(){ FrequencyID = 2, FrequencyName = "Quaterly"},
                new Frequency(){ FrequencyID = 5, FrequencyName = "Bi-Monthly"},
                new Frequency(){ FrequencyID = 1, FrequencyName = "Monthly"},
                new Frequency(){ FrequencyID = 4, FrequencyName = "Weekly"}
            };

        public Frequency SelectedFrequency { get; set; }

        public DateTime NewDate { get; set; }

        void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            if (TransactionToCopyId == 0) MudDialog.Cancel();

            using var ctx = await Factory.CreateDbContextAsync();
            TransactionToCopy = await ctx.GetTransactionById(TransactionToCopyId);
            if (TransactionToCopy.TransactionId == 0 || TransactionToCopy.TransactionTypeId == 0) MudDialog.Cancel();

            SelectedFrequency = Frequencies.FirstOrDefault(f => f.FrequencyID == 1);

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
                        TransactionDate = NewDate
                    }
                );

                SetNewDate();
            }
        }

        protected async Task HandleSubmit()
        {
            using var ctx = await Factory.CreateDbContextAsync();
            await ctx.AddTransactions(NewTransactions);

            var parameters = new DialogParameters<PromptDialog>();
            string promptText = NewTransactions.Count() > 1 ? $"<h2>{NewTransactions.Count()} Transactions created</h2>" : $"<h2>{NewTransactions.Count()} Transaction created</h2>";
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

                default:
                    NewDate = NewDate.AddMonths(1);
                    break;
            }
        }

    }
}
