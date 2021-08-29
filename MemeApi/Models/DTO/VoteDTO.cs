namespace MemeApi.Models.DTO
{
    public class VoteDTO
    {
        public long ElementID { get; set; }
        public long UserID { get; set; }
        public bool Upvote { get; set; }
    }
}
