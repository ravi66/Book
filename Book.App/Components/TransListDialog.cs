using Book.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SqliteWasmHelper;

namespace Book.Components
{
    public partial class TransListDialog
    {
        public IEnumerable<Transaction> Transactions { get; set; }

        [Inject]
        public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Inject]
        public IJSRuntime jsRuntime { get; set; }

        [Parameter]
        public EventCallback<bool> CloseEventCallback { get; set; }

        private ElementReference CloseButton;

        public bool ShowDialog { get; set; }

        public string Name { get; set; } = string.Empty;

        public int TypeId { get; set; }

        public int SummaryTypeId { get; set; }

        public DateTime StartDate { get; set; } = new DateTime(2017, 01, 01);
        public DateTime EndDate { get; set; } = new DateTime(DateTime.Now.Year + 5, 12, 31);

        protected TransactionDialog TransactionDialog { get; set; }

        protected PromptDialog CopyCountPrompt { get; set; }

        protected ConfirmDialog ConfirmDelete { get; set; }

        protected TransCopyDialog CopyDialog { get; set; }

        public int DeleteId { get; set; }

        public List<int> Types { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public string DialogTitle { get; set; }

        public string MonthName { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (ShowDialog)
            {
                if (!CopyDialog.ShowDialog && !CopyCountPrompt.ShowPrompt) await CloseButton.FocusAsync();
            }
        }

        protected void EditTransaction(int transactionId)
        {
            TransactionDialog.Show(transactionId);
        }

        public async void TransactionDialog_OnDialogClose()
        {
            await LoadTransactions();
            ShowDialog = true;
            StateHasChanged();
        }

        async Task CopyTransaction(int pId)
        {
            CopyDialog.Show(pId);
            StateHasChanged();
        }

        public async void CopyDialog_OnDialogClose()
        {
            if (CopyDialog.CopyCount > 0)
            {
                if (CopyDialog.CopyCount == 1)
                {
                    CopyCountPrompt.Show("1 transaction created");
                }
                else
                {
                    CopyCountPrompt.Show($"{CopyDialog.CopyCount} transactions created");
                }
            }

            await LoadTransactions();
            ShowDialog = true;
            StateHasChanged();
        }

        public async Task Show(string pSummaryName, List<int> pTypes, int pYear, int pMonth)
        {
            SummaryTypeId = 0;
            TypeId = 0;
            Name = pSummaryName;
            Types = pTypes;
            Year = pYear;
            Month = pMonth;
            isSortedAscending = false;
            activeSortColumn = "TransactionDate";

            if (Name == "Total") Name = "";

            await LoadTransactions();
            ShowDialog = true;
            StateHasChanged();
        }

        public async Task ShowSummary(int pSummaryTypeId, string pSummaryName, List<int> pTypes)
        {
            SummaryTypeId = pSummaryTypeId;
            TypeId = 0;
            Name = pSummaryName;
            Types = pTypes;
            Year = 0;
            Month = 0;
            isSortedAscending = false;
            activeSortColumn = "TransactionDate";

            if (Name == "Total") Name = "";

            await LoadTransactions();
            ShowDialog = true;
            StateHasChanged();
        }

        public async Task ShowType(int pTypeId, string pTypeName)
        {
            SummaryTypeId = 0;
            TypeId = pTypeId;
            Name = pTypeName;
            Types = new List<int>();
            Year = 0;
            Month = 0;
            isSortedAscending = false;
            activeSortColumn = "TransactionDate";

            await LoadTransactions();
            ShowDialog = true;
            StateHasChanged();
        }

        protected async Task HandleFilter()
        {
            await LoadTransactions();
            ShowDialog = true;
            StateHasChanged();
        }

        public async Task Close()
        {
            ShowDialog = false;
            await CloseEventCallback.InvokeAsync(true);
            StateHasChanged();
        }

        private async Task ResetDialogAsync()
        {
            await LoadTransactions();
        }

        private async Task LoadTransactions()
        {
            using var ctx = await Factory.CreateDbContextAsync();

            if (TypeId != 0)
            {
                Transactions = (await ctx.GetTransactionsByType(TypeId, StartDate, EndDate)).ToList();
            }
            else if (SummaryTypeId != 0)
            {
                Transactions = (await ctx.GetTransactionsBySummary(Types, StartDate, EndDate)).ToList();
            }
            else
            {
                Transactions = (await ctx.GetTransactionsByTypeMonth(Types, Year, Month)).ToList();
            }

            /*
            ** Set Value text Colour
            */
            foreach (Transaction t in Transactions)
            {
                t.CssClass = (t.Value <= 0) ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;
            }

            /*
            ** Set Dialogue Title
            */
            string transactionOrTransactions = (Transactions.Count() == 1) ? " transaction " : " transactions ";

            if (TypeId != 0)
            {
                DialogTitle = $"{Name} {transactionOrTransactions}";
            }
            else if (SummaryTypeId != 0)
            {
                DialogTitle = $"{Name} {transactionOrTransactions}";
            }
            else
            {
                if (Month > 0)
                {
                    MonthName = new DateTime(2020, Month, 1).ToString("MMMM");
                    DialogTitle = $"{Name} {transactionOrTransactions} in {MonthName}, {Year}";
                }
                else
                {
                    DialogTitle = $"{Name} {transactionOrTransactions} in {Year}";
                }
            }

            DialogTitle = $"{Transactions.Count()} {DialogTitle} [{Transactions.Sum(t => t.Value).ToString("C")}]";
        }

        async Task DeleteTransaction(int id, string pName, decimal value)
        {
            DeleteId = id;
            ConfirmDelete.Show($"Delete {pName} Transaction", $"Are you sure you want to delete this transaction for {value.ToString("C")}?");
        }

        protected async Task ConfirmDelete_Click(bool deleteConfirmed)
        {
            if (deleteConfirmed && DeleteId != 0)
            {
                using var ctx = await Factory.CreateDbContextAsync();

                await ctx.DeleteTransaction(DeleteId);
                await ctx.SaveChangesAsync();

                await LoadTransactions();
                ShowDialog = true;
                StateHasChanged();
            }
        }

        private bool isSortedAscending = true;
        private string activeSortColumn = string.Empty;
        private void SortTable(string columnName)
        {
            if (columnName != activeSortColumn)
            {
                Transactions = Transactions.OrderBy(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                isSortedAscending = true;
                activeSortColumn = columnName;
            }
            else
            {
                if (isSortedAscending)
                {
                    Transactions = Transactions.OrderByDescending(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                }
                else
                {
                    Transactions = Transactions.OrderBy(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
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
