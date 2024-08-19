using Book.Pages;

namespace Book.Services
{
    public class PageParamsSvc()
    {
        public string Name { get; set; } = default!;
        public int Year { get; set; }
        public int Mode { get; set; }
        public List<int> Types { get; set; } = [];
        public int Month { get; set; }
        public int TransactionTypeId { get; set; }
        public int SummaryTypeId { get; set; }
        public List<Transaction> Transactions { get; set; } = [];
        public string PageTitle { get; set; } = default!;

        public void Init()
        {
            Name = string.Empty;
            Year = 0;
            Mode = 0;
            Types = [];
            Month = 0;
            TransactionTypeId = 0;
            SummaryTypeId = 0;
            Transactions = [];
            PageTitle = string.Empty;
        }
    }
}