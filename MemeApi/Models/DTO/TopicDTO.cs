using System;
using System.Collections.Generic;

namespace MemeApi.Models.DTO
{
    /// <summary>
    /// A DTO for a topic
    /// </summary>
    public class TopicDTO
    {
        /// <summary>
        /// Topic Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Topic name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description of the topic
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Topic owner name
        /// </summary>
        public string Owner { get; set; }
        /// <summary>
        /// Topic moderator names
        /// </summary>
        public List<string> Moderators { get; set; }
        /// <summary>
        /// Topic creation time
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Latest update to topic
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
