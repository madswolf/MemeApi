using MemeApi.Models.DTO;

namespace MemeApi.Models.Entity;

public class PlacePixelPurchase : DubloonEvent
{
    public string SubmissionId { get; set; }
    public PlaceSubmission Submission { get; set; }
    public override DubloonEventInfoDTO ToDubloonEventInfoDTO() => new DubloonEventInfoDTO
    (
        Id,
        Owner.UserName,
        "PlacePixelPurchase",
        (int)Dubloons
    );
}
