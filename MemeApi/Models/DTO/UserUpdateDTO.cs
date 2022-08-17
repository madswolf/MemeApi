using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Models.DTO
{
    public class UserUpdateDTO
    {

        [Required]
        public string NewUsername { get; set; }
        public string NewEmail { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public IFormFile NewProfilePic { get; set; }
    }
}