using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.Models;
using MemeApi.Models.DTO;

namespace MemeApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TextsController : ControllerBase
    {
        private readonly TextRepository _textRepository;
        private readonly IMapper _mapper;

        public TextsController( TextRepository textRepository, IMapper mapper)
        {
            _textRepository = textRepository;
            _mapper = mapper;
        }

        [HttpGet("{type?}")]
        public async Task<ActionResult<IEnumerable<MemeText>>> GetTexts(MemeTextPosition? type = null) => await _textRepository.GetTexts(type);

        [HttpGet("one/{id}")]
        public async Task<ActionResult<MemeText>> GetMemeText(int id)
        {
            var memeBottomText = await _textRepository.GetText(id);

            if (memeBottomText == null) return NotFound();
            

            var componentDTO = new RandomComponentDTO
            {
                data = memeBottomText.Text,
                id = memeBottomText.Id
            };

            return Ok(componentDTO);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMemeText(int id, string newMemeBottomText, MemeTextPosition? newMemeTextPosition = null)
        {
            var memeText = await _textRepository.UpdateText(id, newMemeBottomText, newMemeTextPosition);

            if (!memeText) return NotFound();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<MemeText>> CreateMemeText([FromBody] TextCreationDTO textCreationDTO)
        {
            var memeText = await _textRepository.CreateText(textCreationDTO.Text, textCreationDTO.position);
            return CreatedAtAction("CreateMemeText", new { id = memeText.Id }, memeText);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMemeBottomText(int id)
        {
            var removed = await _textRepository.RemoveText(id);

            if (!removed) return NotFound();
            return NoContent();
        }

        [HttpGet]
        [Route("random/{type}")]
        public async Task<ActionResult<RandomComponentDTO>> RandomText(MemeTextPosition type)
        {
            var texts = await _textRepository.GetTexts(type);
            var text = texts.RandomItem();
            var randomDTO = new RandomComponentDTO
            {
                data = text.Text,
                id = text.Id,
                votes = text.SumVotes()
            };

            return Ok(randomDTO);
        }

    }
}
