using Book.Components;
using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SqliteWasmHelper;
using System.Globalization;

namespace Book.Pages
{
    public partial class Index
    {
        [Inject] public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        public int Year { get; set; }

        public IEnumerable<SummaryType> SummaryTypes { get; set; }

        public List<MonthlySummary> MonthlySummaries { get; set; } = new List<MonthlySummary>();

        private IEnumerable<Transaction> Transactions { get; set; }

        private string BookName { get; set; } = "Book";

        public List<int> YearSL { get; set; } = new List<int>();

        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();

        protected async override Task OnInitializedAsync()
        {
            int startYear = await BookSettingSvc.GetStartYear();
            int endYear = await BookSettingSvc.GetEndYear();

            for (int i = startYear; i <= endYear; i++) YearSL.Add(i);

            if (Year == 0)
            {
                if (DateTime.Today.Year < startYear || DateTime.Today.Year > endYear)
                {
                    Year = startYear;
                }
                else
                {
                    Year = DateTime.Today.Year;
                }
            }

            BookName = await BookSettingSvc.GetBookName();

            using var ctx = await Factory.CreateDbContextAsync();

            SummaryTypes = (await ctx.LoadSummary()).Where(s => s.Types.Count > 0).ToList();

            await LoadSummary();
        }

        private async Task LoadSummary()
        {
            CreateMonthlySummaries();

            await CreateSummaryDetails();

            RemoveZeroTransactionsSummaryDetails();
            CreateColumnInfo();
        }

        private void CreateMonthlySummaries()
        {
            MonthlySummaries = new List<MonthlySummary>();

            for (int i = 1; i < 13; i++)
            {
                MonthlySummaries.Add(new MonthlySummary()
                {
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i),
                    MonthNo = i,
                    SummaryDetails = new List<SummaryDetail>()
                });
            }

            // Add Total to MonthlySummary (row)
            MonthlySummaries.Add(new MonthlySummary()
            {
                MonthName = "Total",
                MonthNo = 0,
                SummaryDetails = new List<SummaryDetail>()
            });
        }

        private async Task CreateSummaryDetails()
        {
            using var ctx = await Factory.CreateDbContextAsync();

            foreach (MonthlySummary monthlySummary in MonthlySummaries)
            {
                // Add Total SummaryDetail (column)
                monthlySummary.SummaryDetails.Add(new SummaryDetail()
                {
                    SummaryTypeId = 0,
                    SummaryName = "Total",
                    Types = new List<int>(),
                });

                // Add user defined SummaryDetails
                foreach (SummaryType summaryType in SummaryTypes)
                {
                    monthlySummary.SummaryDetails.Add(new SummaryDetail()
                    {
                        SummaryTypeId = summaryType.SummaryTypeId,
                        SummaryName = summaryType.Name,
                        Types = summaryType.Types,
                    });
                }

                // Set SummaryDetail fields
                foreach (SummaryDetail summaryDetail in monthlySummary.SummaryDetails)
                {
                    Transactions = await ctx.GetTransactionsByTypeMonth(summaryDetail.Types, Year, monthlySummary.MonthNo);

                    summaryDetail.Total = Transactions.Sum(t => t.Value) * -1;
                    summaryDetail.CssClass = summaryDetail.Total >= 0 ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;

                    summaryDetail.HasTransactions = Transactions.Count() > 0 ? true : false;
                }
            }
        }

        private void RemoveZeroTransactionsSummaryDetails()
        {
            List<int> summariesToBeRemoved = new List<int>();

            foreach (SummaryDetail totalDetail in MonthlySummaries[12].SummaryDetails)  // Total Row
            {
                if (totalDetail.SummaryTypeId != 0 && !totalDetail.HasTransactions)  // Always keep the Total Column
                {
                    summariesToBeRemoved.Add(totalDetail.SummaryTypeId);
                }
            }

            foreach (MonthlySummary monthlySummary in MonthlySummaries)
            {
                monthlySummary.SummaryDetails = monthlySummary.SummaryDetails
                    .Select(s => new SummaryDetail
                    {
                        SummaryTypeId = s.SummaryTypeId,
                        SummaryName = s.SummaryName,
                        Types = s.Types,
                        Total = s.Total,
                        CssClass = s.CssClass
                    })
                    .Where(s => !summariesToBeRemoved.Contains(s.SummaryTypeId))
                    .ToList();
            }
        }

