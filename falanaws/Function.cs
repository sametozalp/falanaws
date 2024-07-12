using Amazon;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Newtonsoft.Json;
using Telegram.Bot;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace falanaws;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(ILambdaContext context)
    {
        string token = "";
        var chatID = "812672293";

        var bot = new TelegramBotClient(token);
        string jsonPath = "credentials.json";

        string bucketName = "";
        string keyName = "credentials.json";
        string region = "us-west-2"; // S3 bucket'�n bulundu�u b�lge


        var s3Client = new AmazonS3Client("", "", Amazon.RegionEndpoint.USEast1);

        // Dosyay� S3'ten indirin ve i�eri�ini okuyun
        string credentialsJson = await ReadFileFromS3Async(s3Client, bucketName, keyName);

        string tempFilePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFilePath, credentialsJson);

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", tempFilePath);

        var db = FirestoreDb.Create("");

        CollectionReference collection = db.Collection("falList");
        List<bool> reviewStatusList = new List<bool> { false };
        Query query = collection.WhereIn("reviewStatus", reviewStatusList);
        QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

        var count = querySnapshot.Count;

        if (count > 0)
        {
            string message = count + " adet bekleyen fal mevcut..";
            Console.WriteLine(message);
            await bot.SendTextMessageAsync(chatID, message);
        }
    }

    private static async Task<string> ReadFileFromS3Async(IAmazonS3 s3Client, string bucketName, string keyName)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucketName,
            Key = keyName
        };

        using (var response = await s3Client.GetObjectAsync(request))
        using (var responseStream = response.ResponseStream)
        using (var reader = new StreamReader(responseStream))
        {
            return await reader.ReadToEndAsync();
        }
    }
}
