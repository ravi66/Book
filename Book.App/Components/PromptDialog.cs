using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Book.Components
{
    public partial class PromptDialog
    {
        [CascadingParameter] MudDialogInstance MudDialog { get; set; }

        [Parameter] public string PromptMessage { get; set; }

        void Close() => MudDialog.Close(DialogResult.Ok(true));

        public Boolean ShowPrompt;

        public void Show(string? promptMessage)
        {
            /*
            if (promptMessage != null)
                PromptMessage = promptMessage;

            ShowPrompt = true;
            StateHasChanged();
            */
        }
    }
}
