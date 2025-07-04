<a name="readme-top"></a>

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/OutSystems/ODC-VG-UltimatePDF-Service">
    <img src="images/PrintLayout.png" alt="Logo" width="80" height="80" />
  </a>

  <h3 align="center"><b>Ultimate PDF</b></h3>
  
  <p align="center">
    Generate PDF reports by using modern web technologies.
  </p>
</div>

<!-- TABLE OF CONTENTS -->
<details open>
  <summary>Table of Contents</summary>
  <ol>
    <li><a href="#about-the-project">About The Project</a></li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li>
      <a href="#advance-scenarios">Advance Scenarios</a>
      <ul>
        <li><a href="#prerequisites-1">Prerequisites</a></li>
        <li><a href="#advance-pdf-generation">Advance PDF Generation</a></li>
        <li><a href="#screen-to-pdf">Screen to PDF</a></li>
        <li><a href="#external-logic-call-rest-api-to-store-the-pdf">External Logic call Rest API to store the PDF</a></li>
        <li><a href="#external-logic-call-s3-bucket-to-store-the-pdf">External Logic call S3 Bucket to store the PDF</a></li>
        <li><a href="#basic-page-screenshot">Basic page screenshot</a></li>
        <li><a href="#add-fonts-to-the-report">Add Fonts to the Report</a></li>
      </ul>
    </li>
    <li><a href="#license">License</a></li>
    <li><a href="#known-limitations">Known Limitations</a></li>
    <li><a href="#get-in-touch">Get in touch</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#testing">Testing</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>

## About The Project

Project that enables ODC customers to generate PDFs using modern web technologies.

This component uses the same rendering engine as Chromium (an open-source version of Google Chrome) to transform web pages into PDF documents.

This component is based on the O11 version of <a href="https://www.outsystems.com/forge/component-overview/5641/ultimate-pdf">Ultimate PDF</a>.

<b>THIS CODE IS NOT SUPPORTED BY OUTSYSTEMS.</b>

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Getting Started

