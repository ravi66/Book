using Microsoft.AspNetCore.Components;

namespace Book.Components
{
    public partial class PromptDialog
    {
        public bool ShowPrompt { get; set; }

        public string PromptMessage { get; set; } = "";

        private ElementReference OKButton;

        public void Show(string? promptMessage)
        {
            ShowPrompt = true;

            if (promptMessage != null)
                PromptMessage = promptMessage;

            StateHasChanged();
        }

        public void Close()
        {
            ShowPrompt = false;
            StateHasChanged();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (ShowPrompt)
            {
                await OKButton.FocusAsync();
            }
        }

    }
}
