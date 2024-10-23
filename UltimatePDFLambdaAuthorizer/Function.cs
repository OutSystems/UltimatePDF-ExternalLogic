using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace UltimatePDFLambdaAuthorizer
{
    public class Function
    {
        public APIGatewayCustomAuthorizerResponse FunctionHandler(APIGatewayCustomAuthorizerRequest request, ILambdaContext context)
        {
            var authorized = false;

            request.Headers.TryGetValue("Authorization", out string? authHeader);

            if (!string.IsNullOrWhiteSpace(authHeader)) {
                var encodedCredentials = authHeader.Split(' ')[1];

                byte[] data = Convert.FromBase64String(encodedCredentials);
                var decodedCredentials = System.Text.Encoding.UTF8.GetString(data).Split(":");
                var username = decodedCredentials[0];
                var password = decodedCredentials[1];

                // Sample credentials below for demostration purposes
                // These details should be fetched from somewhere secure and configurable
                if (username == "test" && password == "test")
                {
                    authorized = true;
                }
            }
            
            APIGatewayCustomAuthorizerPolicy policy = new APIGatewayCustomAuthorizerPolicy
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
            };

            policy.Statement.Add(new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
            {
                Action = new HashSet<string>(new string[] { "execute-api:Invoke" }),
                Effect = authorized ? "Allow" : "Deny",
                Resource = new HashSet<string>(new string[] { request.MethodArn })

            });

            APIGatewayCustomAuthorizerContextOutput contextOutput = new APIGatewayCustomAuthorizerContextOutput()
            {
                ["User"] = "User",
                ["Path"] = request.MethodArn
            };

            return new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = "User",
                Context = contextOutput,
                PolicyDocument = policy
            };
        }
    }
}
