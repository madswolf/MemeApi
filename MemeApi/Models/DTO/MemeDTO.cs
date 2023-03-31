using MemeApi.Models.Entity;
using System.Collections.Generic;

namespace MemeApi.Models.DTO
{
    /// <summary>
    /// A DTO for memes
    /// </summary>
    public class MemeDTO
    {
        /// <summary>
        /// Visual component of the meme
        /// </summary>
        public MemeVisual MemeVisual { get; set; }
        /// <summary>
        /// Textual top component of the meme
        /// </summary>
        public MemeText Toptext { get; set; }
        /// <summary>
        /// Textual bottom component of the meme
        /// </summary>
        public MemeText BottomText { get; set; }
        /// <summary>
        /// Topics that the meme belongs to
        /// </summary>
        public List<string> Topics { get; set; }
    }
}
