using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Book.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }

        public TransactionType? TransactionType { get; set; }

        public int? TransactionTypeId { get; set; }

        [NotMapped]
        [Display(Name = "Type")]
        public string? TransactionTypeName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/YYYY}")]
        [Display(Name = "Dated")]
        public DateTime TransactionDate { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        [Display(Name = "Value")]
        [RegularExpression("(.*[1-9].*)|(.*[.].*[1-9].*)", ErrorMessage = "Value can not be zero")]
        public decimal Value { get; set; }

        [Display(Name = "Created")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

    }
}
