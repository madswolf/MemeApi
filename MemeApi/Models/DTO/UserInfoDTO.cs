using System.Collections.Generic;

namespace MemeApi.Models.DTO;


/// <summary>
/// A DTO of user info
/// </summary>
/// <param name="Id"> Users Id </param>
/// <param name="UserName"> Users name </param>
/// <param name="ProfilePicURl"> Users profile picture URL </param>
/// <param name="OwnedTopics"> Topics that the users owns </param>
public record UserInfoDTO(string Id, string UserName, string ProfilePicURl, List<string>? OwnedTopics);