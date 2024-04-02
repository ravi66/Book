using FluentValidation;
using static MudBlazor.CategoryTypes;

namespace Book.Models
{
    public class BookSetting
    {
        public int BookSettingId { get; set; }

        public bool UserAmendable { get; set; }

        [Required]
        public string SettingName { get; set; }

        public string? SettingValue { get; set; }
    }

    public class BookSettingValidator : AbstractValidator<BookSetting>
    {
        public BookSettingValidator()
        {
            RuleFor(x => x.SettingValue).NotEmpty().MaximumLength(50);

            RuleFor(x => x.SettingValue).Cascade(CascadeMode.Stop).Custom((x, context) =>
            {
                if ((!(int.TryParse(x, out int value)) || value < 0))
                {
                    context.AddFailure($"{x} is not a valid number or less than 0");
                }
            }).When(x => x.BookSettingId == 3 || x.BookSettingId == 4);
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<BookSetting>.CreateWithOptions((BookSetting)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid) return [];
            return result.Errors.Select(e => e.ErrorMessage);
        };

        public async Task<IEnumerable<string>> ValidateValueAsync(object model, string propertyName)
        {
            var result = await ValidateAsync(ValidationContext<BookSetting>.CreateWithOptions((BookSetting)model, x => x.IncludeProperties(propertyName)));
            if (result.IsValid) return [];
            return result.Errors.Select(e => e.ErrorMessage);
        }
    }
}
