using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Dialogs
{
    public partial class TransListDialog
    {
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

        [Inject] public ISnackbar Snackbar { get; set; }

        private IEnumerable<Transaction> Transactions { get; set; }

        private IEnumerable<Transaction> pagedData;

        private MudTable<Transaction> table;

        private int totalItems;

        private string searchString = "";

        private List<int> Types { get; set; }

        private string DialogTitle { get; set; } = "Loading Entries...";

        private string MonthName { get; set; }

        void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            if (Mode < 1 || Mode > 3) Close();

            MudDialog.Options.MaxWidth = MaxWidth.ExtraLarge;
            //MudDialog.Options.FullWidth = true;
            MudDialog.Options.NoHeader = true;

            MudDialog.SetOptions(MudDialog.Options);

            if ((Mode == 1 || Mode == 2) && Name == "Total") Name = "";
            if (Mode == 3) TypesString = String.Empty;

            Types = TypesString != String.Empty ? TypesString.Split(',').Select(int.Parse).ToList() : new List<int>();

            MessageSvc.TransactionsChanged += () => TransactionsChanged(MessageSvc.TransactionYears);
        }

        private async Task<TableData<Transaction>> ServerReload(TableState state)
        {
            await Busy();

            switch (Mode)
            {
                case 1:
                    Transactions = await Repo.GetTransactionsByTypeMonth(Types, Year, Month);
                    break;

                case 2:
                    Transactions = await Repo.GetTransactionsBySummary(Types);
                    break;

                case 3:
                    Transactions = await Repo.GetTransactionsByType(TransactionTypeId);
                    break;
            }

            totalItems = Transactions.Count();

            Transactions = Transactions.Where(transaction =>
            {
                if (string.IsNullOrWhiteSpace(searchString))
                    return true;
                if (transaction.TransactionTypeName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (transaction.TransactionDate.ToShortDateString().Contains(searchString))
                    return true;
                if ($"{transaction.Value}".Contains(searchString))
                    return true;
                if (transaction.Notes != null && transaction.Notes.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                return false;
            }).ToArray();

            SetDialogTitle();

            switch (state.SortLabel)
            {
                case "type_field":
                    Transactions = Transactions.OrderByDirection(state.SortDirection, t => t.TransactionTypeName);
                    break;
                case "value_field":
                    Transactions = Transactions.OrderByDirection(state.SortDirection, t => t.Value);
                    break;
                case "date_field":
                    Transactions = Transactions.OrderByDirection(state.SortDirection, t => t.TransactionDate);
                    break;
                case "notes_field":
                    Transactions = Transactions.OrderByDirection(state.SortDirection, t => t.Notes);
                    break;
            }

            pagedData = Transactions.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray();
            return new TableData<Transaction>() { TotalItems = Transactions.Count(), Items = pagedData };
        }
        
        private async Task Busy()
        {
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomStart;
            Snackbar.Add("Loading Entries", Severity.Normal, config =>
            {
                config.Icon = Icons.Material.Outlined.HourglassTop;
                config.ShowCloseIcon = false;
                config.VisibleStateDuration = 1000;
                config.ShowTransitionDuration = 250;
                config.HideTransitionDuration = 250;
            });

            await Task.Delay(300); // need this
        }

        private void SetDialogTitle()
        {
            string entryOrEntries = (Transactions.Count() == 1) ? " Entry " : " Entries ";

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

            if (totalItems - Transactions.Count() > 0) DialogTitle = $"{DialogTitle} ({totalItems - Transactions.Count()} Entries filtered)";

            StateHasChanged();
            return;
        }

        private void OnSearch(string text)
        {
            searchString = text;
            table.ReloadServerData();
        }

        private void TransactionsChanged(List<int> transactionYears)
        {
            // Reload regardless of Year
            table.ReloadServerData();
        }

        private string GetValueCSS(decimal value)
        {
            return (value <= 0) ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;
        }

        public void Dispose()
        {
            MessageSvc.TransactionsChanged -= () => TransactionsChanged(MessageSvc.TransactionYears);
        }

    }
}
