This project is for illustration purposes only and provides a sample Custom Authorizer for use with AWS API Gateway endpoints using Basic Auth

Step to execute the code:
1. Run .\generate_upload_package.ps1
1. Create a AWS Lambda .net 8.0 runtime
1. Upload the zip UltimatePDFLambdaAuthorizer.zip as the lambda function code
1. Add the lambda function as a new request based custom authorizer on API gateway


To test the custom authorizer call an API Gateway endpoint that has the authorizer enabled using Basic Auth headers

```text
// This header equates to Basic Auth with username: test | password: test
Authorization: Basic dGVzdDp0ZXM=
```