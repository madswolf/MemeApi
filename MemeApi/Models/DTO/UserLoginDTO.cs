using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models
{
    public class UserLoginDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string password { get; set; }
    }
}