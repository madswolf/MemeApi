using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.Entity.Memes;
using Newtonsoft.Json;

namespace MemeApi.library.Services;

public class MemeOfTheDayService : IMemeOfTheDayService
{
    private readonly MemeRepository _memeRepository;
    private readonly MemeApiSettings _settings;
    private readonly DiscordWebhookSender _discordSender;
    private readonly IMemeRenderingService _memeRenderingService;
    private readonly IMailSender _mailSender;

    public MemeOfTheDayService(MemeRepository memeRepository, IMemeRenderingService memeRenderingService, IMailSender mailSender, MemeApiSettings settings, DiscordWebhookSender discordSender)
    {
        _memeRepository = memeRepository;
        _memeRenderingService = memeRenderingService;
        _mailSender = mailSender;
        _settings = settings;
        _discordSender = discordSender;
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Meme meme = await _memeRepository.RandomMemeByComponents(topicName: _settings.GetMemeOfTheDayTopicName());
        var imageContent = await _memeRenderingService.RenderMeme(meme);
        
        var seedNumber = new Random().Next(10);
        var message = seedNumber != 1 ? "Meme Of the Day" : messages.RandomItem();

        var success = await _discordSender.SendMessageWithImage(
            imageContent,
            meme.ToFilenameString(),
            message,
            "Hjerneskade(Meme Of The Day)",
            $"{_settings.GetMediaHost()}default.jpg",
            _settings.GetMemeOfTheDayWebhook(),
            stoppingToken);
        
        if (!success)
        {
            var jsonResponse = JsonConvert.SerializeObject(meme.ToMemeDTO(_settings.GetMediaHost()));

            Console.Error.WriteLine("Failed meme");
            Console.Error.WriteLine(Regex.Replace(jsonResponse, @"[^\x20-\x7E]", "X"));
        }

        //TODO: add subscribers
        //_mailSender.sendMemeOfTheDayMail(recipient, _memeRenderingService.RenderMeme(meme));
    }
    
    // auto generated text messages
    private static readonly List<string> messages =
    [
            "Yo this one is fire 🔥🔥🔥",
            "SHEEEESH i am laughing at this one 😆😆😆😆",
            "This one is making me LOL, LMAO even",
            "This shit so cray cray",
            "If you readed this u stupid",
            "If this u read u dum",
            "Literally dummere end ost",
            "Overvej lige halvdelen... af den her EPISKE MEME",
            "Demens er den ultimative blue pill",
            "Jeg edger ikke aktivt, jeg er smart. Det handler om ikke at gå og anticipate det og keep it cool",
            "Find someone that... looks at you the way Hjalte looks at Simon's coq",
            "\"mads, jeg er en coomer connoisseur\" -Hjalte",
            "\"Jeg hygger mig ikke. Jeg sidder bare og venter på jeg dør\" -Bunu",
            "\"Løsningen er at sætte ild til hele Afrika\" -Jakob",
            "\"Jeg går ind for dårlig videnskab Hjalte, du fik mig.\" - Simon",
            "\"Mads i mikroovnen er good shit\" -Hjalte klokken 2 om natten",
            "\"Den er lidt tiltet... ligesom anders\" -Nico",
            "Did you know? Snask undertale 🥶🥶🥶",
            "UNU BUNU!!!!11!11! HVOR ER DU HENNE????",
            "Er der nogen der læser dette?",
            "En dubloon til den første der liker denne meme",
            "Discombobulate",
            "Bengegigt ququmbabakt",
            "lille pumpe",
            "TEXTURLØS ROTTE??????",
            "Nicolai approves of this image",
            "Hjalte disapproves of this image",
            "Bunu coomer over dette billede",
            "Toni... don't hurry up with that 😅",
            "Skaftet",
        ];
}

