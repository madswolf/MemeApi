using System;
using System.Collections.Generic;

namespace MemeApi.Models.DTO
{
    public class VisualDTO
    {
        /// <summary>
        /// Filename of the Visual
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// Topics that the Visual are in
        /// </summary>
        public List<string> Topics { get; set; }
        /// <summary>
        /// The time at which the Visual was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
