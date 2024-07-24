Wrap code to execute the External Logic code in a AWS Lambda

Step to execute the code:
1. Run .\generate_upload_package.ps1
1. Create a AWS Lambda .net 6.0 runtime, 1Gb Memory
1. Upload the zip TestLambda_UltimatePDF_ExternalLogic.zip as the lambda function code

To test the lambda run with the inputs

```json
{
  "Url": "https://pdf-url",
  "Bucket": "aws s3 bucket name"
}
```

Common errors:
* AWS Lambda handler should be: Runtime - .Net 6; Handler - TestLambda_UltimatePDF_ExternalLogic::TestLambda_UltimatePDF_ExternalLogic.Function::FunctionHandler; Architecture - x86_64 
* Check you have permissions to store files in the S3 bucket (https://repost.aws/knowledge-center/lambda-execution-role-s3-bucket)