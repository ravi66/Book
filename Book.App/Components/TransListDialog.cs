using Book.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SqliteWasmHelper;
using static MudBlazor.CategoryTypes;

namespace Book.Components
{
    public partial class TransListDialog
    {
        public IEnumerable<Transaction> Transactions { get; set; }

        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Inject] public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Parameter] public int Mode { get; set; }

        [Parameter] public string Name { get; set; }

        [Parameter] public string TypesString { get; set; }

        [Parameter] public int Year { get; set; }

        [Parameter] public int Month { get; set; }

        [Parameter] public int TransactionTypeId { get; set; }

        [Parameter] public int SummaryTypeId { get; set; }

        private DateRange _dateRange = new DateRange(new DateTime(2017, 1, 1), new DateTime(DateTime.Today.Year + 5, 1 , 1));
   
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
            
            Types = TypesString != String.Empty ? TypesString.Split(',').Select(int.Parse).ToList() : new List<int>();
            
            await LoadTransactions();
            StateHasChanged();
        }

        protected async Task HandleFilter()
        {
            await LoadTransactions();
            StateHasChanged();
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

                default:
                    Close();
                    break;
            }

            /* Set Value text Colour */
            foreach (Transaction t in Transactions)
            {
                t.CssClass = (t.Value <= 0) ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;
            }

            /* Set Dialogue Info */
            string transactionOrTransactions = (Transactions.Count() == 1) ? " transaction " : " transactions ";

            switch (Mode)
            {
                case 1:
                    if (Month > 0)
                    {
                        MonthName = new DateTime(2020, Month, 1).ToString("MMMM");
                        DialogTitle = $"{Name} {transactionOrTransactions} in {MonthName}, {Year}";
                    }
                    else
                    {
                        DialogTitle = $"{Name} {transactionOrTransactions} in {Year}";
                    }
                    break;

                case 2:
                    DialogTitle = $"{Name} {transactionOrTransactions}";
                    break;

                case 3:
                    DialogTitle = $"{Name} {transactionOrTransactions}";
                    break;

                default:
                    Close();
                    break;
            }

            DialogTitle = $"{Transactions.Count()} {DialogTitle} [{Transactions.Sum(t => t.Value).ToString("C")}]";
        }

        protected async void EditTransaction(int transactionId)
        {
            var parameters = new DialogParameters<TransactionDialog>();
            parameters.Add(x => x.SavedTransactionId, transactionId);

            var dialog = DialogService.Show<TransactionDialog>("Edit Transaction", parameters); //, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadTransactions();
                StateHasChanged();
            }
        }

        async Task CopyTransaction(int transactionId, string typeName, decimal value)
        {
            var parameters = new DialogParameters<TransCopyDialog>();
            parameters.Add(x => x.TransactionToCopyId, transactionId);

            var dialog = DialogService.Show<TransCopyDialog>($"Copying {value.ToString("C2")} {typeName} transaction", parameters); //, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadTransactions();
                StateHasChanged();
            }
        }

        async Task DeleteTransaction(int transactionId, string typeName, decimal value)
        {
            var parameters = new DialogParameters<ConfirmDialog>();
            parameters.Add(x => x.ConfirmationTitle, $"Delete {typeName} Transaction");
            parameters.Add(x => x.ConfirmationMessage, $"Are you sure you want to delete this transaction for {value.ToString("C2")}?");
            parameters.Add(x => x.CancelColorInt, 0);
            parameters.Add(x => x.DoneColorInt, 1);

            var dialog = DialogService.Show<ConfirmDialog>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled && transactionId != 0)
            {
                using var ctx = await Factory.CreateDbContextAsync();

                await ctx.DeleteTransaction(transactionId);

                await LoadTransactions();
                StateHasChanged();
            }
        }

    }
}
