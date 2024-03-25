namespace Book.Buttons
{
    public partial class TransactionBtn
    {
        [Parameter] public bool IsIcon { get; set; }

        [Parameter] public int TransactionId { get; set; }

        [Parameter] public string Icon { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        private string DialogTitle = string.Empty;

        private Variant Variant = Variant.Text;

        private string AriaLabel = string.Empty;

        protected async override Task OnInitializedAsync()
        {
            AriaLabel = TransactionId == 0 ? "Create New Entry" : "Edit Entry";
            Variant = TransactionId == 0 ? Variant.Filled : Variant.Text;
        }

        private async Task CallTransactionDialog()
        {
            DialogTitle = TransactionId == 0 ? "New Entry" : "Edit Entry";

            DialogService.Show<TransactionDialog>(DialogTitle, new DialogParameters<TransactionDialog>{ { x => x.SavedTransactionId, TransactionId } });
        }
    }
}