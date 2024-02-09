using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static MudBlazor.CategoryTypes;

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

        public IEnumerable<Transaction> Transactions { get; set; }

        private IEnumerable<Transaction> pagedData;

        private MudTable<Transaction> table;

        private int totalItems;

        private string searchString = "";

        public List<int> Types { get; set; }

        public string DialogTitle { get; set; } = "Loading Entries...";

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

            Types = TypesString != String.Empty ? TypesString.Split(',').Select(int.Parse).ToList() : new List<int>();

            MessageSvc.TransactionsChanged += () => TransactionsChanged(MessageSvc.TransactionYears);
        }

        private async Task<TableData<Transaction>> ServerReload(TableState state)
        {
            ShowLoadingSnackbar();

            await Task.Delay(300); // need this
            IEnumerable<Transaction> data = await LoadTransactions();
            totalItems = data.Count();

            data = data.Where(transaction =>
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

            if (totalItems - data.Count() > 0) DialogTitle = $"{DialogTitle} ({totalItems - data.Count()} Entries filtered)";

            StateHasChanged();

            switch (state.SortLabel)
            {
                case "type_field":
                    data = data.OrderByDirection(state.SortDirection, t => t.TransactionTypeName);
                    break;
                case "value_field":
                    data = data.OrderByDirection(state.SortDirection, t => t.Value);
                    break;
                case "date_field":
                    data = data.OrderByDirection(state.SortDirection, t => t.TransactionDate);
                    break;
                case "notes_field":
                    data = data.OrderByDirection(state.SortDirection, t => t.Notes);
                    break;
            }

            pagedData = data.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray();
            return new TableData<Transaction>() { TotalItems = totalItems, Items = pagedData };
        }

        private void ShowLoadingSnackbar()
        {
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomStart;
            Snackbar.Add("Loading Entries", Severity.Normal, config =>
            {
                config.Icon = Icons.Material.Outlined.HourglassTop;
                config.ShowCloseIcon = false;
                config.VisibleStateDuration = 1000;
                config.ShowTransitionDuration = 0;
                config.HideTransitionDuration = 250;
            });

            StateHasChanged();
        }

        private void OnSearch(string text)
        {
            searchString = text;
            table.ReloadServerData();
        }

        public async Task<IEnumerable<Transaction>> LoadTransactions()
        {
            switch (Mode)
            {
                case 1:
                    Transactions = await Repo.GetTransactionsByTypeMonth(Types, Year, Month);
                    break;

                case 2:
                    Transactions = await Repo.GetTransactionsBySummary(Types); //, DateFilter.Start.Value, DateFilter.End.Value);
                    break;

                case 3:
                    Transactions = await Repo.GetTransactionsByType(TransactionTypeId); // , DateFilter.Start.Value, DateFilter.End.Value);
                    break;
            }

            /* Set Value text Colour */
            foreach (Transaction t in Transactions)
            {
                t.CssClass = (t.Value <= 0) ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;
            }

            /* Set Dialogue Title */
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
            return Transactions;
        }

        private void TransactionsChanged(List<int> transactionYears)
        {
            // Reload regardless of Year
            table.ReloadServerData();
        }

        public void Dispose()
        {
            MessageSvc.TransactionsChanged -= () => TransactionsChanged(MessageSvc.TransactionYears);
        }

    }
}
