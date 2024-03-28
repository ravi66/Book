namespace Book.Dialogs
{
    public partial class TransCopyDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int TransactionToCopyId { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public INotifierSvc NotifierSvc { get; set; }

        [Inject] internal ITransactionRepository Repo { get; set; }

        [Inject] internal IBookSettingSvc BookSettingSvc { get; set; }

        private Transaction TransactionToCopy { get; set; }

        private IEnumerable<Transaction> NewTransactions { get; set; } = [];

        public record Frequency(int FrequencyID, string FrequencyName);

        private List<Frequency> Frequencies { get; set; } =
            [
                new Frequency(3, "Yearly"),
                new Frequency(2, "Quaterly"),
                new Frequency(5, "Bimonthly"),
                new Frequency(1, "Monthly"),
                new Frequency(4, "Weekly"),
                new Frequency(6, "Daily"),
            ];

        private string DialogTitle { get; set; }

        private Frequency SelectedFrequency { get; set; }

        private DateTime? EndDate { get; set; }

        private DateTime? MinDate { get; set; }

        private DateTime NewDate { get; set; }

        void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            TransactionToCopy = await Repo.GetTransactionById(TransactionToCopyId);
            if (TransactionToCopy.TransactionId == 0 || TransactionToCopy.TransactionTypeId == 0) MudDialog.Cancel();

            MudDialog.Options.NoHeader = true;
            MudDialog.SetOptions(MudDialog.Options);

            SelectedFrequency = Frequencies.First(f => f.FrequencyID == 3);
            EndDate = new DateTime(await BookSettingSvc.GetEndYear(), 12, 31);
            MinDate = new DateTime(await BookSettingSvc.GetStartYear(), 1, 1);

            DialogTitle = $"{Localizer["Copy"]} {TransactionToCopy.Value:C2} {TransactionToCopy.TransactionType.Name} {Localizer["Entry"]}";

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
            NewTransactions = [];

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
            if (!NewTransactions.Any()) return;

            var dialog = DialogService.Show<ConfirmDialog>("", new DialogParameters<ConfirmDialog>
            {
                { x => x.AcceptLabel, NewTransactions.Count() > 1 ? $"{Localizer["Create"]} {NewTransactions.Count()} {Localizer["Entries"]}?" : $"{Localizer["Create1Entry"]}?" },
                { x => x.AcceptColour, Color.Primary },
                { x => x.AcceptToolTip, Localizer["CreateEntries"] },
                { x => x.CancelColour, Color.Default },
            });

            if ((await dialog.Result).Canceled) return;

            await Repo.AddTransactions(NewTransactions);

            List<int> years = [];

            foreach (Transaction transaction in NewTransactions)
            {
                if (years.IndexOf(transaction.TransactionDate.Year) == -1) years.Add(transaction.TransactionDate.Year);
            }

            NotifierSvc.OnTransactionsChanged(this, years);

            Close();
        }

        async Task DeleteTransaction(DateTime transDate)
        {
            NewTransactions = NewTransactions.Where(ct => ct.TransactionDate != transDate);

            if (!NewTransactions.Any()) MudDialog.Cancel();
        }

        private void SetNewDate()
        {
            NewDate = SelectedFrequency.FrequencyID switch
            {
                1 => NewDate.AddMonths(1),
                2 => NewDate.AddMonths(3),
                3 => NewDate.AddYears(1),
                4 => NewDate.AddDays(7),
                5 => NewDate.AddMonths(2),
                6 => NewDate.AddDays(1),
                _ => NewDate.AddMonths(1),
            };
        }
    }
}