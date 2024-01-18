using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Dialogs
{
    public partial class PromptDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public string PromptMessage { get; set; }

        void Close() => MudDialog.Close(DialogResult.Ok(true));
    }
}
