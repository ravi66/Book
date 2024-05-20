using Microsoft.AspNetCore.Components.Web;

namespace Book.Pages
{
    public partial class TransList
    {
        [Inject] PageParamsSvc PageParamsSvc { get; set; }

        [Inject] internal ITransactionRepository Repo { get; set; }

        [Inject] internal IBookSettingSvc BookSettingSvc { get; set; }

        [Inject] public INotifierSvc NotifierSvc { get; set; }

        [Inject] public ISnackbar Snackbar { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

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
            if (PageParamsSvc.Mode < 1 || PageParamsSvc.Mode > 3) NavigationManager.NavigateTo("/", false);

            BookName = await BookSettingSvc.GetBookName();

            if ((PageParamsSvc.Mode == 1 || PageParamsSvc.Mode == 2) && PageParamsSvc.Name == Localizer["Total"]) PageParamsSvc.Name = "";

            NotifierSvc.TransactionsChanged += TransactionsChanged;
        }

        private async Task<TableData<Transaction>> ServerReload(TableState state)
        {
            await Busy();

            Transactions = [];

            switch (PageParamsSvc.Mode)
            {
                case 1:
                    Transactions = await Repo.GetTransactionsByTypeMonth(PageParamsSvc.Types, PageParamsSvc.Year, PageParamsSvc.Month);
                    break;
                case 2:
                    Transactions = await Repo.GetTransactionsBySummary(PageParamsSvc.Types);
                    break;
                case 3:
                    Transactions = await Repo.GetTransactionsByType(PageParamsSvc.TransactionTypeId);
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

            await Task.Yield();
        }

        private void SetEntriesTitle()
        {
            switch (PageParamsSvc.Mode)
            {
                case > 1:
                    EntriesTitle = Transactions.Count() < 2 ? Localizer["TransListNameS", Transactions.Count(), PageParamsSvc.Name] : Localizer["TransListNameM", Transactions.Count(), PageParamsSvc.Name];
                    break;
                default:
                    _ = Transactions.Count() < 2 ? EntriesTitle = PageParamsSvc.Month > 0 ? Localizer["TransListTitleS", Transactions.Count(), PageParamsSvc.Name, new DateTime(2020, PageParamsSvc.Month, 1).ToString("MMMM"), PageParamsSvc.Year]
                            : Localizer["TransListTitleS", Transactions.Count(), PageParamsSvc.Name, PageParamsSvc.Year, ""] : EntriesTitle = PageParamsSvc.Month > 0 ? Localizer["TransListTitleM", Transactions.Count(), PageParamsSvc.Name, new DateTime(2020, PageParamsSvc.Month, 1).ToString("MMMM"), PageParamsSvc.Year] : Localizer["TransListTitleM", Transactions.Count(), PageParamsSvc.Name, PageParamsSvc.Year, ""];
                    break;
            }

            Total = Transactions.Sum(t => t.Value);

            if (totalItems - Transactions.Count() > 0) FilteredItems = Localizer["EntriesFiltered", totalItems - Transactions.Count()];

            StateHasChanged();
            return;
        }

        private void OnSearch(string text)
        {
            searchString = text;
            table.ReloadServerData();
        }

        private void OnDblClick(int transactionId)
        {
            DialogService.Show<TransactionDialog>(Localizer["EditEntry"], new DialogParameters<TransactionDialog> { { x => x.SavedTransactionId, transactionId } });
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

        public void Dispose()
        {
            NotifierSvc.TransactionsChanged -= TransactionsChanged;
            GC.SuppressFinalize(this);
        }
    }
}