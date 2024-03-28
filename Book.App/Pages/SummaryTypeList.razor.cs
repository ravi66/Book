namespace Book.Pages
{
    public partial class SummaryTypeList
    {
        [Inject] public IDialogService DialogService { get; set; }

        [Inject] internal IBookSettingSvc BookSettingSvc { get; set; }

        [Inject] internal ISummaryTypeRepository Repo { get; set; }

        [Inject] public INotifierSvc NotifierSvc { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] TransListSvc TransListSvc { get; set; }

        private string BookName { get; set; } = string.Empty;

        public List<SummaryType> SummaryTypes { get; set; }

        private SummaryType SelectedSummaryType { get; set; }

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();

            NotifierSvc.TransactionsChanged += TransactionsChanged;
            NotifierSvc.SummaryTypeDeleted += () => LoadSummaryTypes();
            NotifierSvc.TransactionTypeDeleted += () => LoadSummaryTypes();

            await LoadSummaryTypes();
        }

        public async Task LoadSummaryTypes()
        {
            SummaryTypes = await Repo.GetAllSummaryTypes();
            StateHasChanged();
        }

        protected async void ListTransactionsSummary(SummaryType summary)
        {
            TransListSvc.Mode = 2;
            TransListSvc.Name = summary.Name;
            TransListSvc.Types = summary.Types;
            TransListSvc.PreviousPage = "/SummaryTypeList";

            NavigationManager.NavigateTo("TransList", false);
        }

        private async Task AddSummaryType()
        {
            if (!(await DialogService.Show<STypeDialog>(Localizer["NewSummaryType"]).Result).Canceled) await LoadSummaryTypes();
        }

        private async Task EditSType(int summaryTypeId)
        {
            if (!(await DialogService.Show<STypeDialog>(Localizer["EditSummaryType"], new DialogParameters<STypeDialog> { { x => x.SavedSummaryTypeId, summaryTypeId } }).Result).Canceled) await LoadSummaryTypes();
        }

        private void ShowTransactionTypeBtnPress(int summaryTypeId)
        {
            SummaryType tmpSummaryType = SummaryTypes.First(s => s.SummaryTypeId == summaryTypeId);
            tmpSummaryType.ShowTransactionTypes = !tmpSummaryType.ShowTransactionTypes;
        }

        private async Task AddTransactionType(int summaryTypeId)
        {
            if (!(await DialogService.Show<TTypeDialog>(Localizer["NewEntryType"], new DialogParameters<TTypeDialog> { { x => x.NewSummaryTypeId, summaryTypeId } }).Result).Canceled) await LoadSummaryTypes();
        }

        protected async Task ListTransactionsTType(TransactionType transactionType)
        {
            TransListSvc.Mode = 3;
            TransListSvc.Name = transactionType.Name;
            TransListSvc.TransactionTypeId = transactionType.TransactionTypeId;
            TransListSvc.PreviousPage = "/SummaryTypeList";

            NavigationManager.NavigateTo("TransList", false);
        }

        protected async Task EditTType(int transactionTypeId)
        {
            if (!(await DialogService.Show<TTypeDialog>(Localizer["EditEntryType"], new DialogParameters<TTypeDialog> { { x => x.SavedTransactionTypeId, transactionTypeId }, }).Result).Canceled) await LoadSummaryTypes();
        }

        private void TransactionsChanged(object? sender, TransactionsChangedEventArgs args)
        {
            // Reload regardless of year
            LoadSummaryTypes();
        }

        public void Dispose()
        {
            NotifierSvc.TransactionsChanged -= TransactionsChanged;
            NotifierSvc.SummaryTypeDeleted -= () => LoadSummaryTypes();
            NotifierSvc.TransactionTypeDeleted -= () => LoadSummaryTypes();
            GC.SuppressFinalize(this);
        }
    }
}