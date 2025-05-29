using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MemeApi.library.Services;

public class DiscordWebhookSender
{
    private readonly MemeApiSettings _settings;

    public DiscordWebhookSender(MemeApiSettings settings)
    {
        _settings = settings;
    }
    
    public async Task<bool> SendMessageWithImage(byte[] imageContent, string fileName, string message, string username, string avatarUrl, string webhookUrl, CancellationToken stoppingToken = default)
    {
        using HttpClient httpClient = new();
        MultipartFormDataContent form = [];
        
        var json_payload = CreateJsonPayload(message, username, avatarUrl);

        form.Add(new ByteArrayContent(imageContent, 0, imageContent.Length), "image/png", fileName);
        form.Add(json_payload, "payload_json");
        
        
        try
        {
            var response = await httpClient.PostAsync(webhookUrl, form, stoppingToken);
            if (response == null || !response.IsSuccessStatusCode)
            {
                Console.WriteLine("Response was null");
                return false;
            }
            
            Console.WriteLine(await LogHttpResponse(response));
            return true;
        }
        catch
        {
            return false;
        }
    }


    public async Task<string> LogHttpResponse(HttpResponseMessage response)
    {
        var log = new StringBuilder();

        log.AppendLine($"Status Code: {response.StatusCode}");

        log.AppendLine("Headers:");
        foreach (var header in response.Headers)
        {
            log.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }

        if (response.Content != null)
        {
            var content = await response.Content.ReadAsStringAsync();
            log.AppendLine("Content:");
            log.AppendLine(content);
        }

        return log.ToString();
    }

    private StringContent CreateJsonPayload(string message, string username, string avatarUrl)
    {
        return new StringContent(
            "{" +
            "\"content\":\"" + message + "\"," +
            "\"username\":\"" + username + "\"," +
            "\"avatar_url\":\"" + avatarUrl + "\"" +
            "}",
            Encoding.UTF8, "application/json");
    }
}