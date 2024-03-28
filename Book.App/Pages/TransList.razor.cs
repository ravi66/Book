namespace Book.Pages
{
    public partial class TransList
    {
        [Inject] TransListSvc TransListSvc { get; set; }

        [Inject] internal ITransactionRepository Repo { get; set; }

        [Inject] internal IBookSettingSvc BookSettingSvc { get; set; }

        [Inject] public INotifierSvc NotifierSvc { get; set; }

        [Inject] public ISnackbar Snackbar { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        string EntriesTitle { get; set; } = string.Empty;

        decimal Total { get; set; }

        string FilteredItems { get; set; } = string.Empty;

        private IEnumerable<Transaction> Transactions { get; set; }

        private IEnumerable<Transaction> pagedData;

        private MudTable<Transaction> table;

        private int totalItems;

        private string searchString = "";

        private string BookName { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            if (TransListSvc.Mode < 1 || TransListSvc.Mode > 3) NavigationManager.NavigateTo("/", false);

            BookName = await BookSettingSvc.GetBookName();

            if ((TransListSvc.Mode == 1 || TransListSvc.Mode == 2) && TransListSvc.Name == Localizer["Total"]) TransListSvc.Name = "";

            NotifierSvc.TransactionsChanged += TransactionsChanged;
        }

        private async Task<TableData<Transaction>> ServerReload(TableState state)
        {
            await Busy();

            Transactions = [];

            switch (TransListSvc.Mode)
            {
                case 1:
                    Transactions = await Repo.GetTransactionsByTypeMonth(TransListSvc.Types, TransListSvc.Year, TransListSvc.Month);
                    break;
                case 2:
                    Transactions = await Repo.GetTransactionsBySummary(TransListSvc.Types);
                    break;
                case 3:
                    Transactions = await Repo.GetTransactionsByType(TransListSvc.TransactionTypeId);
                    break;
                default:
                    break;
            }

            totalItems = Transactions.Count();

            Transactions = Transactions.Where(transaction =>
            {
                if (string.IsNullOrWhiteSpace(searchString))
                    return true;
                if (transaction.TransactionTypeName != null && transaction.TransactionTypeName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (transaction.TransactionDate.ToShortDateString().Contains(searchString))
                    return true;
                if ($"{transaction.Value}".Contains(searchString))
                    return true;
                if (transaction.Notes != null && transaction.Notes.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                    return true;
                return false;
            }).ToArray();

            SetEntriesTitle();

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
                default:
                    break;
            }

            pagedData = Transactions.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray();
            return new TableData<Transaction>() { TotalItems = Transactions.Count(), Items = pagedData };
        }

        private async Task Busy()
        {
            Snackbar.Configuration.PositionClass = Defaults.Classes.Position.BottomStart;
            Snackbar.Add(Localizer["LoadingEntries"], Severity.Normal, config =>
            {
                config.Icon = Icons.Material.Outlined.HourglassTop;
                config.ShowCloseIcon = false;
                config.VisibleStateDuration = 1000;
                config.ShowTransitionDuration = 250;
                config.HideTransitionDuration = 250;
            });

            await Task.Delay(300); // need this
        }

        private void SetEntriesTitle()
        {
            string entryOrEntries = (Transactions.Count() == 1) ? $" {Localizer["Entry"]} " : $" {Localizer["Entries"]} ";

            switch (TransListSvc.Mode)
            {
                case 1:
                    EntriesTitle = TransListSvc.Month > 0 ? $"{TransListSvc.Name} {entryOrEntries} {Localizer["In"]} {new DateTime(2020, TransListSvc.Month, 1):MMMM}, {TransListSvc.Year}" : EntriesTitle = $"{TransListSvc.Name} {entryOrEntries} {Localizer["In"]} {TransListSvc.Year}";
                    break;

                case 2:
                    EntriesTitle = $"{TransListSvc.Name} {entryOrEntries}";
                    break;

                case 3:
                    EntriesTitle = $"{TransListSvc.Name} {entryOrEntries}";
                    break;
            }

            EntriesTitle = $"{Transactions.Count()} {EntriesTitle}";
            Total = Transactions.Sum(t => t.Value);

            if (totalItems - Transactions.Count() > 0) FilteredItems = $"({totalItems - Transactions.Count()} {Localizer["EntriesFiltered"]})";

            StateHasChanged();
            return;
        }

        private void OnSearch(string text)
        {
            searchString = text;
            table.ReloadServerData();
        }

        private void TransactionsChanged(object? sender, TransactionsChangedEventArgs args)
        {
            // Reload regardless of Year
            table.ReloadServerData();
        }

        private static string GetValueCSS(decimal value)
        {
            return (value <= 0) ? $"{Constants.PositiveValueCssClass} py-0" : $"{Constants.NegativeValueCssClass} py-0";
        }

        void GoBack() => NavigationManager.NavigateTo(TransListSvc.PreviousPage, false);

        public void Dispose()
        {
            NotifierSvc.TransactionsChanged -= TransactionsChanged;
            GC.SuppressFinalize(this);
        }
    }
}