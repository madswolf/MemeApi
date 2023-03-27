using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO
{
    /// <summary>
    /// A DTO with information for logging into a user
    /// </summary>
    public class UserLoginDTO
    {
        /// <summary>
        /// Username or email of the user
        /// </summary>
        [Required]
        public string Username { get; set; }
        
        /// <summary>
        /// Password of the user
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}