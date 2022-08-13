using Microsoft.Build.Framework;

namespace MemeApi.Models.DTO
{
    public class TopicCreationDTO
    {
        [Required]
        public string TopicName { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