        private void CreateColumnInfo()
        {
            Columns = new List<ColumnInfo>();

            foreach (SummaryDetail columnDetail in MonthlySummaries[12].SummaryDetails)
            {
                var columnInfo = new ColumnInfo();
                columnInfo.Name = columnDetail.SummaryName;
                columnInfo.MoreInfoToolTip = $"{columnDetail.SummaryName} More Info";

                if (DateTime.Today.Year == Year)
                {
                    columnInfo.InfoText = SetInfoText(columnDetail.SummaryName, GetCurAvg(columnDetail.SummaryTypeId), GetCurAvg(columnDetail.SummaryTypeId) * 12);
                }
                else
                {
                    columnInfo.InfoText = SetInfoText(columnDetail.SummaryName, (columnDetail.Total / 12), 0);
                }

                Columns.Add(columnInfo);
            }
        }

        public async void YearChanged(int year)
        {
            Year = year;
            await LoadSummary();
            StateHasChanged();
        }

        private async Task AddTransaction()
        {
            var parameters = new DialogParameters<TransactionDialog>();
            parameters.Add(x => x.SavedTransactionId, 0);

            var dialog = DialogService.Show<TransactionDialog>("Create Transaction", parameters); //, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadSummary();
                StateHasChanged();
            }
        }

        protected async void TransList(string summaryName, List<int> types, int month)
        {
            string typesString = (types != null && types.Count > 0) ? typesString = string.Join(",", types) : String.Empty;

            var parameters = new DialogParameters<TransListDialog>();
            parameters.Add(x => x.Mode, 1);
            parameters.Add(x => x.Name, summaryName);
            parameters.Add(x => x.TypesString, typesString);
            parameters.Add(x => x.Year, Year);
            parameters.Add(x => x.Month, month);

            var dialog = DialogService.Show<TransListDialog>("Transaction List", parameters);
            var result = await dialog.Result;

            await LoadSummary();
            StateHasChanged();
        }

        protected void TotalInfo(string infoText)
        {
            var parameters = new DialogParameters<PromptDialog>();
            parameters.Add(x => x.PromptMessage, infoText);

            var options = new DialogOptions() { NoHeader = true };

            DialogService.Show<PromptDialog>("", parameters, options);
        }

        private decimal GetCurAvg(int summaryTypeId)
        {
            decimal total = 0;

            foreach (MonthlySummary monthlySummary in  MonthlySummaries)
            {
                if (monthlySummary.MonthNo > DateTime.Today.Month) break;

                foreach (SummaryDetail summaryDetail in monthlySummary.SummaryDetails)
                {
                    if (summaryDetail.SummaryTypeId == summaryTypeId)
                    {
                        total += summaryDetail.Total;
                        break;
                    }
                }
            }

            return total / DateTime.Today.Month;
        }

        private string SetInfoText(string name, decimal curAvg, decimal projTotal)
        {
            string infoText = "<h2>" + name + "</h2>";
            string curAvgTextClass = curAvg >= 0 ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;
            string projTotalTextClass = projTotal >= 0 ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;

            if (DateTime.Now.Year == Year)
            {
                infoText += "<h3>Projected Year Total: <span class=\"" + projTotalTextClass + "\">" + projTotal.ToString("N2") + "</span></h3>";
                infoText += "<h3>Current Monthly Average: <span class=\"" + curAvgTextClass + "\">" + curAvg.ToString("N2") + "</span></h3>";
            }
            else
            {
                infoText += "<h3>Monthly Average: <span class=\"" + curAvgTextClass + "\">" + curAvg.ToString("N2") + "</span></h3>";
            }

            return infoText;
        }

    }
}
