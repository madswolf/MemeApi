using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemeApi.library;
using Microsoft.AspNetCore.Http;

namespace MemeApi.Test.library
{
    internal class FileSaverStub : IFileSaver
    {
        public void SaveFile(IFormFile file, string path)
        {
            return;
        }
    }
}
