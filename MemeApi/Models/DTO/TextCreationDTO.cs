using MemeApi.Models.Entity;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO
{
    public class TextCreationDTO
    {
        [Required]
        public string Text { get; set; }

        [Required]
        public MemeTextPosition position { get; set; }
    }
}
