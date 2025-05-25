using System.ComponentModel.DataAnnotations;

namespace Sota6Si.Dto
{
    public class UserProjDto
    {
        [Required(ErrorMessage = "The Login field is required.")]
        public string Login { get; set; }

        [Required(ErrorMessage = "The Password field is required.")]
        public string Password { get; set; }

    }
}
