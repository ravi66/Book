namespace Book.Buttons
{
    public partial class CopyTransactionBtn
    {
        [Parameter] public Transaction Transaction { get; set; }

        [Parameter] public Variant Variant { get; set; }

        [Parameter] public Size Size { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        async Task CopyTransaction() => DialogService.Show<TransCopyDialog>("Copy", new DialogParameters<TransCopyDialog> { { c => c.TransactionToCopyId, Transaction.TransactionId } });
    }
}