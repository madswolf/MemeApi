using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.DTO.Places;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Dubloons;
using MemeApi.Models.Entity.Places;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace MemeApi.library.Repositories
{
    public class MemePlaceRepository
    {
        private readonly static ConcurrentDictionary<string, SemaphoreSlim> PlaceLocks = new();
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
            var defaultPixelPrice = new PlacePixelPrice
            {
                Id = Guid.NewGuid().ToString(),
                PricePerPixel = 0.01
            };

            var place = new MemePlace
            {
                Id = Guid.NewGuid().ToString(),
                Name = placeCreationDTO.Name,
                Height = placeCreationDTO.Height,
                Width = placeCreationDTO.Width,
                PlaceSubmissions = [],
                PriceHistory = [defaultPixelPrice]
            };

            defaultPixelPrice.Place = place;

            _context.MemePlaces.Add(place);
            await _context.SaveChangesAsync();
            return place;
        }

        public async Task<PlacePixelPrice?> ChangePrice(PriceChangeDTO priceChangeDTO)
        {
            var place = await GetMemePlace(priceChangeDTO.PlaceId);
            if (place == null) return null;

            var price = new PlacePixelPrice
            {
                Id = Guid.NewGuid().ToString(),
                PricePerPixel = priceChangeDTO.NewPricePerPixel,
                Place = place,
            };

            _context.PixelPrices.Add(price);
            await _context.SaveChangesAsync();
            return price;
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
                .Include(m => m.PriceHistory)
                .FirstOrDefaultAsync(p => p.Id == placeId);
        }

        public async Task<(bool, MemePlace?)> DeleteSubmission(string submissionId)
        {
            var submission = 
                await _context.PlaceSubmissions
                .Include(ps => ps.Place)
                .ThenInclude(p => p.PlaceSubmissions)
                .FirstOrDefaultAsync(ps => ps.Id == submissionId);

            if (submission == null) return (false, null);
            submission.IsDeleted = true;

            await _context.SaveChangesAsync();

            return (true, submission.Place);
        }

        public async Task<List<PlaceSubmission>> GetMemePlaceSubmissions(string placeId, bool includeDeleted = false)
        {
            var queryable = _context.PlaceSubmissions
                .Where(p => p.Id == placeId);

            if (!includeDeleted)
                queryable = queryable.Where(p => p.IsDeleted == false);

            return await queryable.Include(p => p.Owner).ToListAsync();
        }

        public async Task<PlaceSubmission?> GetPlaceSubmission(string submissionId)
        {
            return await _context.PlaceSubmissions.FirstOrDefaultAsync(s => s.Id == submissionId);
        }


        public async Task<bool> RerenderPlace(MemePlace place)
        {
            var lockObject = PlaceLocks.GetOrAdd(place.Id, new SemaphoreSlim(1, 1));
            await lockObject.WaitAsync();

            try
            {
                var submissionImages = 
                    await FetchSubmissionRenders(
                        place.PlaceSubmissions
                        .Where(p => p.IsDeleted == false)
                        .ToList()
                    );

                if (submissionImages == null || submissionImages.Any(s => s == null)) return false;

                var baseImage = new SKBitmap(place.Width, place.Height);
                var canvas = new SKCanvas(baseImage);
                canvas.Clear(SKColors.White);

                var render = RenderSubmissionsToBase(baseImage, submissionImages);

                await SavePlaceRender(render, place.Id);
            }
            finally { lockObject.Release(); }

            return true;
        }

        private static SKBitmap RenderSubmissionsToBase(SKBitmap baseImage, byte[][] submissionImages)
        {
            var canvas = new SKCanvas(baseImage);

            foreach (var submissionImage in submissionImages)
            {
                using var overlayBitmap = SKBitmap.Decode(submissionImage);
                canvas.DrawBitmap(overlayBitmap, 0, 0);

                canvas.Flush();
            }
            return baseImage;
        }

        private async Task<byte[]?[]?> FetchSubmissionRenders(
            List<PlaceSubmission> placeSubmissions)
        {
            return await Task.WhenAll(placeSubmissions.OrderBy(ps => ps.CreatedAt).Select(
                    async p => await GetPlaceSubmissionRender(p.Id)
                ));
        }

        public async Task<byte[]> GetLatestPlaceRender(string placeId)
        {
            return 
                (await _fileLoader.LoadFile($"places/{placeId}_latest.png"))
                .WriteExifComment(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public async Task<byte[]?> GetPlaceSubmissionRender(string submissionId)
        {
            return await _fileLoader.LoadFile($"placesubmissions/{submissionId}.png");
        }

        public async Task<(bool, byte[]?)> RenderDelta(MemePlace place)
        {
            var lockObject = PlaceLocks.GetOrAdd(place.Id, new SemaphoreSlim(1, 1));
            await lockObject.WaitAsync();

            try
            {
                var latestRender = await _fileLoader.LoadFile($"places/{place.Id}_latest.png");
                if (latestRender == null) return (false, null);

                var renderTime = latestRender.GetExifComment();
                if (renderTime == null) return (false, null);

                var renderDateTime = DateTime.Parse(renderTime);
                var submissions =
                    place.PlaceSubmissions
                        .Where(p => renderDateTime < p.CreatedAt && p.IsDeleted == false)
                        .ToList();

                if (submissions.Count == 0) return (true, null);

                var submissionImages = await FetchSubmissionRenders(submissions);
                if (submissionImages == null || submissionImages.Any(s => s == null)) return (false, null);

                using var latestRenderBitmap = SKBitmap.Decode(latestRender);

                var render = RenderSubmissionsToBase(latestRenderBitmap, submissionImages);

                await SavePlaceRender(latestRenderBitmap, place.Id);
                return (true, render.ToByteArray());
            }
            catch { return (false, null); }
            
            finally
            {
                lockObject.Release();
            }
        }

        private async Task SavePlaceRender(SKBitmap latestRenderBitmap, string placeId)
        {
            var imageBytes = latestRenderBitmap.ToByteArray();

            await _fileSaver.SaveFile(imageBytes, "places/", $"{placeId}_latest.png", "image/png");
        }

        public async Task<PlaceSubmission> CreatePlaceSubmission(MemePlace place, User submitter, Dictionary<Coordinate, Color> pixelSubmissions, IFormFile submissionImage, double price)
        {
            var submission = new PlaceSubmission
            {
                Id = Guid.NewGuid().ToString(),
                Place = place,
                Owner = submitter,
                PixelChangeCount = pixelSubmissions.Count,
                IsDeleted = false,
            };

            var pixelSubmissionsImage = pixelSubmissions.ToRenderedSubmission(place);
            await _fileSaver.SaveFile(pixelSubmissionsImage, "placesubmissions/", $"{submission.Id}.png", "image/png");
            await _fileSaver.SaveFile(submissionImage.ToByteArray(), "placesubmissions/backups/", $"{submission.Id}.png", "image/png");

            var dubloonEvent = new PlacePixelPurchase
            {
                Id = Guid.NewGuid().ToString(),
                Owner = submitter,
                Submission = submission,
                Dubloons = -price
            };

            _context.PlaceSubmissions.Add(submission);
            _context.DubloonEvents.Add(dubloonEvent);
            _context.SaveChanges();
            return submission;
        }
    }
}
