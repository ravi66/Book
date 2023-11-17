using Book.Components;
using Book.Models;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SqliteWasmHelper;

namespace Book.Pages
{
    public partial class SummaryTypeList
    {
        [Inject] public ISqliteWasmDbContextFactory<BookDbContext> Factory { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public BookSettingSvc BookSettingSvc { get; set; }

        private string BookName { get; set; } = "Book";

        public List<SummaryType> SummaryTypes { get; set; }

        private SummaryType selectedSummaryType { get; set; }

        private SummaryType summaryTypeBeforeEdit { get; set; }

        private MudTable<SummaryType> _table { get; set; }

        private bool blockSwitch { get; set; } = false;

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();

            await LoadSummaryTypes();
        }

        private async Task LoadSummaryTypes()
        {
            using var ctx = await Factory.CreateDbContextAsync();
            SummaryTypes = await ctx.GetAllSummaryTypes();
        }

        protected async void ListTransactions(int summaryTypeId, string summaryName, List<int>? types)
        {
            _table.SetEditingItem(null);

            string typesString = (types != null && types.Count > 0) ? typesString = string.Join(",", types) : String.Empty;

            var parameters = new DialogParameters<TransListDialog>();
            parameters.Add(x => x.Mode, 2);
            parameters.Add(x => x.Name, summaryName);
            parameters.Add(x => x.TypesString, typesString);
            parameters.Add(x => x.SummaryTypeId, summaryTypeId);

            DialogService.Show<TransListDialog>("Transaction List", parameters);
        }
        
        async Task DeleteSummaryType(int summaryTypeId, string summaryName)
        {
            _table.SetEditingItem(null);

            var parameters = new DialogParameters<ConfirmDialog>();
            parameters.Add(x => x.ConfirmationTitle, $"Delete {summaryName} Summary Type");
            parameters.Add(x => x.ConfirmationMessage, "Are you sure you want to delete this Summary Type?");
            parameters.Add(x => x.CancelColorInt, 0);
            parameters.Add(x => x.DoneColorInt, 1);

            var dialog = DialogService.Show<ConfirmDialog>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled && summaryTypeId != 0)
            {
                using var ctx = await Factory.CreateDbContextAsync();

                await ctx.DeleteSummaryType(summaryTypeId);

                await LoadSummaryTypes();
                StateHasChanged();
            }
        }

        private void BackupItem(object summaryType)
        {
            summaryTypeBeforeEdit = new()
            {
                SummaryTypeId = ((SummaryType)summaryType).SummaryTypeId,
                Name = ((SummaryType)summaryType).Name,
                Order = ((SummaryType)summaryType).Order,
                Types = ((SummaryType)summaryType).Types,
                TransactionTypeList = ((SummaryType)summaryType).TransactionTypeList,
                CreateDate = ((SummaryType)summaryType).CreateDate
            };
        }

        private void ResetItemToOriginalValues(object summaryType)
        {
            if (((SummaryType)summaryType).SummaryTypeId != 0)
            {
                ((SummaryType)summaryType).SummaryTypeId = summaryTypeBeforeEdit.SummaryTypeId;
                ((SummaryType)summaryType).Name = summaryTypeBeforeEdit.Name;
                ((SummaryType)summaryType).Order = summaryTypeBeforeEdit.Order;
                ((SummaryType)summaryType).Types = summaryTypeBeforeEdit.Types;
                ((SummaryType)summaryType).TransactionTypeList = summaryTypeBeforeEdit.TransactionTypeList;
                ((SummaryType)summaryType).CreateDate = summaryTypeBeforeEdit.CreateDate;
            }
            else
            {
                SummaryTypes.RemoveAt(0);
            }

            blockSwitch = false;
            StateHasChanged();
        }

        private async void ItemHasBeenCommitted(object summaryType)
        {
            using var ctx = await Factory.CreateDbContextAsync();

            if (((SummaryType)summaryType).SummaryTypeId == 0)
            {
                await ctx.AddSummaryType((SummaryType)summaryType);
            }
            else
            {
                await ctx.UpdateSummaryType((SummaryType)summaryType);
            }

            blockSwitch = false;
        }

        private async Task AddSummaryType()
        {
            if (_table.IsEditRowSwitchingBlocked) return;

            SummaryType newSummaryType = new SummaryType();
            newSummaryType.Name = String.Empty;
            newSummaryType.Order = SummaryTypes.Count + 1;
            newSummaryType.CreateDate = DateTime.Today;
            newSummaryType.Types = new List<int>();
            newSummaryType.TransactionTypeList = new List<TransactionType>();

            SummaryTypes.Insert(0, newSummaryType);
            await Task.Delay(50);
            _table.SetSelectedItem(newSummaryType);
            _table.SetEditingItem(newSummaryType);
            blockSwitch = true;
        }

    }
}
