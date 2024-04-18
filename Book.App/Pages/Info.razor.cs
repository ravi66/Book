namespace Book.Pages
{
    public partial class Info
    {
        [Inject] internal IBookSettingSvc BookSettingSvc { get; set; }

        [Inject] public NavigationManager NavigationManager { get; set; }

        [Inject] HttpClient HttpClient { get; set; }

        private string Version { get; set; } = Constants.BookVersion;

        private string BookName { get; set; } = string.Empty;

        private int VisitCount { get; set; }

        protected async override Task OnInitializedAsync()
        {
            BookName = await BookSettingSvc.GetBookName();

            VisitsCounter visitsCounter = new(HttpClient, new Uri(NavigationManager.Uri).Host);
            VisitCount = await visitsCounter.GetVisitsCount();
        }
    }
}