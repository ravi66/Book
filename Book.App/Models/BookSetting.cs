using System.ComponentModel.DataAnnotations;

namespace Book.Models
{
    public class BookSetting
    {
        public int BookSettingId { get; set; }

        public bool UserAmendable { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Setting Name is too long [50].")]
        [Display(Name = "Setting")]
        public string SettingName { get; set; }

        [StringLength(255, ErrorMessage = "Value is too long [255].")]
        [Display(Name = "Value")]
        public string? SettingValue { get; set; }

    }
}
