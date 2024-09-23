using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MemeApi.library.Repositories
{
    public class MemePlaceRepository
    {
        private readonly MemeContext _context;
        public MemePlaceRepository(MemeContext context)
        {
            _context = context;
        }

        public async Task<MemePlace> CreateMemePlace(PlaceCreationDTO placeCreationDTO)
        {
            var place = new MemePlace
            {
                Id = Guid.NewGuid().ToString(),
                Name = placeCreationDTO.Name,
                Height = placeCreationDTO.Height,
                Width = placeCreationDTO.Width,
                PlaceSubmissions = []
            };

            _context.MemePlaces.Add(place);
            await _context.SaveChangesAsync();
            return place;
        }

        public async Task<List<MemePlace>> GetMemePlaces()
        {
            return await _context.MemePlaces
                .Include(m => m.PlaceSubmissions)
                .ThenInclude(s => s.Owner)
                .ToListAsync();
        }

        public async Task<MemePlace?> GetMemePlace(string placeId)
        {
            return await _context.MemePlaces
                .Include(m => m.PlaceSubmissions)
                .ThenInclude(s => s.Owner)
                .FirstOrDefaultAsync(p => p.Id == placeId);
        }

        public async Task<List<PlaceSubmission>> GetMemePlaceSubmissions(string placeId)
        {
            return await _context.PlaceSubmissions
                .Include(p => p.Owner)
                .Where(p => p.PlaceId == placeId)
                .ToListAsync();
        }

        public async Task<PlaceSubmission?> GetPlaceSubmission(string submissionId)
        {
            return await _context.PlaceSubmissions.FirstOrDefaultAsync(s => s.Id == submissionId);
        }

        public async Task<PlaceSubmission> CreatePlaceSubmission(MemePlace place, User submitter, Dictionary<Coordinate, Color> pixelSubmissions)
        {
            var submission = new PlaceSubmission
            {
                Id = Guid.NewGuid().ToString(),
                Place = place,
                Owner = submitter,
                PixelSubmissions = pixelSubmissions.Select(pair => new Pixel
                {
                    Coordinate = pair.Key,
                    Color = pair.Value
                }).ToList(),
            };

            var dubloonEvent = new PlacePixelPurchase
            {
                Id = Guid.NewGuid().ToString(),
                Owner = submitter,
                Submission = submission,
                Dubloons = -Math.Ceiling(pixelSubmissions.Count / 100.0)
            };

            _context.PlaceSubmissions.Add(submission);
            _context.DubloonEvents.Add(dubloonEvent);
            _context.SaveChanges();
            return submission;
        }
    }
}
