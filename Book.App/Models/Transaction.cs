namespace Book.Models
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Transaction
    {
        public int TransactionId { get; set; }

        [JsonIgnore]
        public TransactionType? TransactionType { get; set; } = default!;

        public int TransactionTypeId { get; set; }

        public DateTime TransactionDate { get; set; }

        [Column(TypeName = "money")]
        public decimal Value { get; set; }

        public DateTime CreateDate { get; set; }

        public string? Notes { get; set; }

        [JsonIgnore]
        [NotMapped]
        public string? SummaryName { get; set; }

        [JsonIgnore]
        [NotMapped]
        public int SummaryTypeId { get; set; }

        [JsonIgnore]
        [NotMapped]
        public string? TransactionTypeName { get; set; }

        [JsonIgnore]
        [NotMapped]
        public int Order { get; set; }

        [JsonIgnore]
        [NotMapped]
        public string? SummaryColour { get; set; }

        [JsonIgnore]
        [NotMapped]
        public string? TypeColour { get; set; }
    }

    public class TransactionValidator : AbstractValidator<Transaction>
    {
        public TransactionValidator(IStringLocalizer<Resources.BookResources> localizer)
        {
            RuleFor(x => x.Value).NotNull().NotEqual(0).WithMessage(x => localizer["ValueNotZero"]);
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