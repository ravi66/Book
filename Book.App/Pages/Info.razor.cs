namespace Book.Pages
{
    public partial class Info
    {
        [Inject] internal IBookSettingSvc BookSettingSvc { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        private string Version { get; set; } = Constants.BookVersion;

        private string BookName { get; set; } = string.Empty;

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();
        }
    }
}