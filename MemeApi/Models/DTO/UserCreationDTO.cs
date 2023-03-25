using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO
{
    /// <summary>
    /// A DTO for creating a new user
    /// </summary>
    public class UserCreationDTO
    {
        /// <summary>
        /// Username of the user
        /// </summary>
        [Required]
        public string Username { get; set; }
        /// <summary>
        /// Email of the user
        /// </summary>
        [Required]
        public string Email { get; set; }
        /// <summary>
        /// Password of the user
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}