﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Memes;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.library.repositories;

public class VisualRepository
{
    private readonly MemeContext _context;
    private readonly TopicRepository _topicRepository;
    private readonly IFileSaver _fileSaver;
    private readonly IFileRemover _fileRemover;
    private readonly UserRepository _userRepository;
    public VisualRepository(MemeContext context, IFileSaver fileSaver, IFileRemover fileRemover, TopicRepository topicRepository, UserRepository userRepository)
    {
        _context = context;
        _fileSaver = fileSaver;
        _fileRemover = fileRemover;
        _topicRepository = topicRepository;
        _userRepository = userRepository;
    }

    public async Task<List<MemeVisual>> GetVisuals()
    {
        return await _context.Visuals
            .Include(x => x.Votes)
            .Include(x => x.Topics)
            .Include(x => x.Owner)
            .ToListAsync();
    }

    public MemeVisual GetRandomVisual(string seed = "")
    {
        var regex = new Regex("^.*\\.gif$");
        return _context.Visuals.ToList().Where(x => !regex.IsMatch(x.Filename)).ToList().RandomItem(seed);
    }

    public MemeVisual GetRandomVisualInTopic(Topic topic, string seed = "")
    {
        var regex = new Regex("^.*\\.gif$");
        return _context.Visuals.Include(v => v.Topics).Where(v => v.Topics.Contains(topic)).ToList().Where(x => !regex.IsMatch(x.Filename)).ToList().RandomItem(seed);
    }

    public async Task<MemeVisual?> GetVisual(string? id)
    {
        return await _context.Visuals.Include(x => x.Votes).Include(v => v.Owner).FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<MemeVisual> CreateMemeVisual(IFormFile visual, string filename, List<string>? topicNames = null, string? userId = null) 
    { 
        var user = await _userRepository.GetUser(userId);
        return await CreateMemeVisual(visual, filename, topicNames, user);
    }

    public async Task<MemeVisual> CreateMemeVisual(IFormFile visual, string filename, List<string>? topicNames = null, User? user = null)
    {   
        var (existingVotable, filteredTopics) =
            await _topicRepository.GetOrUpdateVotableIfExistsAndFilterTopics<MemeVisual>(visual.ToByteArray().ToContentHash(), user, topicNames);
        if (existingVotable != null) return existingVotable;
        var topics = await _topicRepository.GetTopicsByNameForUser(filteredTopics, user?.Id);

        var memeVisual = new MemeVisual
        {
            Id = Guid.NewGuid().ToString(),
            Filename = filename,
            Topics = topics,
            ContentHash = visual.ToByteArray().ToContentHash()
        };

        if(user != null) memeVisual.Owner = user;

        var skipUpload = false;
        var visuals = _context.Visuals.Where(x => x.Filename == memeVisual.Filename);
        if (visuals.Any())
        {
            var visualWithSameContent = visuals.FirstOrDefault(v =>
                v.ContentHash == memeVisual.ContentHash &&
                v.Filename == memeVisual.Filename);
            
            if (visualWithSameContent != null)
            {
                skipUpload = true;
            }
            else
            {
                memeVisual.Filename = memeVisual.Filename.PrependRandomString();
            }
        }

        if (!skipUpload)
        {
            using var memoryStream = new MemoryStream();
            await visual.CopyToAsync(memoryStream);
            await _fileSaver.SaveFile(memoryStream.ToArray(), "visual/", memeVisual.Filename, visual.ContentType);
        }

        _context.Visuals.Add(memeVisual);
        await _context.SaveChangesAsync();
        return memeVisual;
    }

    public async Task<bool> Delete(string id)
    {
        var memeVisual = await _context.Visuals.FindAsync(id);
        if (memeVisual == null)
        {
            return false;
        }

        await _fileRemover.RemoveFile(Path.Combine("visual/", memeVisual.Filename));

        _context.Visuals.Remove(memeVisual);
        await _context.SaveChangesAsync();

        return true;
    }
}
