using System;
using System.Collections.Generic;

namespace MemeApi.Models.DTO;

/// <summary>
/// A DTO for memes
/// </summary>
/// <param name="Id"></param>
/// <param name="MemeVisual"> Visual component of the meme </param>
/// <param name="Toptext"> Textual top component of the meme </param>
/// <param name="BottomText"> Textual bottom component of the meme </param>
/// <param name="Owner"> Owner/Creator of the meme </param>
/// <param name="Topics"> Topics that the meme belongs to </param>
/// <param name="CreatedAt"></param>
/// <param name="RenderedMeme"> Rendered meme, only returned if the renderMeme query parameter is set</param>
public record MemeDTO(string Id, VisualDTO MemeVisual, TextDTO? Toptext, TextDTO? BottomText, UserInfoDTO? Owner, List<string> Topics, DateTime CreatedAt, byte[]? RenderedMeme = null);
