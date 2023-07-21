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
    <li><a href="#getting-started">Getting Started</a></li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#advance-scenarios">Advance Scenarios</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#know-limitations">Known Limitations</a></li>
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
* Ultimate PDF.oml - Library with accelarators to use the code from the External Logic actions
* Template_UltimatePDF.oml - Application template that is ready to have 

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Usage

The simplest way to generate a PDF is by:

1. Create an empty screen
2. Add to the screen the web block PrintLayout (from UltimatePDF)
3. Build the report
4. Call the action PrintToPDF to grnerate the PDF (from UltimatePDF)

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Advance Scenarios

### Prerequisites

In the instructions bellow we will assume that the application that is generating the PDFs was created based on Template_UltimatePDF

### Advance PDF Generation

1. Create a Flow named *Print*, if not present
2. Add an empty screen
3. Under the Authorization properties, select `Everyone`
4. Add an input parameter named `Token` (Data Type = Text, Is Mandatory = Yes)
5. Delete the web block `Layouts\LayoutTopMenu`
6. Fill the screen with the information to have in the PDF
7. Add `On Initialize` event, and add a call to IsPDFTokenValid with the `Token` as parameter
8. Add a if clause `IsPDFTokeValid.Valid`, and end the *False* branch with and exception `PDFTokenExpired`
  [on-initialize]
9. On another screen create a button to generate the PDF
10. Call the Server Action `GeneratePDFToken`
11. Call the Server Action `PrintToPDF_Advanced`
  * URL - url for the page to be generated. In this example, the screen created at _2._
  * Environment - information of the environment where the browser will be launched. Can use the output of the Client Action `CurrEnvironment`
  * PaperSize - Paper size mesures separated by _x_ (eg: "21.00x29.70"). Can use the Static Entity `PaperSize` from `UltimatePDF`
  * MarginSize - Paper margin size separated by _x_ (eg: "2.50x3.00x2.50x3.00"). Can use the Static Entity `MarginSize` from `UltimatePDF`
  * CollectLogs - If the execution of the external logic collects logs. If True, the output parameter LogsZipFile has the logs, it's empty otherwise.
  * Cookies - Cookie values to be used in the browser that will be launched.
  * TimeoutSeconds - Timeout in seconds the bowser will wait to render and generate the PDF
  * RestCaller - REST API information for the external logic to store the PDF and the LogsZipFile.
  * PDF _(Output parameter)_ - The PDF file binary data. Empty if RestCaller is passed.
  * LogsZipFile _(Output parameter)_ - The logs of the external logic execution. Empty if RestCaller is passed.
12. Call the Server Action `ExpireToken`
13. Call Download with the output parameter *PDF* of the Server Action `PrintToPDF_Advanced`
  [on-click]

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
  [on-initialize-screen]
1. Add `On Ready` event, and add a call to `ExpireToken` with the `Token` as parameter
  [on-ready-screen]
1. On another screen create a link with an action on click
1. Call the Server Action `GeneratePDFToken`
1. End the flow with a destination to the screen created at 2.
  [on-click-screen]

## License

BSD-3 license. See `LICENSE` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

## Known Limitations

* The screens to print cannot be protected by authentication. We recomend the screens to be protected by tokens
* The input and output paylod of the external logic cannot be greather than 5.5MB. Workaround use the REST API Store functionality.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- MARKDOWN LINKS & IMAGES -->
[on-initialize]: images/OnInitialize.png
[on-click]: images/OnClick.png
[on-initialize-screen]: images/OnInitializeScreen.png
[on-ready-screen]: images/OnReadyScreen.png
[on-click-screen]: images/OnClickScreen.png