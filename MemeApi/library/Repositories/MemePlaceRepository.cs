using MemeApi.library.Extensions;
using MemeApi.library.Services.Files;
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
        private readonly IFileSaver _fileSaver;
        private readonly IFileLoader _fileLoader;
        public MemePlaceRepository(MemeContext context, IFileSaver fileSaver, IFileLoader loader)
        {
            _context = context;
            _fileSaver = fileSaver;
            _fileLoader = loader;
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

        public async Task<bool> DeleteSubmission(string submissionId)
        {
            var submission = await _context.PlaceSubmissions.FindAsync(submissionId);
            if (submission == null) return false;


            _context.PlaceSubmissions.Remove(submission);
            await _context.SaveChangesAsync();

            return true;
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

        public async Task<bool> RenderDelta(MemePlace place)
        {
            var latestRender = await _fileLoader.LoadFile($"places/{place.Id}_latest.png");
            if (latestRender == null) return false;
            
            var renderTime = latestRender.GetExifComment();
            if (renderTime == null) return false;
            
            var renderDateTime = DateTime.Parse(renderTime);
            var submissions = place.PlaceSubmissions.Where(p => renderDateTime < p.CreatedAt).ToList();
            if (!submissions.Any()) return true;

            var newRender = latestRender.ToRenderedPlaceWithBase(submissions);

            await _fileSaver.SaveFile(newRender, "places/", $"{place.Id}_latest.png", "image/png");
            return true;
        }

        public async Task ReRender(MemePlace place)
        {
            await _fileSaver.SaveFile(place.ToRenderedPlace(), "places/", $"{place.Id}_latest.png", "image/png");
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
