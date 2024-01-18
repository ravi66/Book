using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SqliteWasmHelper;
using static MudBlazor.CategoryTypes;

namespace Book.Dialogs
{
    public partial class TransListDialog
    {
        public IEnumerable<Transaction> Transactions { get; set; }

        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Inject] public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        [Inject] public MessageSvc MessageSvc { get; set; }

        [Parameter] public int Mode { get; set; }

        [Parameter] public string Name { get; set; }

        [Parameter] public string TypesString { get; set; }

        [Parameter] public int Year { get; set; }

        [Parameter] public int Month { get; set; }

        [Parameter] public int TransactionTypeId { get; set; }

        [Parameter] public int SummaryTypeId { get; set; }

        private DateRange _dateRange { get; set; } 
   
        public List<int> Types { get; set; }

        public string DialogTitle { get; set; }

        public string MonthName { get; set; }

        private MudTable<Transaction> _table { get; set; }

        void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            if (Mode < 1 || Mode > 3) Close();

            MudDialog.Options.MaxWidth = MaxWidth.ExtraLarge;
            MudDialog.Options.FullWidth = true;
            MudDialog.Options.NoHeader = true;

            MudDialog.SetOptions(MudDialog.Options);

            if ((Mode == 1 || Mode == 2) && Name == "Total") Name = "";
            if (Mode == 3) TypesString = String.Empty;

            if (Mode > 1)
            {
                _dateRange = new DateRange(new DateTime(await BookSettingSvc.GetStartYear(), 1, 1), new DateTime(await BookSettingSvc.GetEndYear(), 12, 31));
            }

            Types = TypesString != String.Empty ? TypesString.Split(',').Select(int.Parse).ToList() : new List<int>();

            MessageSvc.TransactionsChanged += () => TransactionsChanged(MessageSvc.TransactionYears);

            await LoadTransactions();
        }

        protected async Task HandleFilter()
        {
            await LoadTransactions();
        }

        public async Task LoadTransactions()
        {
            using var ctx = await Factory.CreateDbContextAsync();

            switch (Mode)
            {
                case 1:
                    Transactions = (await ctx.GetTransactionsByTypeMonth(Types, Year, Month)).ToList();
                    break;

                case 2:
                    Transactions = (await ctx.GetTransactionsBySummary(Types, _dateRange.Start.Value, _dateRange.End.Value)).ToList();
                    break;

                case 3:
                    Transactions = (await ctx.GetTransactionsByType(TransactionTypeId, _dateRange.Start.Value, _dateRange.End.Value)).ToList();
                    break;
            }

            /* Set Value text Colour */
            foreach (Transaction t in Transactions)
            {
                t.CssClass = (t.Value <= 0) ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;
            }

            /* Set Dialogue Title */
            string entryOrEntries = (Transactions.Count() == 1) ? " entry " : " entries ";

            switch (Mode)
            {
                case 1:
                    if (Month > 0)
                    {
                        MonthName = new DateTime(2020, Month, 1).ToString("MMMM");
                        DialogTitle = $"{Name} {entryOrEntries} in {MonthName}, {Year}";
                    }
                    else
                    {
                        DialogTitle = $"{Name} {entryOrEntries} in {Year}";
                    }
                    break;

                case 2:
                    DialogTitle = $"{Name} {entryOrEntries}";
                    break;

                case 3:
                    DialogTitle = $"{Name} {entryOrEntries}";
                    break;
            }

            DialogTitle = $"{Transactions.Count()} {DialogTitle} [{Transactions.Sum(t => t.Value):C}]";
            MudDialog.StateHasChanged();
        }

        protected async void EditTransaction(int transactionId)
        {
            var parameters = new DialogParameters<TransactionDialog>
            {
                { x => x.SavedTransactionId, transactionId }
            };

            DialogService.Show<TransactionDialog>("Edit Entry", parameters); //, options);
        }

        async Task CopyTransaction(int transactionId, string typeName, decimal value)
        {
            var parameters = new DialogParameters<TransCopyDialog>
            {
                { c => c.DialogTitle, $"Copying {value:C2} {typeName} entry" },
                { c => c.TransactionToCopyId, transactionId }
            };

            DialogService.Show<TransCopyDialog>("Copy", parameters);
        }

        async Task DeleteTransaction(Transaction transaction)
        {
            var parameters = new DialogParameters<ConfirmDialog>
            {
                { x => x.ConfirmationTitle, $"Delete {transaction.TransactionTypeName} Entry" },
                { x => x.ConfirmationMessage, $"Are you sure you want to delete this entry for {transaction.Value:C2}?" },
                { x => x.CancelColorInt, 0 },
                { x => x.DoneColorInt, 1 }
            };

            var dialog = DialogService.Show<ConfirmDialog>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled && transaction.TransactionId != 0)
            {
                using var ctx = await Factory.CreateDbContextAsync();

                await ctx.DeleteTransaction(transaction.TransactionId);

                MessageSvc.ChangeTransactions(new List<int> { transaction.TransactionDate.Year });
            }
        }

        private void TransactionsChanged(List<int> transactionYears)
        {
            LoadTransactions();
            StateHasChanged();
        }

        public void Dispose()
        {
            MessageSvc.TransactionsChanged -= () => TransactionsChanged(MessageSvc.TransactionYears);
        }

    }
}
