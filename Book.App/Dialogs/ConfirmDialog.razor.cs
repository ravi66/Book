namespace Book.Dialogs
{
    public partial class ConfirmDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public string CancelLabel { get; set; } = "Cancel";

        [Parameter] public Color CancelColour { get; set; } = Color.Default;

        [Parameter] public string CancelToolTip { get; set; } = "Cancel";

        [Parameter] public string AcceptLabel { get; set; } = "Continue";

        [Parameter] public Color AcceptColour { get; set; } = Color.Default;

        [Parameter] public string AcceptToolTip { get; set; } = "Continue";

        [Parameter] public bool Warning { get; set; }

        [Parameter] public string WarningMessage { get; set; } = string.Empty;

        private string ContentClass { get; set; } = string.Empty;

        void Close() => MudDialog.Cancel();

        void Accept() => MudDialog.Close(DialogResult.Ok(true));

        protected override async Task OnInitializedAsync()
        {
            MudDialog.Options.NoHeader = true;
            MudDialog.SetOptions(MudDialog.Options);

            if (Warning) ContentClass = "mud-warning";
        }
    }
}
