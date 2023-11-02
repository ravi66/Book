using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Book.Components
{
    public partial class ConfirmDialog
    {
        protected bool ShowConfirmation { get; set; }

        public string ConfirmationTitle { get; set; }

        public string ConfirmationMessage { get; set; }

        public void Show(string? confirmationTitle, string? confirmationMessage)
        {
            ShowConfirmation = true;

            if (confirmationTitle == null)
                ConfirmationTitle = "Confirm";
            else
                ConfirmationTitle = confirmationTitle;

            if (confirmationMessage == null)
                ConfirmationMessage = "Are you sure you want to";
            else
                ConfirmationMessage = confirmationMessage; 

            StateHasChanged();
        }

        [Parameter]
        public EventCallback<bool> ConfirmationChanged { get; set; }

        protected async Task OnConfirmationChange(bool value)
        {
            ShowConfirmation = false;
            await ConfirmationChanged.InvokeAsync(value);
        }
        
    }
}
