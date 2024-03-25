namespace Book.Buttons
{
    public partial class DeleteSTypeBtn
    {
        [Parameter] public SummaryType SummaryType { get; set; }

        [Parameter] public Variant Variant { get; set; }

        [Parameter] public Size Size { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] internal ISummaryTypeRepository Repo { get; set; }

        [Inject] public INotifierSvc NotifierSvc { get; set; }

        async Task Delete()
        {
            var dialog = DialogService.Show<ConfirmDialog>("", new DialogParameters<ConfirmDialog>
                {
                    { x => x.AcceptLabel, $"Delete {SummaryType.Name} Summary Type" },
                    { x => x.AcceptColour, Color.Error },
                    { x => x.AcceptToolTip, "Delete Summary Type" },
                    { x => x.CancelColour, Color.Success },
                    { x => x.CancelLabel, "No" },
                });

            if (!(await dialog.Result).Canceled)
            {
                await Repo.DeleteSummaryType(SummaryType.SummaryTypeId);

                NotifierSvc.OnSummaryTypeDeleted();
            }
        }
    }
}