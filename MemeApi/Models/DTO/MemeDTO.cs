using MemeApi.Models.Entity;
using System;
using System.Collections.Generic;

namespace MemeApi.Models.DTO
{
    /// <summary>
    /// A DTO for memes
    /// </summary>
    public class MemeDTO
    {
        public int Id { get; set; }
        /// <summary>
        /// Visual component of the meme
        /// </summary>
        public string MemeVisual { get; set; }
        /// <summary>
        /// Textual top component of the meme
        /// </summary>
        public string Toptext { get; set; }
        /// <summary>
        /// Textual bottom component of the meme
        /// </summary>
        public string BottomText { get; set; }
        /// <summary>
        /// Topics that the meme belongs to
        /// </summary>
        public List<string> Topics { get; set; }
        public DateTime CreatedAt { get; set; } 
    }
}
