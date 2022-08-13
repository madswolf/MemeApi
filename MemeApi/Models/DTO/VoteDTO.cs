using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO
{
    public class VoteDTO
    {
        [Required]
        public int ElementID { get; set; }
        [Required]
        public int UserID { get; set; }
        [Required]
        public bool? UpVote { get; set; }
    }
}