This repo contains all the external code (C#) and the modules that can be used to accelerate the development of PDFs at ODC Applications.

### Prerequisites

To generate the External Logic package run 
```sh
.\generate_upload_package.ps1
```

### Installation

This component is published at forge. So the best way to install it on your ODC tenant is by searching for:
* UltimatePDF_ExternalLogic
* Ultimate PDF
* _(Optional)_ Template_UltimatePDF

#### From the repo

The code will generate the file `UltimatePDF_ExternalLogic.zip` that can be uploaded to the ODC Portal as external logic (<a href="https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/extend_your_apps_with_external_logic_using_custom_code/">documentation</a>).

Use ODC Studio to publish the modules
* Ultimate PDF.oml - Library with accelerators to use the code from the External Logic actions
* Template_UltimatePDF.oml - Application template. Use this template to create an application that is ready to use the Ultimate PDF component

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Usage

The simplest way to generate a PDF is by:

1. Create an empty screen
1. Add to the screen the web block `PrintLayout` (from UltimatePDF)
1. Build the report
1. Call the server action `PrintToPDF` to generate the PDF (from UltimatePDF)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Public Elements

All the listed public elements are present in the library **Ultimate PDF**.

### Block

- **PageCount**: Displays the total number of pages. Use inside the header of footer of PrintLayout.  See also: PrintLayout.
- **PageNumber**: Displays the current page number. Use inside the header of footer of PrintLayout.  See also: PrintLayout.
- **PageBreak**: Force a page break.
- **SectionBreak**: Breaks a section between two PrintLayout blocks, and allows to control how pagination continues on the new section.
- **PrintLayout**: Creates a page layout, including header and footer placeholders that are repeated on every page, and a page background that can be used for watermarks and other full-bleed design elements.  See also: ScreenToPDF.
- **ScreenToPDF**: Automatically converts a screen into PDF, such that any navigation to the screen automatically downloads it as a PDF.
- **HideOnPrint**: Content will not be shown on print media.
- **ShowOnPrint**: Content will only be shown on print media.

### Client actions

- **OnApplicationReady_UltimatePDF**: Loads support for Ultimate PDF on reactive applications.  This action must be invoked during the OnApplicationReady event. Some features of Ultimate PDF may not work otherwise.
- **ScreenToPDF_OnInitialize**: Initializes a screen that is using ScreenToPDF block.  This action must be invoked during the OnInitialize event of the screen. The ScreenToPDF block will not work otherwise.
- **CurrEnvironment**: Current Environment information.
- **GetDefaultViewport**: Defines a default viewport size of 1366x768

### Server actions

- **PrintToPDF**: Generates a PDF from a given URL, using the paper size and margin size from the print stylesheet.
- **PrintToPDF_Advanced**: Generates a PDF from a given URL, specifying paper size and margin size.
- **ScreenshotToPNG**: Generates a screenshot (PNG) from a given URL, using the paper size and margin size from the print stylesheet.
- **ScreenshotToPNG_Advanced**: Generates a screenshot (PNG) from a given URL, specifying paper size and margin size.
- **GetDefaultViewport**: Defines a default viewport size of 1366x768

### Static Entities

- **MarginSize**: Common document margin sizes.
- **PaperSize**: Common paper sizes.

## Advance Scenarios

### Prerequisites

In the instructions bellow we will assume that the application that is generating the PDFs was created based on Template_UltimatePDF. This Template creates an application that contains:
* REST API `pdf` - that implements an API to store the PDF on the application
* Entity `GeneratePDF` - contains information for the pdf and the token authentication for the the pages
* Entity `GeneratedPDF_Files` - entity where the REST API saves the PDF files
* Entity `GeneratedPDF_Logs` - entity where the REST API saves the Log files

### Advance PDF Generation

1. Create a Flow named *Print*, if not present
1. Add an empty screen
1. Under the Authorization properties, select `Everyone`
1. Add an input parameter named `Token` (Data Type = Text, Is Mandatory = Yes)
1. Delete the web block `Layouts\LayoutTopMenu`
1. Fill the screen with the information to have in the PDF
1. Add `On Initialize` event, and add a call to IsPDFTokenValid with the `Token` as parameter
1. Add a if clause `IsPDFTokeValid.Valid`, and end the *False* branch with and exception `PDFTokenExpired`
1. Add `On Ready` event, and add a call to `ExpireToken` with the `Token` as parameter
1. On another screen create a button to generate the PDF
1. Call the Server Action `GeneratePDFToken`
1. Call the Server Action `PrintToPDF_Advanced`
  * URL - url for the page to be generated. In this example, the screen was created at _2._
  * Environment - information of the environment where the browser will be launched. Can use the output of the Client Action `CurrEnvironment`
  * PaperSize - Paper size measures separated by _x_ (eg: "21.00x29.70"). Can use the Static Entity `PaperSize` from `UltimatePDF`
  * MarginSize - Paper margin size separated by _x_ (eg: "2.50x3.00x2.50x3.00"). Can use the Static Entity `MarginSize` from `UltimatePDF`
  * CollectLogs - If the execution of the external logic collects logs. If True, the output parameter LogsZipFile has the logs, it's empty otherwise.
  * Cookies - Cookie values to be used in the browser that will be launched.
  * TimeoutSeconds - Timeout in seconds the browser will wait to render and generate the PDF
  * RestCaller - REST API information for the external logic to store the PDF and the LogsZipFile.
  * PDF _(Output parameter)_ - The PDF file binary data. Empty if RestCaller is passed.
  * LogsZipFile _(Output parameter)_ - The logs of the external logic execution. Empty if RestCaller is passed.
1. Call Download with the output parameter *PDF* of the Server Action `PrintToPDF_Advanced`

<img src="images/OnInitialize.png" width="200" height="auto"/><img src="images/OnClick.png" width="200" height="auto"/>

### Screen to PDF

1. At the Logic table add a System Event > On Application Ready
1. Add a call to `OnApplicationReady_UltimatePDF`
1. Create a Flow named *Print*, if not present
1. Add an empty screen
1. Under the Authorization properties, select `Everyone`
1. Add an input parameter named `Token` (Data Type = Text, Is Mandatory = Yes)
1. Delete the web block `Layouts\LayoutTopMenu`
1. Add the web block `PrintLayout\ScreenToPDF`
1. Fill the screen with the information to have in the PDF
1. Add `On Initialize` event, and add a call to `IsPDFTokenValid` with the `Token` as parameter
1. Add a if clause `IsPDFTokeValid.Valid`, and end the *False* branch with and exception `PDFTokenExpired`
1. Add a call to `ScreenToPDF_OnInitialize`
1. Add `On Ready` event, and add a call to `ExpireToken` with the `Token` as parameter
1. On another screen create a link with an action on click
1. Call the Server Action `GeneratePDFToken`
1. End the flow with a destination to the screen created at 2.

<img src="images/OnApplicationReadyScreen.png" width="200" height="auto"/><img src="images/OnInitializeScreen.png" width="200" height="auto"/><img src="images/OnReadyScreen.png" width="200" height="auto"/><img src="images/OnClickScreen.png" width="200" height="auto"/>

### External Logic call Rest API to store the PDF

The Template_UltimatePDF already creates a REST API named *pdf* with two methods *Store* and *StoreLogs*. The external logic expects the REST API to be implemented as POST methods with binary data as the body of the request. The API call uses the `Token` parameter as an authorization header.

1. Create a Flow named *Print*, if not present
1. Add an empty screen
1. Under the Authorization properties, select `Everyone`
1. Add an input parameter named `Token` (Data Type = Text, Is Mandatory = Yes)
1. Delete the web block `Layouts\LayoutTopMenu`
1. Fill the screen with the information to have in the PDF
1. Add `On Initialize` event, and add a call to IsPDFTokenValid with the `Token` as parameter
1. Add a if clause `IsPDFTokeValid.Valid`, and end the *False* branch with and exception `PDFTokenExpired`
1. On another screen create a button to generate the PDF
1. Call the Server Action `GeneratePDFToken`
1. Call the Server Action `PrintToPDF_Advanced_ToRest`, fill the RestCaller parameter
  * Token - The token of the printable page, we use it for REST API authentication.
  * BaseUrl - base tenant url, eg: _https://tenant.outsystems.com_
  * Module - Name of the module that implements the REST API, can use `GetOwnerURLPath()`
  * StorePath - Rest method URL Path to store the PDF, eg: `/rest/pdf/Store`
  * LogPath - Rest method URL Path to store the logs, eg: `/rest/pdf/StoreLogs`
  * The PDF will be stored at the entity `GeneratedPDF_Files`
  * The Logs will be stored at the entity `GeneratedPDF_Logs`, if requested

### External Logic call S3 Bucket to store the PDF

More information on S3 buckets and presigned urls can be found in official <a href="https://docs.aws.amazon.com/AmazonS3/latest/userguide/using-presigned-url.html">AWS documentation</a>.

1. Create a Flow named *Print*, if not present
1. Add an empty screen
1. Under the Authorization properties, select `Everyone`
1. Add an input parameter named `Token` (Data Type = Text, Is Mandatory = Yes)
1. Delete the web block `Layouts\LayoutTopMenu`
1. Fill the screen with the information to have in the PDF
1. Add `On Initialize` event, and add a call to IsPDFTokenValid with the `Token` as parameter
1. Add a if clause `IsPDFTokeValid.Valid`, and end the *False* branch with and exception `PDFTokenExpired`
1. On another screen create a button to generate the PDF
1. Call the Server Action `GeneratePDFToken`
1. Call the Server Action `PrintToPDF_Advanced_ToS3`, fill the S3Endpoints parameter
  * PdfPreSignedUrl - Presigned url to a S3 Bucket to store the resulting PDF
  * LogsPreSignedUrl - Presigned url to a S3 Bucket to store the resulting logs

### Basic page screenshot

1. Create an empty screen
1. Add to the screen the web block `PrintLayout` (from UltimatePDF)
1. Build the report
1. Call the server action `ScreenshotToPNG` to generate the image (from UltimatePDF)

<img src="images/screenshot.png"/>

### Add Fonts to the Report

The library uses <a href="https://developers.google.com/fonts/docs/getting_started">Google Fonts</a> to download the fonts for the PDF generation. Fonts added using these methods need to be available in the Google Fonts.

1. At the report page on the OnInitialize action add `SetDocumentFont`
1. If the font you need is not present on the static entity `Fonts`, call the `AddFontFamilyToDocument` to the add the custom font.

### Add Fonts to the Report as Resource

If you want to use a font that is not available in Google Fonts distribution service you can include the fonts as a resource in the ODC application. You will need the font file. Supported font files are `.ttf`, `.otf`, `.woff` and `.woff2`.

1. Add the font file as a resource ([documentation](https://success.outsystems.com/documentation/11/building_apps/data_management/use_resources/))
1. Add the following CSS code ([documentation](https://developer.mozilla.org/en-US/docs/Web/CSS/@font-face)) to the Application Style or to the Screen Style Sheet ([documentation](https://success.outsystems.com/documentation/11/building_apps/user_interface/look_and_feel/cascading_style_sheets_css))
```css
/* Define font */
@font-face {
  font-family: MyFont;
  font-style: normal;
  font-weight: 400;
  /* This value is the Runtime Path from the resource */
  src: url(/UltimatePDFTests/fonts/MyFont.ttf);
}

/* Define style class to use the font inside */
.myfont-font {
  font-family: MyFont;
}
/* or, apply font to the full body of the screen */
body {
  font-family: MyFont;
}
```
3. Reference the CSS on the PDF report screen

<img src="images/ResourceFont.png"/>

## License

BSD-3 license. See <a href="LICENSE">LICENSE</a> for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Known Limitations

* Ultimate PDF doesn’t support the <a href="https://success.outsystems.com/documentation/outsystems_developer_cloud/managing_outsystems_platform_and_apps/configure_ip_filters/">IP Filtering</a> feature. Enabling this feature in a stage will result in the reports returning a 403 error page. According to OutSystems <a href="https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/extend_your_apps_with_external_logic_using_custom_code/external_libraries_sdk_readme/">documentation</a>, the external libraries run on “an external service” outside the realm of the tenant applications. Because of this, Ultimate PDF fails to call the report page to render the PDF.
* The screens to print cannot be protected by authentication. We recommend the screens to be protected by tokens. See the usage of `GeneratePDFToken` on this documentation for examples.
* The input and output payload of the external logic cannot be greater than 5.5MB. <a href="#external-logic-call-rest-api-to-store-the-pdf">Workaround use the REST API Store functionality</a>.
* The version of chromium bundle with the forge component only has <a href="https://fonts.google.com/specimen/Open+Sans?query=open+sans">Open Sans font</a> installed meaning it only supports a subset of languages. As a <a href="#add-fonts-to-the-report">workaround</a> the customer can add the needed fonts as css (<a href="https://developers.google.com/fonts/docs/getting_started">https://developers.google.com/fonts/docs/getting_started</a>).
* ODC has a time limit for running external logic (custom code). The current configuration is 95 seconds (<a href="https://success.outsystems.com/documentation/outsystems_developer_cloud/getting_started/outsystems_system_requirements_for_odc/#platform-limits">documentation</a>). Any call to this library that takes more than 95 seconds will fail with a timeout. If you are getting a 95 seconds timeout please consider: 1) Reviewing the report (screen) logic to try and improve its performance; 2) using a different library that does not rely on rendering the page on demand to generate the PDF. 
  * To check if you are affected by this check the <a href="https://success.outsystems.com/documentation/outsystems_developer_cloud/monitoring_and_troubleshooting_apps/">Traces</a> to see if you have a message like <i>"OS-BERT-ELR-61302 - Running this action's custom code has exceeded the timeout limit of 95 seconds."</i>.
* Rendering multiple instances of the <a href="https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/user_interface/patterns/interaction/map/">Map widget</a> have been reported and tested as not working correctly when generating a PDF or Screenshot (<a href="https://github.com/OutSystems/UltimatePDF-ExternalLogic/issues/15">Issue</a>).
  * A workaround for this problem is using <a href="https://outsystemsui.outsystems.com/OutSystemsMapsSample/StaticMap">Static map</a>, which we advise as a more efficient component to use in reports.
* The first execution of the UltimatePDF external logic normally is slower than 10 seconds. This results in the request timing out. The main reason for this slowness is the fact that we need to prepare a chromium browser to run the requests, and then the render of the page. In an interval of 10 to 15 minutes this penalty is reduced since the external logic infrastructure is reused. The best workaround for this limitation is to set a Server Request Timeout greater than 10 seconds. 
  * To check if you are affected by this check the <a href="https://success.outsystems.com/documentation/outsystems_developer_cloud/monitoring_and_troubleshooting_apps/">Traces</a> to see if you have a timeout.

<img src="images/ServerRequestTimeout.png" width="200" height="auto"/><img src="images/TimeoutTrace.png" width="200" height="auto"/>

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Get in touch

Help us improve `UltimatePDF-ExternalLogic` by either:
* <a href="https://github.com/OutSystems/UltimatePDF-ExternalLogic/issues">Submitting an issue</a> with detailed information about the problem you're having
* <a href="mailto:vanguard@outsystems.com">Sending us an email</a> with any feedback or questions that you may have

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Contributing

1. Do a repository Fork;
1. Create a branch based in the branch master (latest & greatest release);
1. Open the branch with you favorite C# code editor;
1. Make your code change;
1. Document your code;
1. Install the external logic in your tenant and test;
1. Kept the branch updated with the master branch and also synchronized with the upstream master;
1. Create a PR, describing what was the (mis)behavior, what you changed and please provide a sample;
1. Address any feedback in code review.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Testing

At the oml folder, there is an application named <a href="oml/Ultimate%20PDF%20Tests.oml">Ultimate PDF Tests</a> that contains multiple examples and tests of the component. All the pull requests will be tested against the test application scenarios. Use this application to test your changes before sending the PR.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Acknowledgments

Project base on OutSystems Forge component <a href="https://www.outsystems.com/forge/component-overview/5641/ultimate-pdf">Ultimate PDF</a>.

<p align="right">(<a href="#readme-top">back to top</a>)</p>
