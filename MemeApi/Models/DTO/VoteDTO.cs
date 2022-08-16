using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO
{
    public class VoteDTO
    {
        [Required]
        public List<int> ElementIDs { get; set; }

        [Required]
        public Upvote UpVote { get; set; }
    }
}
