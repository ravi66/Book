using Book.Models;
using Microsoft.AspNetCore.Components;
using SqliteWasmHelper;

namespace Book.Components
{
    public partial class TransCopyDialog
    {
        public Transaction TransactionToCopy { get; set; }

        public TransactionType TransactionType { get; set; }

        private ElementReference FocusField;

        public IEnumerable<Transaction> NewTransactions { get; set; } = new List<Transaction>();

        [Inject]
        public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Parameter]
        public EventCallback<bool> CloseEventCallback { get; set; }

        public bool ShowDialog { get; set; }

        public string DialogTitle { get; set; }

        public int CopyCount { get; set; }

        public List<Frequency> FrequencySL { get; set; } = new List<Frequency>() {
                new Frequency(){ FrequencyID = 3, FrequencyName = "Yearly"},
                new Frequency(){ FrequencyID = 2, FrequencyName = "Quaterly"},
                new Frequency(){ FrequencyID = 5, FrequencyName = "Bi-Monthly"},
                new Frequency(){ FrequencyID = 1, FrequencyName = "Monthly"},
                new Frequency(){ FrequencyID = 4, FrequencyName = "Weekly"}
            };

        public int SelectedFrequency { get; set; } = 1;

        public DateTime NewDate { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (ShowDialog)
            {
                await FocusField.FocusAsync();
            }
        }

        public async void FrequencyChanged(ChangeEventArgs args)
        {
            SelectedFrequency = int.Parse(args.Value.ToString());
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
                    new Transaction {
                        TransactionTypeId = TransactionToCopy.TransactionTypeId,
                        Value = TransactionToCopy.Value,
                        TransactionDate = NewDate
                    }
                );

                SetNewDate();
            }
        }

        public async Task Show(int pTransactionId)
        {
            CopyCount = 0;

            if (pTransactionId == 0) Close();

            using var ctx = await Factory.CreateDbContextAsync();
            TransactionToCopy = await ctx.GetTransactionById(pTransactionId);
            if (TransactionToCopy.TransactionId == 0 || TransactionToCopy.TransactionTypeId == 0) Close();

            TransactionType = await ctx.GetTransactionTypeById(TransactionToCopy.TransactionTypeId ?? -1);

            DialogTitle = "Copying " + TransactionToCopy.Value.ToString("C") + " " + TransactionType.Name + " transaction";

            await LoadCopiedTransactions();
            ShowDialog = true;
            StateHasChanged();
        }

        protected async Task HandleSubmit()
        {
            using var ctx = await Factory.CreateDbContextAsync();
            await ctx.AddTransactions(NewTransactions);
            await ctx.SaveChangesAsync();

            CopyCount = NewTransactions.Count();
            Close();
        }

        public async void Close()
        {
            await CloseEventCallback.InvokeAsync(true);
            ShowDialog = false;
            StateHasChanged();
        }

        async Task DeleteTransaction(DateTime transDate)
        {
            NewTransactions = NewTransactions.Where(ct => ct.TransactionDate != transDate);

            if (NewTransactions.Count() == 0) Close();
        }

        private void SetNewDate()
        {
            switch (SelectedFrequency)
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
                    break;
            }
        }

        private bool isSortedAscending;
        private string activeSortColumn;

        private void SortTable(string columnName)
        {
            if (columnName != activeSortColumn)
            {
                NewTransactions = NewTransactions.OrderBy(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                isSortedAscending = true;
                activeSortColumn = columnName;
            }
            else
            {
                if (isSortedAscending)
                {
                    NewTransactions = NewTransactions.OrderByDescending(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                }
                else
                {
                    NewTransactions = NewTransactions.OrderBy(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                }

                isSortedAscending = !isSortedAscending;
            }
        }

        private string SetSortIcon(string columnName)
        {
            if (activeSortColumn != columnName)
            {
                return string.Empty;
            }
            if (isSortedAscending)
            {
                return "oi-arrow-circle-top";
            }
            else
            {
                return "oi-arrow-circle-bottom";
            }
        }

    }
}
