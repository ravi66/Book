namespace Book.Buttons
{
    public partial class DeleteTTypeBtn
    {
        [Parameter] public TransactionType TransactionType { get; set; }

        [Parameter] public Variant Variant { get; set; }

        [Parameter] public Size Size { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] internal ITransactionTypeRepository Repo { get; set; }

        [Inject] public INotifierSvc NotifierSvc { get; set; }

        async Task Delete()
        {
            var dialog = DialogService.Show<ConfirmDialog>("", new DialogParameters<ConfirmDialog>
                {
                    { x => x.AcceptLabel, $"{Localizer["Delete"]} {TransactionType.Name} {Localizer["EntryType"]}" },
                    { x => x.AcceptColour, Color.Error },
                    { x => x.AcceptToolTip, Localizer["DeleteEntryType"] },
                    { x => x.CancelColour, Color.Success },
                    { x => x.CancelLabel, Localizer["No"] },
                });

            if (!(await dialog.Result).Canceled)
            {
                await Repo.DeleteTransactionType(TransactionType.TransactionTypeId);

                NotifierSvc.OnTransactionTypeDeleted();
            }
        }
    }
}