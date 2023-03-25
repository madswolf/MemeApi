namespace MemeApi.Models.DTO
{
    /// <summary>
    /// A DTO for a random meme or component.
    /// </summary>
    public class RandomComponentDTO
    {
        /// <summary>
        /// The data or content of the component
        /// </summary>
        public string data { get; set; }
        /// <summary>
        /// The components ID
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// The vote score of the component
        /// </summary>
        public int votes { get; set; }
    }
}
