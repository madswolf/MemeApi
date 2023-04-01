using MemeApi.Models.Entity;
using System;
using System.Collections.Generic;

namespace MemeApi.Models.DTO
{
    /// <summary>
    /// A DTO for textual meme components
    /// </summary>
    public class TextDTO
    {
        /// <summary>
        /// Id of the Text
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Actual content of the text
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Position of the textual component
        /// </summary>
        public MemeTextPosition Position { get; set; }
        /// <summary>
        /// Topics that the text belongs to
        /// </summary>
        public List<string> Topics { get; set; }
        /// <summary>
        /// The time at which the Text was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

    }
}
