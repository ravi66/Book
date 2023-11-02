using Book.Components;
using Book.Models;
using Microsoft.AspNetCore.Components;
using SqliteWasmHelper;

namespace Book.Pages
{
    public partial class Index
    {
        public IEnumerable<SummaryType> SummaryTypes { get; set; }

		public IEnumerable<Summary> Summarys { get; set; }

        private IEnumerable<Transaction> Transactions { get; set; }

        private ElementReference New;

        private BookSetting BookNameSetting { get; set; }
        private string BookName { get; set; }

        public int Year { get; set; } = 0;

        public List<int> YearSL { get; set; }

        public DateTime startDate { get; set; }
        
        public DateTime endDate { get; set; }

        [Inject]
        public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Parameter]
        public EventCallback<bool> CloseEventCallback { get; set; }
        
        protected TransactionDialog TransactionDialog { get; set; }

        protected TransListDialog TransListDialog { get; set; }

        protected PromptDialog TotalInfoDialog { get; set; }
        
        protected async override Task OnInitializedAsync()
        {
            await EnsureDbSeeded();
            await LoadSummary();
		}

        private async Task EnsureDbSeeded()
        {
            SummaryType? summaryTypeSeeded;
            TransactionType? transactionTypeSeeded;
            BookSetting? bookSettingSeeded;

            using var ctx = await Factory.CreateDbContextAsync();

            summaryTypeSeeded = (await ctx.GetSummaryTypeById(-1));

            if (summaryTypeSeeded is null)
            {
                ctx.SummaryTypes.Add(new SummaryType { SummaryTypeId = -1, Name = "Unknown", Order = 9999, CreateDate = DateTime.Today });
                await ctx.SaveChangesAsync();
            }

            transactionTypeSeeded = (await ctx.GetTransactionTypeById(-1));

            if (transactionTypeSeeded is null)
            {
                ctx.TransactionTypes.Add(new TransactionType { TransactionTypeId = -1, Name = "Unknown", SummaryTypeId = -1, CreateDate = DateTime.Today });
                await ctx.SaveChangesAsync();
            }

            bookSettingSeeded = (await ctx.GetBookSettingById(1));

            if (bookSettingSeeded is null)
            {
                ctx.BookSetting.Add(new BookSetting { BookSettingId = 1, UserAmendable = 1,  SettingName = "Book Name", SettingValue = "Book" });
                await ctx.SaveChangesAsync();
            }

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                if (!TotalInfoDialog.ShowPrompt) await New.FocusAsync();
            }
        }

        protected void AddTransaction()
        {
            TransactionDialog.Show(0);
        }

        protected void TransList(string summaryName, List<int> types, int month)
        {
            TransListDialog.Show(summaryName, types, Year, month);
        }

        protected void TotalInfo(string infoText)
        {
            TotalInfoDialog.Show(infoText);
        }

        public async void YearChanged(ChangeEventArgs args)
        {
            Year = int.Parse(args.Value.ToString());
            SummaryTypes = null;
            await LoadSummary();
            StateHasChanged();
        }

        public async void TransactionDialog_OnDialogClose()
        {
            await LoadSummary();
            StateHasChanged();
        }

        public async void TransListDialog_OnDialogClose()
        {
            await LoadSummary();
            StateHasChanged();
        }

        private async Task LoadSummary()
        {
            if (Year == 0)
                Year = DateTime.Now.Year;
            YearSL = new List<int>();

            for (int i = 2017; i <= DateTime.Now.Year + 5; i++)
            {
                YearSL.Add(i);
            }

            using var ctx = await Factory.CreateDbContextAsync();

            BookNameSetting = (await ctx.GetBookSettingById(1));
            BookName = BookNameSetting.SettingValue;

            SummaryTypes = (await ctx.LoadSummary()).ToList();

            Summarys = SummaryTypes
                .Select(s => new Summary 
                    {Name = s.Name,
                     Types =  s.Types,
                     Order = s.Order})
                .Where (s => s.Types.Count > 0)
                .ToList();

            foreach (Summary summary in Summarys)
            {
                Transactions = (await ctx.GetTransactionsByTypeYear(summary.Types, Year)).ToList();

                summary.Count = Transactions.Count();

                summary.SummaryMonths = new();

                for (int monthNo = 1; monthNo < 13; monthNo++)
                {
                    startDate = new DateTime(Year, monthNo, 1);
                    endDate = startDate.AddMonths(1);

                    decimal monthTotal = 0;
                    monthTotal = Transactions.Where(t => t.TransactionDate >= startDate && t.TransactionDate < endDate).Sum(t => t.Value) * -1;

                    string monthCssClass = string.Empty;
                    monthCssClass = (monthTotal >= 0) ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;

                    summary.SummaryMonths.Add(new SummaryMonth()
                    {
                        Value = monthTotal,
                        CssClass = monthCssClass
                    });
                }

                summary.Total = summary.SummaryMonths.Sum(s => s.Value);
                summary.TotCssClass = (summary.Total >= 0) ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;

                if (DateTime.Now.Year != Year)
                {
                    summary.Mat = GetCurTot(summary, 12) / 12;
                    summary.PTot = 0;
                }
                else
                {
                    summary.Mat = GetCurTot(summary, DateTime.Now.Month) / DateTime.Now.Month;
                    summary.PTot = summary.Mat * 12;
                }

                summary.InfoText = SetInfoText(summary.Name, summary.Mat, summary.PTot);
            }

            Summarys = Summarys.Where(s => s.Count != 0).ToList();

            AddTotalSummary();

			Summarys = Summarys.OrderBy(s => s.Order).ToList();
        }
        
        private decimal GetCurTot(Summary summary, int monthNo)
        {
            decimal total = 0;

            for (int i = 0; i < monthNo; i++)
            {
                total += summary.SummaryMonths[i].Value;
            }

            return total;
        }

        private void AddTotalSummary()
        {
            List<SummaryMonth> summaryMonths = new();

            for (int monthNo = 1; monthNo < 13; monthNo++)
            {
                decimal monthTotal = 0;

                foreach (Summary summary in Summarys)
                {
                    monthTotal += summary.SummaryMonths[monthNo - 1].Value;
                }

                string monthCssClass = string.Empty;
                monthCssClass = (monthTotal >= 0) ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass;

                summaryMonths.Add(new SummaryMonth()
                {
                    Value = monthTotal,
                    CssClass = monthCssClass
                });

            }
                        
            Summarys = Summarys.Append(new Summary
            {
                Name = "Total",
                Order = -1,
                SummaryMonths = summaryMonths,
                Total = Summarys.Sum(s => s.Total),
                TotCssClass = (Summarys.Sum(s => s.Total) >= 0) ? Constants.PositiveValueCssClass : Constants.NegativeValueCssClass,
                Mat = Summarys.Sum(s => s.Mat),
                PTot = Summarys.Sum(s => s.PTot),
                InfoText = SetInfoText("Total", Summarys.Sum(s => s.Mat), Summarys.Sum(s => s.PTot))
            });

        }

        private string SetInfoText(string name, decimal curAvg, decimal projTotal)
        {
            string infoText = "<h5>" + name + "</h5>";
            string curAvgTextClass = "text-body";
            string projTotalTextClass = "text-body";

            if (curAvg < 0) curAvgTextClass = "text-danger";
            if (projTotal < 0) projTotalTextClass = "text-danger";

            if (DateTime.Now.Year == Year)
            {
                infoText += "<div>Projected Year Total: <span class=\"" + projTotalTextClass + "\">" + projTotal.ToString("N2") + "</span></div>";
                infoText += "<div>Current Monthly Average: <span class=\"" + curAvgTextClass + "\">" + curAvg.ToString("N2") + "</span></div>";
            }
            else
            {
                infoText += "<div>Monthly Average: <span class=\"" + curAvgTextClass + "\">" + curAvg.ToString("N2") + "</span></div>";
            }

            return infoText;
        }

    }
}
