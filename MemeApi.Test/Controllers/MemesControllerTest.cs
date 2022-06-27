using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.library;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MemeApi.Test.Controllers
{
    public class MemesControllerTest
    {
        //[Fact]
        //public async Task GIVEN_Visual_WHEN_CreatingMeme_THEN_MemeIsCreated()
        //{
        //    var context = ContextUtils.CreateMemeTestContext();
        //    var controller = CreateMemesController(context);

        //    // given
        //    var filename = "test.png";

        //    var memeCreationDTO = new MemeCreationDTO()
        //    {
        //        VisualFile = CreateFormFile(5, filename),
        //    };
            

        //    // When
        //    var creationTask = controller.PostMeme(memeCreationDTO);

        //    // Then
        //    var createdMeme = await ActionResultUtils.ActionResultToValueAndAssertCreated(creationTask);

        //    (await context.Memes.CountAsync()).Should().Be(1);
        //    (await context.Visuals.CountAsync()).Should().Be(1);

        //    createdMeme.MemeVisual.Filename.Should().Be(filename);
        //}

        public static IFormFile CreateFormFile(int size, string filename)
        {
            var fileStream = new MemoryStream(size);
            var file = new FormFile(fileStream, 0, size, "filestream", filename);
            return file;
        }

        //public static MemesController CreateMemesController(MemeContext context)
        //{
        //    var visualsController = new MemeVisualsController(context,new FileSaverStub(), new FileRemoverStub());
        //    var soundsController = new MemeSoundsController(context);
        //    var textsController = new MemeTextsController(context);
        //    return new MemesController(context, visualsController, soundsController, textsController);
        //}
        
    }
}
