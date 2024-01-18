using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book.Models
{
    public class SummaryType
    {
        public int SummaryTypeId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Name is too long [50].")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        public int Order { get; set; }

        public DateTime CreateDate { get; set; }

        public ICollection<TransactionType>? TransactionTypes { get; set; }

        [NotMapped]
        public List<int>? Types { get; set; }

        [NotMapped]
        public List<TransactionType>? TransactionTypeList { get; set; }

        [NotMapped]
        public bool ShowTransactionTypes { get; set; }

    }
}
