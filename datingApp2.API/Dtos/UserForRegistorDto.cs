using System.ComponentModel.DataAnnotations;

namespace datingApp2.API.Dtos
{
    public class UserForRegistorDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify between 4 and 8 characters")]
        public string Password { get; set; }
    }
}