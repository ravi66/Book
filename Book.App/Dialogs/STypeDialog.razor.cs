using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Dialogs
{
    public partial class STypeDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int SavedSummaryTypeId { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] internal ISummaryTypeRepository Repo { get; set; }

        public SummaryType SummaryType { get; set; }

        private bool ValidationOk {  get; set; }

        void Close() => MudDialog.Cancel();

        protected override async Task OnInitializedAsync()
        {
            if (SavedSummaryTypeId == 0)
            {
                SummaryType = new SummaryType
                {
                    CreateDate = DateTime.Today,
                    Order = 0,
                    Types = []
                };
            }
            else
            {
                SummaryType = await Repo.GetSummaryTypeById(SavedSummaryTypeId);
            }
        }

        async void Save()
        {
            if (!ValidationOk) return;

            if (SavedSummaryTypeId == 0)
            {
                await Repo.AddSummaryType(SummaryType);
            }
            else
            {
                await Repo.UpdateSummaryType(SummaryType);
            }

            MudDialog.Close(DialogResult.Ok(true));
        }

        public void CloseReload()
        {
            MudDialog.Close(DialogResult.Ok(true));
        }
    }
}
