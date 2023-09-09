using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO for Updating user information
/// </summary>
/// <param name="NewUsername"> New username </param>
/// <param name="NewEmail"> New email </param>
/// <param name="CurrentPassword"> Current password </param>
/// <param name="NewPassword"> New password </param>
/// <param name="NewProfilePic"> New profile picture </param>
public record UserUpdateDTO([property: Required] string NewUsername, string NewEmail, string CurrentPassword, string NewPassword, IFormFile NewProfilePic);
