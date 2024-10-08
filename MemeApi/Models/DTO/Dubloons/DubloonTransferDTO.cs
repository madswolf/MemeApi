﻿namespace MemeApi.Models.DTO.Dubloons;
/// <summary>
/// A DTO of user info
/// </summary>
/// <param name="OtherUserId"> The other party user</param>
/// <param name="ExternalUserName"> The other party's user name</param>
/// <param name="DubloonsToTransfer"> The number of dubloons to transfer to the other user </param>
public class DubloonTransferDTO
{
    public string OtherUserId { get; set; }
    public string OtherUserName { get; set; }
    public uint DubloonsToTransfer { get; set; }
}
