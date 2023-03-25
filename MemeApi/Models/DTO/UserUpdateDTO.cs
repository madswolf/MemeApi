using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Models.DTO
{
    /// <summary>
    /// A DTO for Updating user information
    /// </summary>
    public class UserUpdateDTO
    {
        /// <summary>
        /// New username
        /// </summary>
        [Required]
        public string NewUsername { get; set; }
        /// <summary>
        /// New email
        /// </summary>
        public string NewEmail { get; set; }
        /// <summary>
        /// Current password
        /// </summary>
        public string CurrentPassword { get; set; }
        /// <summary>
        /// New password
        /// </summary>
        public string NewPassword { get; set; }
        /// <summary>
        /// New profile picture
        /// </summary>
        public IFormFile NewProfilePic { get; set; }
    }
}