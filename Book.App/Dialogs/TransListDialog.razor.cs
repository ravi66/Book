using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Dialogs
{
    public partial class TransListDialog
    {
        public IEnumerable<Transaction> Transactions { get; set; }

        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int Mode { get; set; }

        [Parameter] public string Name { get; set; }

        [Parameter] public string TypesString { get; set; }

        [Parameter] public int Year { get; set; }

        [Parameter] public int Month { get; set; }

        [Parameter] public int TransactionTypeId { get; set; }

        [Parameter] public int SummaryTypeId { get; set; }

        [Inject] public TransactionRepository Repo { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        [Inject] public MessageSvc MessageSvc { get; set; }

        private DateRange? DateFilter { get; set; } 
   
        public List<int> Types { get; set; }

        public string DialogTitle { get; set; }

        public string MonthName { get; set; }

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
                DateFilter = new DateRange(new DateTime(await BookSettingSvc.GetStartYear(), 1, 1), new DateTime(await BookSettingSvc.GetEndYear(), 12, 31));
            }

            Types = TypesString != String.Empty ? TypesString.Split(',').Select(int.Parse).ToList() : new List<int>();

            MessageSvc.TransactionsChanged += () => TransactionsChanged(MessageSvc.TransactionYears);

            await LoadTransactions();
        }

        protected async Task HandleFilter(DateRange dateRange)
        {
            DateFilter = dateRange;
            await LoadTransactions();
        }

        public async Task LoadTransactions()
        {
            switch (Mode)
            {
                case 1:
                    Transactions = (await Repo.GetTransactionsByTypeMonth(Types, Year, Month)).ToList();
                    break;

                case 2:
                    Transactions = (await Repo.GetTransactionsBySummary(Types, DateFilter.Start.Value, DateFilter.End.Value)).ToList();
                    break;

                case 3:
                    Transactions = (await Repo.GetTransactionsByType(TransactionTypeId, DateFilter.Start.Value, DateFilter.End.Value)).ToList();
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
