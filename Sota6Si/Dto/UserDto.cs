using System.ComponentModel.DataAnnotations;

namespace Sota6Si.Dto
{
    public class UserDto
    {
        [Required(ErrorMessage = "The Username field is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "The Password field is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "The PhoneNumber field is required.")]
        public string PhoneNumber { get; set; }
    }
}
