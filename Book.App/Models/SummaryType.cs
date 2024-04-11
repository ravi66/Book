using FluentValidation;

namespace Book.Models
{
    public class SummaryType
    {
        public int SummaryTypeId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }

        public DateTime CreateDate { get; set; }

        public List<TransactionType> TransactionTypes { get; set; } = [];
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
