namespace Book.Dialogs
{
    public partial class STypeDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int SavedSummaryTypeId { get; set; }

        [Inject] internal ISummaryTypeRepository Repo { get; set; }

        [Inject] public INotifierSvc NotifierSvc { get; set; }

        private SummaryType SummaryType { get; set; } = new SummaryType
            {
                CreateDate = DateTime.Today,
                Order = 0,
                Types = []
            };

        private readonly SummaryTypeValidator validator = new();

        private MudForm form;

        void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            await Task.Yield();

            if (SavedSummaryTypeId != 0) SummaryType = await Repo.GetSummaryTypeById(SavedSummaryTypeId);

            NotifierSvc.SummaryTypeDeleted += Close;
        }

        private async Task Save()
        {
            await form.Validate();

            if (form.IsValid)
            {
                switch (SavedSummaryTypeId)
                {
                    case 0:
                        await Repo.AddSummaryType(SummaryType);
                        break;
                    default:
                        await Repo.UpdateSummaryType(SummaryType);
                        break;
                }

                MudDialog.Close(DialogResult.Ok(true));
            }
        }

        public void Dispose()
        {
            NotifierSvc.SummaryTypeDeleted -= () => Close();
            GC.SuppressFinalize(this);
        }
    }
}