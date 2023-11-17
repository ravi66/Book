using System.ComponentModel.DataAnnotations.Schema;

namespace Book.Models
{
    [NotMapped]
    public class MonthlySummary
    {
        public string MonthName { get; set; }

        public int MonthNo { get; set; }

        public List<SummaryDetail> SummaryDetails { get; set; }
    }

    [NotMapped]
    public class SummaryDetail
    {
        public int SummaryTypeId { get; set; }

        public string SummaryName { get; set; }

        public List<int> Types { get; set; }

        public decimal Total { get; set; }

        public string CssClass { get; set; }

        public bool HasTransactions { get; set; }
    }

    [NotMapped]
    public class ColumnInfo
    {
        public string Name { get; set; }

        public string InfoText { get; set; }

        public string MoreInfoToolTip { get; set; }
    }
}
