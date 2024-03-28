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
                    { x => x.AcceptLabel, $"{Localizer["Delete"]} {SummaryType.Name} {Localizer["SummaryType"]}" },
                    { x => x.AcceptColour, Color.Error },
                    { x => x.AcceptToolTip, Localizer["DeleteSummaryType"] },
                    { x => x.CancelColour, Color.Success },
                    { x => x.CancelLabel, Localizer["No"] },
                });

            if (!(await dialog.Result).Canceled)
            {
                await Repo.DeleteSummaryType(SummaryType.SummaryTypeId);

                NotifierSvc.OnSummaryTypeDeleted();
            }
        }
    }
}