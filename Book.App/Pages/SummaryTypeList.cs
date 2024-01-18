using Book.Dialogs;
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

        private MudTable<SummaryType> _Summaries { get; set; }

        private bool sBlockSwitch { get; set; } = false;

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

        protected async void ListTransactionsSummary(int summaryTypeId, string summaryName, List<int>? types)
        {
            _Summaries.SetEditingItem(null);

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
            _Summaries.SetEditingItem(null);

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
            }
        }

        private void BackupSummary(object summaryType)
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

        private void ResetSummary(object summaryType)
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
                StateHasChanged();
            }

            sBlockSwitch = false;
        }

        private async void SummaryCommitted(object summaryType)
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

            sBlockSwitch = false;
        }

        private async Task AddSummaryType()
        {
            if (_Summaries.IsEditRowSwitchingBlocked) return;

            SummaryType newSummaryType = new SummaryType();
            newSummaryType.Name = String.Empty;
            newSummaryType.Order = SummaryTypes.Count + 1;
            newSummaryType.CreateDate = DateTime.Today;
            newSummaryType.Types = new List<int>();
            newSummaryType.TransactionTypeList = new List<TransactionType>();

            SummaryTypes.Insert(0, newSummaryType);
            await Task.Delay(50);
            _Summaries.SetSelectedItem(newSummaryType);
            _Summaries.SetEditingItem(newSummaryType);
            sBlockSwitch = true;
        }

        private void ShowTransactionTypeBtnPress(int summaryTypeId)
        {
            SummaryType tmpSummaryType = SummaryTypes.First(s => s.SummaryTypeId == summaryTypeId);
            tmpSummaryType.ShowTransactionTypes = !tmpSummaryType.ShowTransactionTypes;
        }

        private async Task AddTransactionType(int summaryTypeId)
        {
            var parameters = new DialogParameters<TTypeDialog>();
            parameters.Add(x => x.NewSummaryTypeId, summaryTypeId);

            var dialog = DialogService.Show<TTypeDialog>("Create Entry Type", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadSummaryTypes();
            }
        }

        protected async Task ListTransactionsTType(int transactionTypeId, string transactionTypeName)
        {
            var parameters = new DialogParameters<TransListDialog>();
            parameters.Add(x => x.Mode, 3);
            parameters.Add(x => x.Name, transactionTypeName);
            parameters.Add(x => x.TransactionTypeId, transactionTypeId);

            DialogService.Show<TransListDialog>("Entries List", parameters);
        }

        protected async Task EditTType(int transactionTypeId)
        {
            var parameters = new DialogParameters<TTypeDialog>();
            parameters.Add(x => x.SavedTransactionTypeId, transactionTypeId);

            var dialog = DialogService.Show<TTypeDialog>("Edit Entry Type", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await LoadSummaryTypes();
            }
        }
    }
}
