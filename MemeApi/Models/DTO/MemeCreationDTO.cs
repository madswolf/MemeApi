using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Models.DTO
{
    public class MemeCreationDTO
    {
        [Required]
        public IFormFile VisualFile { get; set; }
        public IFormFile SoundFile { get; set; }

        public string Toptext { get; set; }
        public string Bottomtext { get; set; }
    }
}
