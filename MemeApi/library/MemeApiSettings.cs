using System;
using System.Collections.Generic;
using System.Linq;
using MemeApi.Models.DTO.Lotteries;
using Microsoft.Extensions.Configuration;

namespace MemeApi.library;

public class MemeApiSettings
{
    private readonly IConfiguration _config;

    public MemeApiSettings(IConfiguration config)
    {
        _config = config;
    }

    public string GetBaseUploadFolder()
    {
        return TryGetConfig("BaseUploadFolder");
    }

    public string GetEmailHost()
    {
        return TryGetConfig("Email_Host");
    }

    public string GetEmailHostPort()
    {
        return TryGetConfig("Email_Host_Port");
    }
    public string GetNoReplyEmail()
    {
        return TryGetConfig("Email_NoReply_Mail");
    }
    public string GetNoReplyEmailPassword()
    {
        return TryGetConfig("Email_NoReply_Password");
    }

    public string GetMemeOfTheDayEmail()
    {
        return TryGetConfig("Email_MemeOfTheDay_Mail");
    }
    public string GetMemeOfTheDayEmailPassword()
    {
        return TryGetConfig("Email_MemeOfTheDay_Password");
    }

    public string GetMemeOfTheDayWebhook()
    {
        return TryGetConfig("MemeOfTheDay_WebHookURL");
    }
    
    public string GetPlaceBumpWebhook()
    {
        return TryGetConfig("PlaceBump_WebHookURL");
    }

    public string GetDefaultTopicName()
    {
        return TryGetConfig("Topic_Default_Topicname");
    }

    public string GetMemeOfTheDayTopicName()
    {
        return TryGetConfig("Topic_MemeOfTheDay_Topicname");
    }

    public string GetApiUsername()
    {
        return TryGetConfig("Api_Username");
    }
    public string GetApiPassword()
    {
        return TryGetConfig("Api_Password");
    }

    public string GetPlaceOwnerId()
    {
        return TryGetConfig("Place_Owner_Id");
    }
    
    public string GetAdminUsername()
    {
        return TryGetConfig("Admin_Username");
    }
    public string GetAdminPassword()
    {
        return TryGetConfig("Admin_Password");
    }

    public string GetMediaHost()
    {
        return TryGetConfig("Media_Host");
    }

    public string GetBlobStorageServiceUrl()
    {
        return TryGetConfig("BlobStorage_ServiceUrl");
    }

    public string GetBlobStorageBucketName()
    {
        return TryGetConfig("BlobStorage_BucketName");
    }

    public string GetBlobStorageAccessKey()
    {
        return TryGetConfig("BlobStorage_Access_Key");
    }
    public string GetBlobStorageAccessKeySecret()
    {
        return TryGetConfig("BlobStorage_Access_Key_Secret");
    }

    public string GetBotSecret()
    {
        return TryGetConfig("Bot_Secret");
    }

    public record LotteryItemEasterEgg
    {
        public LotteryItemPreviewDTO item { get; init; }
        public string lotteryId { get; init; }
        public string ItemName { get; init; }
    }
    
    public List<LotteryItemEasterEgg> GetAllEasterEggs()
    {
        var list = _config.GetSection("Easter_Eggs").Get<List<LotteryItemEasterEgg>>();
        if (list == null)
        {
            throw new ArgumentNullException("Setting for Easter_Eggs is missing");
        }

        return list;
    }
    
    public List<LotteryItemPreviewDTO> GetEasterEggs(string lotteryID)
    {
        var list = _config.GetSection("Easter_Eggs").Get<List<LotteryItemEasterEgg>>();
        if (list == null)
        {
            throw new ArgumentNullException("Setting for Easter_Eggs is missing");
        }

        var filteredList = list.Where(i => i.lotteryId == lotteryID).Select(i => i.item).ToList();

        return filteredList;
    }

    private string TryGetConfig(string key)
    {
        var value = _config[key];
        if (value is null)
        {
            throw new ArgumentNullException("Setting for " + key + " is missing");
        }

        return value;
    }
}
