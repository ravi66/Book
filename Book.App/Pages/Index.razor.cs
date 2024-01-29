using Book.Dialogs;
using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace Book.Pages
{
    public partial class Index
    {
        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        [Inject] public MessageSvc MessageSvc { get; set; }

        [Inject] public TransactionRepository Repo { get; set; }

        [Inject] public SummaryTypeRepository SummaryRepo { get; set; }

        public IEnumerable<SummaryType> SummaryTypes { get; set; }

        public List<MonthlySummary> MonthlySummaries { get; set; }

        private IEnumerable<Transaction> Transactions { get; set; }

        private string BookName { get; set; } = "Book";

        public int Year { get; set; } = DateTime.Today.Year;

        public int[] Years { get; set; } = Array.Empty<int>();

        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();

            MessageSvc.TransactionsChanged += () => TransactionsChanged(MessageSvc.TransactionYears);

            Years = Enumerable.Range(await BookSettingSvc.GetStartYear(), await BookSettingSvc.GetEndYear() - await BookSettingSvc.GetStartYear() + 1).ToArray();

            SummaryTypes = (await SummaryRepo.LoadSummary()).Where(s => s.Types.Count > 0).ToList();

            await LoadSummary();
        }

        private async Task LoadSummary()
        {
            CreateMonthlySummaries();

            // Get all Transactions for year
            Transactions = await Repo.GetTransactionsByTypeMonth(new List<int>(), Year, 0);

            CreateSummaryDetails();
            RemoveZeroTransactionsSummaryDetails();
            CreateColumnInfo();
            StateHasChanged();
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

        private void CreateSummaryDetails()
        {
            DateTime startDate;
            DateTime endDate;

            foreach (MonthlySummary monthlySummary in MonthlySummaries)
            {
                if (monthlySummary.MonthNo > 0)
                {
                    startDate = new DateTime(Year, monthlySummary.MonthNo, 1);
                    endDate = startDate.AddMonths(1);
                }
                else
                {
                    startDate = new DateTime(Year, 1, 1);
                    endDate = new DateTime(Year + 1, 1, 1);
                }

                // Add Total Summary (column)
                monthlySummary.SummaryDetails.Add(new SummaryDetail()
                {
                    SummaryTypeId = 0,
                    SummaryName = "Total",
                    Types = new List<int>(),
                    Total = Transactions
                        .Where(t => t.TransactionDate >= startDate
                            && t.TransactionDate < endDate)
                        .Sum(t => t.Value) * -1,
                });

                // Add user defined Summaries
                foreach (SummaryType summaryType in SummaryTypes)
                {
                    monthlySummary.SummaryDetails.Add(new SummaryDetail()
                    {
                        SummaryTypeId = summaryType.SummaryTypeId,
                        SummaryName = summaryType.Name,
                        Types = summaryType.Types,
                        Total = Transactions
                        .Where(t => t.TransactionDate >= startDate
                            && t.TransactionDate < endDate
                            && summaryType.Types.Contains((int)t.TransactionTypeId))
                        .Sum(t => t.Value) * -1,
                    });
                }

                // Set CSS Class and HasTransactions
                foreach (SummaryDetail summaryDetail in monthlySummary.SummaryDetails)
                {
                    summaryDetail.CssClass = summaryDetail.Total >= 0 ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;

                    // Always keep the total column
                    if (summaryDetail.SummaryTypeId != 0)
                    {
                        summaryDetail.HasTransactions = Transactions
                            .Where(t => t.TransactionDate >= startDate
                                && t.TransactionDate < endDate
                                && summaryDetail.Types.Contains((int)t.TransactionTypeId))
                                .Count() > 0 ? true : false;
                    }
                    else
                    {
                        summaryDetail.HasTransactions = true;
                    }
                }
            }
        }

        private void RemoveZeroTransactionsSummaryDetails()
        {
            List<int> summariesToBeRemoved = new List<int>();

            foreach (SummaryDetail totalDetail in MonthlySummaries[12].SummaryDetails)  // Total Row
            {
                if (!totalDetail.HasTransactions)
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

                if (DateTime.Today.Year == Year)
                {
                    decimal curTot = 0;

                    if (columnDetail.Types.Count > 0)
                    {
                        curTot = Transactions.Where(t => t.TransactionDate <= DateTime.Today && columnDetail.Types.Contains((int)t.TransactionTypeId))
                            .Sum(t => t.Value) * -1 / DateTime.Today.DayOfYear * new DateTime(Year, 12, 31).DayOfYear;
                    }
                    else
                    {
                        curTot = Transactions.Where(t => t.TransactionDate <= DateTime.Today)
                            .Sum(t => t.Value) * -1 / DateTime.Today.DayOfYear * new DateTime(Year, 12, 31).DayOfYear;
                    }

                    columnInfo.InfoText = SetInfoText(columnDetail.SummaryName, curTot / 12, curTot);
                }
                else
                {
                    columnInfo.InfoText = SetInfoText(columnDetail.SummaryName, (columnDetail.Total / 12), 0);
                }

                Columns.Add(columnInfo);
            }
        }

        private string SetInfoText(string name, decimal curAvg, decimal projTotal)
        {
            string infoText = "<h2>" + name + "</h2>";
            string curAvgTextClass = curAvg >= 0 ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;
            string projTotalTextClass = projTotal >= 0 ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;

            if (DateTime.Now.Year == Year)
            {
                infoText += "<h3>Projected Month: <span class=\"" + curAvgTextClass + "\">" + curAvg.ToString("N2") + "</span></h3>";
                infoText += "<h3>Projected Year: <span class=\"" + projTotalTextClass + "\">" + projTotal.ToString("N2") + "</span></h3>";
                
                // No apologies
            }
            else
            {
                infoText += "<h3>Month Average: <span class=\"" + curAvgTextClass + "\">" + curAvg.ToString("N2") + "</span></h3>";
            }

            return infoText;
        }

        public async void YearChanged(int year)
        {
            Year = year;
            await LoadSummary();
        }

        private void TransactionsChanged(List<int> transactionYears)
        {
            foreach (int transactionYear in transactionYears)
            {
                if (transactionYear == Year)
                {
                    LoadSummary();
                    break;
                }
            }
        }

        private void TotalInfo(string infoText)
        {
            var parameters = new DialogParameters<PromptDialog>();
            parameters.Add(x => x.PromptMessage, infoText);

            var options = new DialogOptions() { NoHeader = true };

            DialogService.Show<PromptDialog>("", parameters, options);
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

            DialogService.Show<TransListDialog>("Entry List", parameters);
        }

        public void Dispose()
        {
            MessageSvc.TransactionsChanged -= () => TransactionsChanged(MessageSvc.TransactionYears);
        }

    }

    public class MonthlySummary
    {
        public string MonthName { get; set; }

        public int MonthNo { get; set; }

        public List<SummaryDetail> SummaryDetails { get; set; }
    }

    public class SummaryDetail
    {
        public int SummaryTypeId { get; set; }

        public string SummaryName { get; set; }

        public List<int> Types { get; set; }

        public decimal Total { get; set; }

        public string CssClass { get; set; }

        public bool HasTransactions { get; set; }
    }

    public class ColumnInfo
    {
        public string Name { get; set; }

        public string InfoText { get; set; }
    }

}
