namespace Book.Pages
{
    public partial class Index
    {
        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] internal IBookSettingSvc BookSettingSvc { get; set; }

        [Inject] public INotifierSvc NotifierSvc { get; set; }

        [Inject] internal ITransactionRepository Repo { get; set; }

        [Inject] internal ISummaryTypeRepository SummaryRepo { get; set; }

        [Inject] PageParamsSvc PageParamsSvc { get; set; }

        private IEnumerable<SummaryType> SummaryTypes { get; set; }

        public record MonthlySummary(string MonthName, string MonthNameFull, int MonthNo, List<SummaryDetail> SummaryDetails);

        public record SummaryDetail(int SummaryTypeId, string SummaryName, List<int> Types, decimal Total, string CssClass, bool HasTransactions);

        public List<MonthlySummary> MonthlySummaries { get; set; }

        private IEnumerable<Transaction> Transactions { get; set; }

        private string BookName { get; set; } = string.Empty;

        public int Year { get; set; }

        public int[] Years { get; set; } = [];

        private DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();

            NotifierSvc.TransactionsChanged += TransactionsChanged;

            Years = Enumerable.Range(await BookSettingSvc.GetStartYear(), await BookSettingSvc.GetEndYear() - await BookSettingSvc.GetStartYear() + 1).ToArray();

            if (PageParamsSvc.Year != 0)
            {
                Year = PageParamsSvc.Year;
            }
            else if (DateTime.Today.Year <= Years.Max() && DateTime.Today.Year >= Years.Min())
            {
                Year = DateTime.Today.Year;
            }
            else
            {
                Year = DateTime.Today.Year > Years.Max() ? Years.Max() : Years.Min();
            }

            SummaryTypes = (await SummaryRepo.LoadSummary()).Where(s => s.TransactionTypes.Count > 0).ToList();

            await LoadSummary();
        }

        private async Task LoadSummary()
        {
            // Get all Transactions for year
            Transactions = await Repo.GetTransactionsByTypeMonth([], Year, 0);

            MonthlySummaries = [];

            for (int i = 1; i < 13; i++)
            {
                MonthlySummaries.Add(new MonthlySummary(CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i), CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i), i, CreateSummaryDetails(i)));
            }

            // Add Total (Year) row
            MonthlySummaries.Add(new MonthlySummary(Localizer["Total"], Year.ToString(), 0, CreateSummaryDetails(0)));

            // Remove Zero Transactions SummaryDetails
            List<int> summariesToBeRemoved = MonthlySummaries[12].SummaryDetails.Where(s => !s.HasTransactions).Select(s => s.SummaryTypeId).ToList();

            foreach (MonthlySummary monthlySummary in MonthlySummaries)
            {
                monthlySummary.SummaryDetails.RemoveAll(x => summariesToBeRemoved.Contains(x.SummaryTypeId));
            }

            StateHasChanged();
        }

        private List<SummaryDetail> CreateSummaryDetails(int monthNo)
        {
            List<SummaryDetail> summaryDetails = [];

            SetDates(monthNo);

            // Total Summary (column)
            var total = Transactions.Where(t => t.TransactionDate >= StartDate && t.TransactionDate < EndDate).Sum(t => t.Value) * -1;

            summaryDetails.Add(new SummaryDetail(0, Localizer["Total"], [], total, total >= 0 ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass, true));

            // User defined Summaries
            foreach (SummaryType summaryType in SummaryTypes)
            {
                total = Transactions.Where(t => t.TransactionDate >= StartDate && t.TransactionDate < EndDate && summaryType.TransactionTypes.Any(tt => tt.TransactionTypeId == t.TransactionTypeId)).Sum(t => t.Value) * -1;

                summaryDetails.Add(new SummaryDetail(summaryType.SummaryTypeId, summaryType.Name, summaryType.TransactionTypes.Select(t => t.TransactionTypeId).ToList(), total, total >= 0 ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass, monthNo == 0 && Transactions.Any(t => t.TransactionDate >= StartDate && t.TransactionDate < EndDate && summaryType.TransactionTypes.Any(tt => tt.TransactionTypeId == t.TransactionTypeId))));
            }

            return summaryDetails;
        }

        private void SetDates(int monthNo)
        {
            if (monthNo > 0)
            {
                StartDate = new DateTime(Year, monthNo, 1);
                EndDate = StartDate.AddMonths(1);
            }
            else
            {
                StartDate = new DateTime(Year, 1, 1);
                EndDate = new DateTime(Year + 1, 1, 1);
            }
        }

        public async void YearChanged(int year)
        {
            Year = year;
            PageParamsSvc.Year = year;
            await LoadSummary();
        }

        private void TransactionsChanged(object? sender, TransactionsChangedEventArgs args)
        {
            foreach (int transactionYear in args.Years)
            {
                if (transactionYear == Year)
                {
                    LoadSummary();
                    break;
                }
            }
        }

        private void YearChart(SummaryDetail summary)
        {
            PageParamsSvc.Init();
            PageParamsSvc.Mode = 1;
            PageParamsSvc.Year = Year;
            PageParamsSvc.Name = summary.SummaryName;
            PageParamsSvc.SummaryTypeId = summary.SummaryTypeId;
            PageParamsSvc.Transactions = summary.SummaryTypeId > 0 ? Transactions.Where(t => t.SummaryName == summary.SummaryName).ToList() : Transactions.ToList();
            PageParamsSvc.PreviousPage = "/";

            NavigationManager.NavigateTo("LineChart", false);
        }

        private void MonthChart(int monthNo)
        {
            string dialogTitle = string.Empty;
            List<Transaction> transactions = [];

            if (monthNo >= 0)
            {
                dialogTitle = Localizer["Expenditure", MonthlySummaries[monthNo].MonthNameFull, Year];

                SetDates(++monthNo);
                transactions = Transactions
                    .Where(t => t.TransactionDate >= StartDate && t.TransactionDate < EndDate && t.Value > 0)
                    .ToList();
            }
            else
            {
                dialogTitle = Localizer["Expenditure", Year, ""];
                transactions = Transactions.Where(t => t.Value > 0).ToList();
            }

            var parameters = new DialogParameters<MonthChartDialog>
            {
                { p => p.DialogTitle, dialogTitle },
                { p => p.Transactions, transactions },
            };

            DialogService.Show<MonthChartDialog>("", parameters);
        }

        protected async void TransList(string summaryName, List<int> types, int month)
        {
            PageParamsSvc.Init();
            PageParamsSvc.Mode = 1;
            PageParamsSvc.Name = summaryName;
            PageParamsSvc.Types = types;
            PageParamsSvc.Year = Year;
            PageParamsSvc.Month = month;
            PageParamsSvc.PreviousPage = "/";

            NavigationManager.NavigateTo("TransList", false);
        }

        public void Dispose()
        {
            NotifierSvc.TransactionsChanged -= TransactionsChanged;
            GC.SuppressFinalize(this);
        }
    }
}