using FluentValidation;

namespace Book.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }

        public TransactionType? TransactionType { get; set; }

        public int? TransactionTypeId { get; set; }

        [NotMapped]
        [Label("Type")]
        public string? TransactionTypeName { get; set; }

        [Label("Dated")]
        public DateTime TransactionDate { get; set; }

        [Column(TypeName = "money")]
        [Label("Value")]
        public decimal Value { get; set; }

        [Label("Date Created")]
        public DateTime CreateDate { get; set; }

        [Label("Notes")]
        public string? Notes { get; set; }
    }

    public class TransactionValidator : AbstractValidator<Transaction>
    {
        public TransactionValidator()
        {
            RuleFor(x => x.Value).NotNull().NotEqual(0);
            RuleFor(x => x.TransactionDate).NotEmpty();
            RuleFor(x => x.TransactionTypeId).NotNull().NotEqual(0);
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<Transaction>.CreateWithOptions((Transaction)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid) return [];
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}