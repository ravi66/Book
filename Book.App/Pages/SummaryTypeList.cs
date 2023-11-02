using Book.Components;
using Book.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SqliteWasmHelper;

namespace Book.Pages
{
    public partial class SummaryTypeList
    {
        public IEnumerable<SummaryType> SummaryTypes { get; set; }

        [Inject]
        public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        protected SummaryTypeDialog SummaryTypeDialog { get; set; }

        protected TransListDialog TransListDialog { get; set; }

        protected ConfirmDialog ConfirmDelete { get; set; }
        public int DeleteId { get; set; }

        public bool Loading = false;

        protected async override Task OnInitializedAsync()
        {
            using var ctx = await Factory.CreateDbContextAsync();
            SummaryTypes = (await ctx.GetAllSummaryTypes()).ToList();

            isSortedAscending = true;
            activeSortColumn = "Order";
        }

        protected void EditSummaryType(int summaryTypeId)
        {
            SummaryTypeDialog.Show(summaryTypeId);
        }

        protected async void ListTransactions(int summaryTypeId, string summaryName, List<int>? types)
        {
            Loading = true;
            StateHasChanged();

            await TransListDialog.ShowSummary(summaryTypeId, summaryName, types);

            Loading = false;
            StateHasChanged();
        }

        public async void SummaryTypeDialog_OnDialogClose()
        {
            await OnInitializedAsync();
            StateHasChanged();
        }

        public async void TransListDialog_OnDialogClose()
        {
            await OnInitializedAsync();
            StateHasChanged();
        }

        private bool isSortedAscending;
        private string activeSortColumn;
        private void SortTable(string columnName)
        {
            if (columnName != activeSortColumn)
            {
                SummaryTypes = SummaryTypes.OrderBy(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                isSortedAscending = true;
                activeSortColumn = columnName;
            }
            else
            {
                if (isSortedAscending)
                {
                    SummaryTypes = SummaryTypes.OrderByDescending(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                }
                else
                {
                    SummaryTypes = SummaryTypes.OrderBy(x => x.GetType().GetProperty(columnName).GetValue(x, null)).ToList();
                }

                isSortedAscending = !isSortedAscending;
            }
        }

        private string SetSortIcon(string columnName)
        {
            if (activeSortColumn != columnName)
            {
                return string.Empty;
            }
            if (isSortedAscending)
            {
                return "oi-arrow-circle-top";
            }
            else
            {
                return "oi-arrow-circle-bottom";
            }
        }

        async Task DeleteSummaryType(int id, string name)
        {
            DeleteId = id;
            ConfirmDelete.Show("Delete Summary", $"Are you sure you want to delete Summary: {name}");
        }

        protected async Task ConfirmDelete_Click(bool deleteConfirmed)
        {
            if (deleteConfirmed && DeleteId != 0)
            {
                using var ctx = await Factory.CreateDbContextAsync();
                await ctx.DeleteSummaryType(DeleteId);
                await ctx.SaveChangesAsync();

                await OnInitializedAsync();
                StateHasChanged();
            }
        }

    }

}
