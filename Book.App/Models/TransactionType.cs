using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book.Models
{
    public class TransactionType
    {
        public int TransactionTypeId { get; set; }

        public int? SummaryTypeId { get; set; }

        public SummaryType? SummaryType { get; set; }

        [NotMapped]
        [Display(Name = "Summary")]
        public string? SummaryName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Name is too long [50].")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "Created")]
        public DateTime CreateDate { get; set; }

        public ICollection<Transaction>? Transactions { get; set; }

        [NotMapped]
        public int TransactionCount { get; set; }

    }
}
