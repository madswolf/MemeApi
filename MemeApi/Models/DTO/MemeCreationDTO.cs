using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO
{
    public class MemeCreationDTO
    {
        [Required]
        public string VisualFile { get; set; }
        public string SoundFile { get; set; }
        public string Toptext { get; set; }
        public string BottomText { get; set; }
    }
}
