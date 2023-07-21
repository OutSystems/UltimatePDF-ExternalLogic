<a name="readme-top"></a>

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/OutSystems/ODC-VG-UltimatePDF-Service">
    <img src="images/PrintLayout.png" alt="Logo" width="80" height="80" />
  </a>

  <h3 align="center">Ultimate PDF</h3>

  <p align="center">
    Generate PDF reports by using modern web technologies.
    <br />
    <a href="https://github.com/OutSystems/ODC-VG-UltimatePDF-Service"><strong>Explore files »</strong></a>
</div>

<!-- TABLE OF CONTENTS -->
<details>
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
      </ul>
    </li>
    <li><a href="#license">License</a></li>
    <li><a href="#known-limitations">Known Limitations</a></li>
  </ol>
</details>

## About The Project

Project that enables ODC customers to generate PDFs using modern web technologies.

This component uses the same rendering engine as Chromium (an open-source version of Google Chrome) to transform web pages into PDF documents.

This component is based on the O11 version of <a href="https://www.outsystems.com/forge/component-overview/5641/ultimate-pdf">Ultimate PDF</a>.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Getting Started

This repo contains all the external code (C#) and the modules that can be used to accelerate the development of PDFs at ODC Applications.

### Prerequisites

To generate the External Logic package run 
```sh
.\generate_upload_package.ps1
```

### Installation

The code will generate the file `UltimatePDF_ExternalLogic.zip` that can be uploaded to the ODC Portal as external logic (<a href="https://success.outsystems.com/documentation/outsystems_developer_cloud/building_apps/extend_your_apps_with_external_logic_using_custom_code/">documentation</a>).

Use ODC Studio to publish the modules
* Ultimate PDF.oml - Library with accelerators to use the code from the External Logic actions
* Template_UltimatePDF.oml - Application template that is ready to have 

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Usage

The simplest way to generate a PDF is by:

1. Create an empty screen
1. Add to the screen the web block `PrintLayout` (from UltimatePDF)
1. Build the report
1. Call the server action `PrintToPDF` to generate the PDF (from UltimatePDF)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

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
  * TimeoutSeconds - Timeout in seconds the bowser will wait to render and generate the PDF
  * RestCaller - REST API information for the external logic to store the PDF and the LogsZipFile.
  * PDF _(Output parameter)_ - The PDF file binary data. Empty if RestCaller is passed.
  * LogsZipFile _(Output parameter)_ - The logs of the external logic execution. Empty if RestCaller is passed.
1. Call Download with the output parameter *PDF* of the Server Action `PrintToPDF_Advanced`

![on-initialize] ![on-click]

### Screen to PDF

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

![on-initialize-screen]![on-ready-screen]![on-click-screen]

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
1. Call the Server Action `PrintToPDF_Advanced`, fill the RestCaller parameter
  * Token - The token of the printable page, we use it for REST API authentication.
  * BaseUrl - base tenant url, eg: _https://tenant.outsystems.com_
  * Module - Name of the module that implements the REST API, can use `GetOwnerURLPath()`
  * StorePath - Rest method URL Path to store the PDF, eg: `/rest/pdf/Store`
  * LogPath - Rest method URL Path to store the logs, eg: `/rest/pdf/StoreLogs`
  * The PDF will be stored at the entity `GeneratedPDF_Files`
  * The Logs will be stored at the entity `GeneratedPDF_Logs`, if requested

## License

BSD-3 license. See `LICENSE` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Known Limitations

* The screens to print cannot be protected by authentication. We recommend the screens to be protected by tokens. See the usage of `GeneratePDFToken` on this documentation for examples.
* The input and output payload of the external logic cannot be greater than 5.5MB. <a href="#external-logic-call-rest-api-to-store-the-pdf">Workaround use the REST API Store functionality</a>.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- MARKDOWN LINKS & IMAGES -->
[on-initialize]: images/OnInitialize.png
[on-click]: images/OnClick.png
[on-initialize-screen]: images/OnInitializeScreen.png
[on-ready-screen]: images/OnReadyScreen.png
[on-click-screen]: images/OnClickScreen.png
