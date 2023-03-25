using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Models.DTO
{
    /// <summary>
    /// A DTO with information to create a meme 
    /// </summary>
    public class MemeCreationDTO
    {
        /// <summary>
        /// The visual component of the meme
        /// </summary>
        [Required]
        public IFormFile VisualFile { get; set; }
        //public IFormFile SoundFile { get; set; }
        /// <summary>
        /// The textual top component of the meme
        /// </summary>
        public string Toptext { get; set; }
        /// <summary>
        /// The textual bottom component of the meme
        /// </summary>
        public string Bottomtext { get; set; }
        /// <summary>
        /// Optional name for the visual compo´nent
        /// </summary>
        public string FileName { get; set; }
    }
}
