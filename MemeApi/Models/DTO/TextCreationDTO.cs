using MemeApi.Models.Entity;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO
{
    /// <summary>
    /// A DTO for creating text
    /// </summary>
    public class TextCreationDTO
    {
        /// <summary>
        /// The textual content
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        /// The texts position
        /// </summary>
        [Required]
        public MemeTextPosition position { get; set; }
    }
}
