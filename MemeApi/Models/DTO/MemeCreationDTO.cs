using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MemeApi.Models.Entity;

namespace MemeApi.Models.DTO
{
    public class MemeCreationDTO
    {
        [Required]
        public string VisualFile { get; set; }
        public string SoundFile { get; set; }
        public List<(string, string)> Texts { get; set; }
    }
}
