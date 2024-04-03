using System.Globalization;
using System.Linq;

namespace Book.Dialogs
{
    public partial class YearChartDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public string DialogTitle { get; set; }

        [Parameter] public List<ChartSeries> Series { get; set; }

        private List<ChartSeries> RemovedSeries { get; set; } = [];

        private readonly string[] XAxisLabels = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames;
        
        private readonly ChartOptions Options = new();

        void Close() => MudDialog.Close(DialogResult.Ok(true));

        protected async override Task OnInitializedAsync() => Options.LineStrokeWidth = 5.0;

        private void OnSelectedIndexChanged(int selectedIndex)
        {
            if (Series.Count == 1) return;
            RemovedSeries.Add(Series[selectedIndex]);
            Series.Remove(Series[selectedIndex]);
        }

        private void Reset()
        {
            Series.AddRange(from series in RemovedSeries select series);
            RemovedSeries.Clear();
        }
    }
}