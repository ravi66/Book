using FluentValidation;

namespace Book.Models
{
    public class TransactionType
    {
        public int TransactionTypeId { get; set; }

        public int SummaryTypeId { get; set; }

        public SummaryType SummaryType { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreateDate { get; set; }

        public List<Transaction> Transactions { get; set; } = [];

        [NotMapped]
        public int TransactionCount { get; set; }
    }

    public class TransactionTypeValidator : AbstractValidator<TransactionType>
    {
        public TransactionTypeValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<TransactionType>.CreateWithOptions((TransactionType)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid) return [];
            return result.Errors.Select(e => e.ErrorMessage);
        };

    }
}
