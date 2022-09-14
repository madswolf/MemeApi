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
        public async Task<ActionResult<MemeText>> GetMemeBottomText(int id)
        {
            var memeBottomText = await _textRepository.GetText(id);

            if (memeBottomText == null)
            {
                return NotFound();
            }

            return Ok(memeBottomText);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMemeText(int id, string newMemeBottomText, MemeTextPosition? newMemeTextPosition = null)
        {
            var memeText = await _textRepository.UpdateText(id, newMemeBottomText, newMemeTextPosition);

            if (!memeText) return NotFound();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<MemeText>> CreateMemeBottomText(string text, MemeTextPosition position)
        {
            var memeText = await _textRepository.CreateText(text, position);
            return CreatedAtAction("CreateMemeBottomText", new { id = memeText.Id }, memeText);
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
            var text = (await _textRepository.GetTexts(type)).RandomItem();
            return Ok(_mapper.Map<MemeText, RandomComponentDTO>(text));
        }

    }
}
