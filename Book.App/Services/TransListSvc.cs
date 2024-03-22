using Microsoft.AspNetCore.Components;

namespace Book.Services
{
    public class TransListSvc
    {
        public int Mode { get; set; }
        public string Name { get; set; }
        public List<int> Types { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int TransactionTypeId { get; set; }
        public string PreviousPage {  get; set; }
    }
}
