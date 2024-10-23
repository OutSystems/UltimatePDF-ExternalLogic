using Amazon.Lambda.Core;
using UltimatePDFLambdaFunctions.Inputs;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace UltimatePDFLambdaFunctions
{
    /// <summary>
    /// A class that will hold our AWS Function methods.
    /// </summary>
    public class Functions
    {
        /// <summary>
        /// This function returns a base64 encoded string representation of the generated PDF 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns>Base64 encoded string representation of the generated PDF</returns>
        public string PrintPDFFunctionHandler(PrintPDFInput input, ILambdaContext context)
        {
            var uri = new Uri(input.Url);
            var ultimatePdfLib = new OutSystems.UltimatePDF_ExternalLogic.UltimatePDF_ExternalLogic();
            var environment = new OutSystems.UltimatePDF_ExternalLogic.Structures.Environment
            {
                BaseURL = uri.Host,
                Locale = input.Environment.Locale,
                Timezone = input.Environment.Timezone
            };

            var pdf = ultimatePdfLib.PrintPDF(
                input.Url,
                input.Viewport,
                environment,
                input.Cookies,
                input.Paper,
                input.TimeoutSeconds,
                input.CollectLogs,
                input.AttachFilesLogs,
                out byte[] logsZipFile);

            return Convert.ToBase64String(pdf);
        }

        public void PrintPDFToRestFunctionHandler(PrintPDFToRestInput input, ILambdaContext context)
        {
            var uri = new Uri(input.Url);
            var ultimatePdfLib = new OutSystems.UltimatePDF_ExternalLogic.UltimatePDF_ExternalLogic();
            var environment = new OutSystems.UltimatePDF_ExternalLogic.Structures.Environment
            {
                BaseURL = uri.Host,
                Locale = input.Environment.Locale,
                Timezone = input.Environment.Timezone
            };

            ultimatePdfLib.PrintPDF_ToRest(
                input.Url,
                input.Viewport,
                environment,
                input.Cookies,
                input.Paper,
                input.TimeoutSeconds,
                input.CollectLogs,
                input.AttachFilesLogs,
                input.RestCaller);
        }

        public void PrintPDFToS3FunctionHandler(PrintPDFToS3Input input, ILambdaContext context)
        {
            var uri = new Uri(input.Url);
            var ultimatePdfLib = new OutSystems.UltimatePDF_ExternalLogic.UltimatePDF_ExternalLogic();
            var environment = new OutSystems.UltimatePDF_ExternalLogic.Structures.Environment
            {
                BaseURL = uri.Host,
                Locale = input.Environment.Locale,
                Timezone = input.Environment.Timezone
            };

            ultimatePdfLib.PrintPDF_ToS3(
                input.Url,
                input.Viewport,
                environment,
                input.Cookies,
                input.Paper,
                input.TimeoutSeconds,
                input.CollectLogs,
                input.AttachFilesLogs,
                input.S3Endpoints);
        }

        public string ScreenshotPNGFunctionHandler(ScreenshotPNGInput input, ILambdaContext context)
        {
            var uri = new Uri(input.Url);
            var ultimatePdfLib = new OutSystems.UltimatePDF_ExternalLogic.UltimatePDF_ExternalLogic();
            var environment = new OutSystems.UltimatePDF_ExternalLogic.Structures.Environment
            {
                BaseURL = uri.Host,
                Locale = input.Environment.Locale,
                Timezone = input.Environment.Timezone
            };

            var screenshot = ultimatePdfLib.ScreenshotPNG(
                input.Url,
                input.Viewport,
                environment,
                input.Cookies,
                input.Paper,
                input.ScreenshotOptions,
                input.TimeoutSeconds,
                input.CollectLogs,
                input.AttachFilesLogs,
                out byte[] logsZipFile);

            return Convert.ToBase64String(screenshot);
        }
    }
}
