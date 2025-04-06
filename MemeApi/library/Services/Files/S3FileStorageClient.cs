using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace MemeApi.library.Services.Files;

public class S3FileStorageClient : IFileSaver, IFileRemover
{
    private readonly MemeApiSettings _settings;
    private readonly AmazonS3Client _client;

    public S3FileStorageClient(MemeApiSettings settings)
    {
        _settings = settings;

        string accessKey = _settings.GetBlobStorageAccessKey();
        string secretKey = _settings.GetBlobStorageAccessKeySecret();

        AmazonS3Config config = new AmazonS3Config();
        config.ServiceURL = _settings.GetBlobStorageServiceUrl();

        _client = new AmazonS3Client(
            accessKey,
            secretKey,
            config
        );
    }

    public async Task SaveFile(byte[] file, string path, string fileName, string contentType)
    {

        using var memoryStream = new MemoryStream(file);
        memoryStream.Position = 0;

        var putRequest = new PutObjectRequest
        {
            BucketName = _settings.GetBlobStorageBucketName(),
            Key = path + fileName,
            InputStream = memoryStream,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead
        };

        var response = await _client.PutObjectAsync(putRequest);

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            Console.WriteLine("File uploaded successfully");
        }
        else
        {
            Console.WriteLine($"Error uploading file. Status code: {response.HttpStatusCode}");
        }
    }
    public async Task RemoveFile(string path)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _settings.GetBlobStorageBucketName(),
            Key = path
        };

        var response = await _client.DeleteObjectAsync(deleteRequest);

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            Console.WriteLine("File uploaded successfully");
        }
        else
        {
            Console.WriteLine($"Error uploading file. Status code: {response.HttpStatusCode}");
        }
    }
}
