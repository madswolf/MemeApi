using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MemeApi.Models.DTO
{
    ///<Summary>
    /// A DTO for Votes
    ///</Summary>
    public class VoteDTO
    {

        ///<Summary>
        /// Ids of the element(s) to vote on 
        ///</Summary>
        [Required]
        public List<int> ElementIDs { get; set; }

        ///<Summary>
        /// Upvote, Downvote or unvote
        ///</Summary>
        [Required]
        public Upvote UpVote { get; set; }
    }
}
