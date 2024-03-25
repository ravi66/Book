using FluentValidation;

namespace Book.Models
{
    public class SummaryType
    {
        public int SummaryTypeId { get; set; }

        [Label("Name")]
        public string Name { get; set; }

        [Label("Order")]
        public int Order { get; set; }

        [Label("Date Created")]
        public DateTime CreateDate { get; set; }

        public ICollection<TransactionType>? TransactionTypes { get; set; }

        [NotMapped]
        public List<int>? Types { get; set; }

        [NotMapped]
        public List<TransactionType>? TransactionTypeList { get; set; }

        [NotMapped]
        public bool ShowTransactionTypes { get; set; }
    }

    public class SummaryTypeValidator : AbstractValidator<SummaryType>
    {
        public SummaryTypeValidator()
        { 
            RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<SummaryType>.CreateWithOptions((SummaryType)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid) return [];
            return result.Errors.Select(e => e.ErrorMessage);
        };
    }
}
