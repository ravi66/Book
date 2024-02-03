﻿using Book.Models;
using Book.Pages;
using Book.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Dialogs
{
    public partial class TTypeDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public int SavedTransactionTypeId { get; set; }

        [Parameter] public int NewSummaryTypeId { get; set; }

        [Parameter] public SummaryTypeList? SummaryTypeList { get; set; }

        [Inject] public IDialogService DialogService { get; set; }

        [Inject] public SummaryTypeRepository SummaryRepo { get; set; }

        [Inject] public TransactionTypeRepository TTypeRepo { get; set; }

        public TransactionType TransactionType { get; set; }

        private List<SummaryType> SummaryTypes { get; set; }

        private SummaryType _SelectedSummaryType {  get; set; }

        private bool validationOk {  get; set; }

        private void Close() => MudDialog.Cancel();

        private bool ReadOnlySummary { get; set; } = false;
        
        protected override async Task OnInitializedAsync()
        {
            SummaryTypes = await SummaryRepo.GetAutoCompleteList();

            if (SavedTransactionTypeId == 0)
            {
                TransactionType = new TransactionType
                {
                    SummaryTypeId = NewSummaryTypeId,
                    CreateDate = DateTime.Today,
                    SummaryType = SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == NewSummaryTypeId),
                };
            }
            else
            {
                TransactionType = await TTypeRepo.GetTransactionTypeById(SavedTransactionTypeId);

                if (SavedTransactionTypeId == -1) ReadOnlySummary = true;
            }

            _SelectedSummaryType = SummaryTypes.FirstOrDefault(s => s.SummaryTypeId == TransactionType.SummaryTypeId);
        }

        async void Save()
        {
            if (!validationOk) return;

            TransactionType.SummaryTypeId = _SelectedSummaryType.SummaryTypeId;

            if (SavedTransactionTypeId == 0)
            {
                await TTypeRepo.AddTransactionType(TransactionType);
            }
            else
            {
                await TTypeRepo.UpdateTransactionType(TransactionType);
            }

            MudDialog.Close(DialogResult.Ok(true));
        }

        private async Task<IEnumerable<SummaryType>> TypeSearch(string searchValue)
        {
            await Task.Yield();

            if (string.IsNullOrEmpty(searchValue))
            {
                return SummaryTypes;
            }

            return SummaryTypes
                .Where(s => s.Name.Contains(searchValue, StringComparison.InvariantCultureIgnoreCase));
        }

        async Task DeleteTType()
        {
            var parameters = new DialogParameters<ConfirmDialog>();
            parameters.Add(x => x.ConfirmationTitle, $"Delete {TransactionType.Name} Entry Type");
            parameters.Add(x => x.ConfirmationMessage, "Are you sure you want to delete this Entry Type?");
            parameters.Add(x => x.CancelColorInt, 0);
            parameters.Add(x => x.DoneColorInt, 1);

            var dialog = DialogService.Show<ConfirmDialog>("Confirm", parameters);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                if (TransactionType.TransactionTypeId != 0)
                {
                    await TTypeRepo.DeleteTransactionType(TransactionType.TransactionTypeId);
                    MudDialog.Close(DialogResult.Ok(true));
                }
                else
                {
                    Close();
                }
            }
        }

        public void CloseReload()
        {
            MudDialog.Close(DialogResult.Ok(true));
        }

    }
}