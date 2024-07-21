namespace Book.Models
{
    [JsonObject(MemberSerialization.OptOut)]
    public class TransactionType
    {
        public int TransactionTypeId { get; set; }

        public int SummaryTypeId { get; set; }

        [JsonIgnore]
        public SummaryType SummaryType { get; set; } = default!;

        public string Name { get; set; } = string.Empty;

        public DateTime CreateDate { get; set; }

        public string? ChartColour { get; set; }

        [JsonIgnore]
        public List<Transaction> Transactions { get; set; } = [];

        [JsonIgnore]
        [NotMapped]
        public bool TransactionsFound { get; set; }
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