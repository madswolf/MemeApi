using MemeApi.library.repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using MemeApi.Models.DTO;

namespace MemeApi.library.Repositories
{
    public class MemePlaceRepository
    {
        private readonly MemeContext _context;
        public MemePlaceRepository(MemeContext context)
        {
            _context = context;
        }

        public async Task<List<MemePlace>> GetMemePlaces()
        {
            return await _context.MemePlaces
                .Include(m => m.PlaceSubmissions)
                .ToListAsync();
        }

        public async Task<MemePlace?> GetMemePlace(string placeId)
        {
            return await _context.MemePlaces
                .Include(m => m.PlaceSubmissions)
                .FirstOrDefaultAsync(p => p.Id == placeId);
        }

        public async Task<List<PlaceSubmission>> GetMemePlaceSubmissions(string placeId)
        {
            return await _context.PlaceSubmissions
                .Include(p => p.Owner)
                .Where(p => p.PlaceId == placeId)
                .ToListAsync();
        }

        public async Task<PlaceSubmission> CreatePlaceSubmission(MemePlace place, User submitter, IFormFile file)
        {
            var place = new MemePlace
            {

            }
        }
    }
}
