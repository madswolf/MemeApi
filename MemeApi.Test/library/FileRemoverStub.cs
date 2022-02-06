using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemeApi.library;

namespace MemeApi.Test.library
{
    internal class FileRemoverStub : IFileRemover
    {
        public void RemoveFile(string path)
        {
            return;
        }
    }
}
