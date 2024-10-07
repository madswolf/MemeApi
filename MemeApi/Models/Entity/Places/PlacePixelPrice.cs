using System;

namespace MemeApi.Models.Entity.Places
{
    public class PlacePixelPrice
    {
        public string Id { get; set; }
        public string PlaceId { get; set; }
        public MemePlace Place { get; set; }
        public double PricePerPixel { get; set; }
        public DateTime PriceChangeTime { get; set; }
    }
}
