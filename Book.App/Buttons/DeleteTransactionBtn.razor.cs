namespace Book.Buttons
{
    public partial class DeleteTransactionBtn
    {
        [Parameter] public Transaction Transaction { get; set; }

        [Parameter] public Variant Variant { get; set; }

        [Parameter] public Size Size { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] internal ITransactionRepository Repo { get; set; }

        [Inject] public INotifierSvc NotifierSvc { get; set; }

        async Task DeleteTransaction()
        {
            var dialog = DialogService.Show<ConfirmDialog>("", new DialogParameters<ConfirmDialog>
                {
                    { x => x.AcceptLabel, $"{Localizer["Delete"]} {Transaction.TransactionTypeName} {Localizer["EntryFor"]} {Transaction.Value:C2}" },
                    { x => x.AcceptColour, Color.Error },
                    { x => x.AcceptToolTip, Localizer["DeleteEntry"] },
                    { x => x.CancelColour, Color.Success },
                    { x => x.CancelLabel, Localizer["No"] },
                });

            if (!(await dialog.Result).Canceled && Transaction.TransactionId != 0)
            {
                await Repo.DeleteTransaction(Transaction.TransactionId);

                NotifierSvc.OnTransactionsChanged(this, [Transaction.TransactionDate.Year]);
            }
        }
    }
}