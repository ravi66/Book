using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Dialogs
{
    public partial class ConfirmDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public string ConfirmationTitle { get; set; }

        [Parameter] public string ConfirmationMessage { get; set; }

        [Parameter] public int CancelColorInt { get; set; }

        [Parameter] public int DoneColorInt { get; set; }

        Color CancelColor { get; set; }

        Color DoneColor { get; set; }

        void Close() => MudDialog.Cancel();

        void Done() => MudDialog.Close(DialogResult.Ok(true));

        protected override async Task OnInitializedAsync()
        {
            if (ConfirmationMessage == null || ConfirmationMessage == String.Empty) ConfirmationMessage = "Are you sure you want to?";
            if (ConfirmationTitle != null && ConfirmationTitle != String.Empty) MudDialog.SetTitle(ConfirmationTitle);

            CancelColor = CancelColorInt == 0 ? Color.Primary : Color.Secondary;
            DoneColor = DoneColorInt == 0 ? Color.Primary : Color.Secondary;

            StateHasChanged();
        }

    }
}
