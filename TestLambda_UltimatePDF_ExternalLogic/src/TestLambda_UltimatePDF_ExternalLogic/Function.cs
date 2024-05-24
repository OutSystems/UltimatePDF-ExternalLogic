using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Model.Internal.MarshallTransformations;
using ExternalLogic = OutSystems.UltimatePDF_ExternalLogic;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TestLambda_UltimatePDF_ExternalLogic;

public class Function {

    /// <summary>
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public string FunctionHandler(FunctionInput input, ILambdaContext context) {
        var url = input.Url;
        var bucket = input.Bucket;

        var ultimatePdfLib = new ExternalLogic.UltimatePDF_ExternalLogic();
        var viewport = new ExternalLogic.Structures.Viewport { Width = 1366, Height = 768 };
        var environment = new ExternalLogic.Structures.Environment {
            BaseURL = "professionalservices-dev.outsystems.app",
            Locale = "en",
            Timezone = "Europe/Lisbon"
        };
        var cookies = Array.Empty<ExternalLogic.Structures.Cookie>();
        var paper = new ExternalLogic.Structures.Paper {
            UseCustomPaper = false,
            Width = 21,
            Height = 29.7M,
            UseCustomMargins = false,
            MarginTop = 2.54M,
            MarginRight = 2.54M,
            MarginBottom = 2.54M,
            MarginLeft = 2.54M
        };

        var pdf = ultimatePdfLib.PrintPDF(
            url,
            viewport,
            environment,
            cookies,
            paper,
            timeoutSeconds: 120,
            collectLogs: true,
            attachFilesLogs: false,
            out byte[] logsZipFile);

        var timestamp = DateTime.Now.ToString("yyyyMMddTHHmmssfff");
        Task[] tasks = {
            Task.Run(() => PutS3Object(bucket, $"pdf_{timestamp}.pdf", pdf)),
            Task.Run(() => PutS3Object(bucket, $"logs_{timestamp}.zip", logsZipFile))
        };
        Task.WaitAll(tasks);

        return $"Got a pdf with size {((pdf.Length / 1024.0f) / 1024.0f)} Mb and the logs as {((logsZipFile.Length / 1024.0f) / 1024.0f)} Mb";
    }

    public static async Task<bool> PutS3Object(string bucket, string key, byte[] content) {
        try {
            using var client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest1);
            var request = new PutObjectRequest {
                BucketName = bucket,
                Key = key,
                InputStream = new MemoryStream(content)
            };
            var response = await client.PutObjectAsync(request);
            return true;
        } catch (Exception ex) {
            Console.WriteLine("Exception in PutS3Object:" + ex.Message);
            return false;
        }
    }
}

public class FunctionInput {
    public string Url { get; init; } = String.Empty;
    public string Bucket { get; init; } = String.Empty;
}