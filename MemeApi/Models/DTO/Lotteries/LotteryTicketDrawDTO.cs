using System.Collections.Generic;

namespace MemeApi.Models.DTO.Lotteries;

public record LotteryTicketDrawDTO
{
        public List<LotterItemPreviewDTO> Items { get; init; }
        public LotterItemWinDTO DrawnItemWin { get; init; }
        public bool WasFree { get; init; }
}

public record LotterItemPreviewDTO
{
    public string ItemThumbnailURL { get; init; }
    public string ItemRarityColor { get; init; }
}

public record LotterItemWinDTO
{
    public string ItemThumbnailURL { get; init; }
    public string ItemPictureURL { get; init; }
    public int ItemRarity { get; init; }
    public string ItemRarityColor { get; init; }
    public string ItemName { get; init; }
}