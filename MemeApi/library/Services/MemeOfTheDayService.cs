namespace MemeApi.library.Services;

using MemeApi.library.repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Http;
using System.Net.Http;

public class MemeOfTheDayService : IMemeOfTheDayService
{
    private readonly MemeRepository _memeRepository;
    private readonly VisualRepository _visualRepository;
    private readonly TextRepository _textRepository;
    private readonly TopicRepository _topicRepository;
    private readonly IConfiguration _configuration;
    private readonly IMemeRenderingService _memeRenderingService;
    private readonly IMailSender _mailSender;

    public MemeOfTheDayService(MemeRepository memeRepository, VisualRepository visualRepository, TextRepository textRepository, TopicRepository topicRepository, IConfiguration configuration, IMemeRenderingService memeRenderingService, IMailSender mailSender)
    {
        _memeRepository = memeRepository;
        _visualRepository = visualRepository;
        _textRepository = textRepository;
        _topicRepository = topicRepository;
        _configuration = configuration;
        _memeRenderingService = memeRenderingService;
        _mailSender = mailSender;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Your daily logic here
        var visual = await _visualRepository.GetRandomVisual();
        var toptext = await _textRepository.GetRandomText();
        var bottomtext = await _textRepository.GetRandomText();

        var topic = await _topicRepository.GetTopicByName("MemeOfTheDay");

        var meme = await _memeRepository.UpsertByComponents(visual, toptext, bottomtext, topic);
        var wekhookUrl = _configuration["MemeOfTheDay.WebHookURL"];

        using (HttpClient httpClient = new HttpClient())
        {
            MultipartFormDataContent form = new MultipartFormDataContent();
            var imageContent = _memeRenderingService.RenderMeme(meme);
            form.Add(new ByteArrayContent(imageContent, 0, imageContent.Length), "image/png", "shit.png");
            await httpClient.PostAsync(wekhookUrl, form);
            httpClient.Dispose();
        }



        //TODO: add subscribers
        var recipient = new MailAddress("mads_1997@live.dk");

        //_mailSender.sendMemeOfTheDayMail(recipient, _memeRenderingService.RenderMeme(meme));
    }
}

