using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO
{
    public class VoteDTO
    {
        [Required]
        public long ElementID { get; set; }
        [Required]
        public long UserID { get; set; }
        [Required]
        public bool? UpVote { get; set; }
    }
}
