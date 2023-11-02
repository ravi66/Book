using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book.Models
{
    [NotMapped]
    public class Summary
    {
        public string Name { get; set; }

        public string Title { get; set; }

        public int Count { get; set; }

        public int Order { get; set; }

        public List<int> Types { get; set; }

        public decimal Total { get; set; }

        public string TotCssClass { get; set; }

        public string InfoText { get; set; }

        public decimal Mat { get; set; }

        public decimal PTot { get; set; }

        public List<SummaryMonth> SummaryMonths { get; set; }

    }

    [NotMapped]
    public class SummaryMonth
    {
        public decimal Value { get; set; }
        public string CssClass { get; set; }
    }

}
