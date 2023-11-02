using Book.Models;
using Microsoft.AspNetCore.Components;
using SqliteWasmHelper;

namespace Book.Components
{
    public partial class SummaryTypeDialog
    {
        public SummaryType SummaryType { get; set; }

        [Inject]
        public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        public bool ShowDialog { get; set; }

        [Parameter]
        public EventCallback<bool> CloseEventCallback { get; set; }

        public int SavedSummaryTypeId { get; set; }

        public async Task Show(int summaryTypeId)
        {
            SavedSummaryTypeId = summaryTypeId;

            await ResetDialogAsync();
            ShowDialog = true;
            StateHasChanged();
        }

        public void Close()
        {
            ShowDialog = false;
            StateHasChanged();
        }

        private async Task ResetDialogAsync()
        {
            if (SavedSummaryTypeId == 0)
            {
                SummaryType = new SummaryType { CreateDate = DateTime.Now };
            }
            else
            {
                using var ctx = await Factory.CreateDbContextAsync();
                SummaryType = await ctx.GetSummaryTypeById(SavedSummaryTypeId);
            }
        }

        protected async Task HandleValidSubmit()
        {
            using var ctx = await Factory.CreateDbContextAsync();

            if (SavedSummaryTypeId == 0)
            {
                await ctx.AddSummaryType(SummaryType);
            }
            else
            {
                await ctx.UpdateSummaryType(SummaryType);
            }

            await ctx.SaveChangesAsync();

            ShowDialog = false;

            await CloseEventCallback.InvokeAsync(true);
            StateHasChanged();
        }
    }
}
