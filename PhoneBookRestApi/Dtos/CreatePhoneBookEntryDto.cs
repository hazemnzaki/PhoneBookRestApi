using System.ComponentModel.DataAnnotations;

namespace PhoneBookRestApi.Dtos
{
    public class CreatePhoneBookEntryDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
