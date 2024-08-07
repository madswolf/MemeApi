﻿using System.Threading.Tasks;

namespace MemeApi.library.Services.Files;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface IFileRemover
{
    Task RemoveFile(string path);
}
