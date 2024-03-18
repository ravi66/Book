﻿using Book.Dialogs;
using Book.Models;
using Book.Services;
using Book.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace Book.Pages
{
    public partial class Index
    {
        [Inject] public IDialogService DialogService { get; set; }

        [Inject] internal BookSettingSvc BookSettingSvc { get; set; }

        [Inject] public MessageSvc MessageSvc { get; set; }

        [Inject] internal ITransactionRepository Repo { get; set; }

        [Inject] internal ISummaryTypeRepository SummaryRepo { get; set; }

        public IEnumerable<SummaryType> SummaryTypes { get; set; }

        public record MonthlySummary(string MonthName, string MonthNameFull, int MonthNo, List<SummaryDetail> SummaryDetails);

        public record SummaryDetail(int SummaryTypeId, string SummaryName, List<int> Types, decimal Total, string CssClass, bool HasTransactions);

        public List<MonthlySummary> MonthlySummaries { get; set; }

        private IEnumerable<Transaction> Transactions { get; set; }

        private string BookName { get; set; } = "Book";

        public int Year { get; set; }

        public int[] Years { get; set; } = [];

        private DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();

            MessageSvc.TransactionsChanged += () => TransactionsChanged(MessageSvc.TransactionYears);

            Years = Enumerable.Range(await BookSettingSvc.GetStartYear(), await BookSettingSvc.GetEndYear() - await BookSettingSvc.GetStartYear() + 1).ToArray();
            Year = Years.Max() >= DateTime.Today.Year ? DateTime.Today.Year : Years.Max();

            SummaryTypes = (await SummaryRepo.LoadSummary()).Where(s => s.Types.Count > 0).ToList();

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

            // Add Total row
            MonthlySummaries.Add(new MonthlySummary("Total", Year.ToString(), 0, CreateSummaryDetails(0)));

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

            summaryDetails.Add(new SummaryDetail(0, "Total", [], total, total >= 0 ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass, true));

            // User defined Summaries
            foreach (SummaryType summaryType in SummaryTypes)
            {
                total = Transactions.Where(t => t.TransactionDate >= StartDate && t.TransactionDate < EndDate && summaryType.Types.Contains((int)t.TransactionTypeId)).Sum(t => t.Value) * -1;

                summaryDetails.Add(new SummaryDetail(summaryType.SummaryTypeId, summaryType.Name, summaryType.Types, total, total >= 0 ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass, monthNo == 0 && Transactions.Any(t => t.TransactionDate >= StartDate && t.TransactionDate < EndDate && summaryType.Types.Contains((int)t.TransactionTypeId))));
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

        private void YearChart(int summaryTypeId)
        {
            List<ChartSeries> summarySeries = [];

            if (summaryTypeId > 0)
            {
                summarySeries.Add(GetSummarySeries(summaryTypeId));
            }
            else
            {
                foreach (var summary in MonthlySummaries[12].SummaryDetails)
                {
                    summarySeries.Add(GetSummarySeries(summary.SummaryTypeId));
                }
            }

            var parameters = new DialogParameters<YearChartDialog>
            {
                { p => p.DialogTitle, $"{Year} Chart" },
                { p => p.Series, summarySeries },
            };

            var options = new DialogOptions() { NoHeader = true, MaxWidth = MaxWidth.ExtraLarge };

            DialogService.Show<YearChartDialog>("", parameters, options);
        }

        private ChartSeries GetSummarySeries(int summaryTypeId)
        {
            List<double> summaryData = [];

            for (int i = 0; i < 12; i++)
            {
                summaryData.Add((double)MonthlySummaries[i].SummaryDetails.Single(s => s.SummaryTypeId == summaryTypeId).Total);
            }

            return new ChartSeries { Name = MonthlySummaries[12].SummaryDetails.Single(s => s.SummaryTypeId == summaryTypeId).SummaryName, Data = [.. summaryData] };
        }

        private void MonthChart(int monthNo)
        {
            SetDates(monthNo + 1);

            monthNo = monthNo < 0 ? 12 : monthNo;

            string dialogTitle = monthNo < 12 ? $"{MonthlySummaries[monthNo].MonthNameFull} {Year} Expenditure" : $"{Year} Expenditure";

            var monthlyTransactions = Transactions
                .Where(t => t.TransactionDate >= StartDate && t.TransactionDate < EndDate && t.Value > 0)
                .GroupBy(t => t.TransactionTypeName)
                .Select(g => new { TransactionTypeName = g.Key, Value = g.Sum(t => t.Value) })
                .ToList();

            var parameters = new DialogParameters<MonthChartDialog>
            {
                { x => x.DialogTitle, dialogTitle },
                { x => x.SummaryData, MonthlySummaries[monthNo].SummaryDetails.Where(s => s.Total < 0 && s.SummaryTypeId != 0).Select(s => (double)(s.Total * -1)).ToArray() },
                { x => x.SummaryLabels, MonthlySummaries[monthNo].SummaryDetails.Where(s => s.Total < 0 && s.SummaryTypeId != 0).Select(s => s.SummaryName).ToArray() },
                { x => x.TypeData, monthlyTransactions.Select(s => (double)(s.Value)).ToArray() },
                { x => x.TypeLabels, monthlyTransactions.Select(s => s.TransactionTypeName).ToArray() },
            };

            var options = new DialogOptions() { NoHeader = true, MaxWidth = MaxWidth.ExtraLarge };

            DialogService.Show<MonthChartDialog>("", parameters, options);
        }

        protected async void TransList(string summaryName, List<int> types, int month)
        {
            string typesString = (types != null && types.Count > 0) ? typesString = string.Join(",", types) : string.Empty;

            var parameters = new DialogParameters<TransListDialog>
            {
                { x => x.Mode, 1 },
                { x => x.Name, summaryName },
                { x => x.TypesString, typesString },
                { x => x.Year, Year },
                { x => x.Month, month }
            };

            DialogService.Show<TransListDialog>("Entry List", parameters);
        }

        public void Dispose()
        {
            MessageSvc.TransactionsChanged -= () => TransactionsChanged(MessageSvc.TransactionYears);
            GC.SuppressFinalize(this);
        }

    }
}
