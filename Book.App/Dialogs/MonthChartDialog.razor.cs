namespace Book.Dialogs
{
    public partial class MonthChartDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public string DialogTitle { get; set; }

        [Parameter] public double[] SummaryData { get; set; }

        [Parameter] public string[] SummaryLabels { get; set; }

        [Parameter] public double[] TypeData { get; set; }

        [Parameter] public string[] TypeLabels { get; set; }

        public int SelectedOption { get; set; } = 1;

        private double[] Data { get; set; }

        private string[] Labels { get; set; }

        private string IndexText { get; set; } = string.Empty;

        private int Index = -1;

        void Close() => MudDialog.Close(DialogResult.Ok(true));

        protected async override Task OnInitializedAsync()
        {
            Data = SummaryData;
            Labels = SummaryLabels;
        }

        private void OnSelectedOptionChanged(int selectedOption)
        {
            SelectedOption = selectedOption;
            IndexText = string.Empty;
            Data = SelectedOption == 1 ? SummaryData : TypeData;
            Labels = SelectedOption == 1 ? SummaryLabels : TypeLabels;
        }

        private void OnSelectedIndexChanged(int selectedIndex)
        {
            Index = selectedIndex;
            IndexText = $"{Labels[Index]}: {Data[Index]:C2} ({Data[Index] / Data.Sum() * 100:N0}%)";
        }
    }
}