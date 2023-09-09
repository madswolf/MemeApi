using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Models.DTO;

//public IFormFile SoundFile { get; set; }
/// <summary>
/// A DTO with information to create a meme 
/// </summary>
/// <param name="VisualFile"> The visual component of the meme </param>
/// <param name="Toptext"> The textual top component of the meme </param>
/// <param name="Bottomtext"> The textual bottom component of the meme </param>
/// <param name="FileName"> Optional name for the visual component </param>
/// <param name="Topics"> The list of topics that the Meme belongs to. </param>
public record MemeCreationDTO([property: Required] IFormFile VisualFile, string Toptext, string Bottomtext, string FileName, IEnumerable<string> Topics);
