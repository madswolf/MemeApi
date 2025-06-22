using MemeApi.Models.DTO.Dubloons;
using MemeApi.Models.Entity.Places;

namespace MemeApi.Models.Entity.Dubloons;

public class PlacePixelPurchase : DubloonEvent
{
    public string SubmissionId { get; set; }
    public PlaceSubmission Submission { get; set; }
    public override DubloonEventInfoDTO ToDubloonEventInfoDTO() => new(
        Id,
        Owner.UserName,
        "PlacePixelPurchase",
        (int)Dubloons
    );
}
