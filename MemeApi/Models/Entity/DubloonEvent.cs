using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MemeApi.Models.Entity
{
    public class DubloonEvent
    {
        public string Id { get; set; }
        public EventType eventType { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime EventTimestamp { get; set; }
        public string ReferenceEntityId { get; set; }
        public string UserId { get; set; }
        public User Owner {  get; set; }
        public double Dubloons { get; set; }
    }
}
