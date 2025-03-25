# WebSharper Encrypted Media Extensions API Binding

This repository provides an F# [WebSharper](https://websharper.com/) binding for the [Encrypted Media Extensions (EME) API](https://developer.mozilla.org/en-US/docs/Web/API/Encrypted_Media_Extensions_API), enabling WebSharper applications to interact with DRM (Digital Rights Management) protected media content.

## Repository Structure

The repository consists of two main projects:

1. **Binding Project**:

   - Contains the F# WebSharper binding for the Encrypted Media Extensions API.

2. **Sample Project**:
   - Demonstrates how to use the Encrypted Media Extensions API with WebSharper syntax.
   - Includes a GitHub Pages demo: [View Demo](https://dotnet-websharper.github.io/EncryptedMediaExtensionsAPI/)

## Installation

To use this package in your WebSharper project, add the NuGet package:

```bash
   dotnet add package WebSharper.EncryptedMediaExtensions
```

## Building

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed on your machine.

### Steps

1. Clone the repository:

   ```bash
   git clone https://github.com/dotnet-websharper/EncryptedMediaExtensions.git
   cd EncryptedMediaExtensions
   ```

2. Build the Binding Project:

   ```bash
   dotnet build WebSharper.EncryptedMediaExtensions/WebSharper.EncryptedMediaExtensions.fsproj
   ```

3. Build and Run the Sample Project:

   ```bash
   cd WebSharper.EncryptedMediaExtensions.Sample
   dotnet build
   dotnet run
   ```

## Example Usage

Below is an example of how to use the Encrypted Media Extensions API in a WebSharper project:

```fsharp
namespace WebSharper.EncryptedMediaExtensions.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.EncryptedMediaExtensions

[<JavaScript>]
module Client =
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    // A variable to store DRM status messages
    let statusMessage = Var.Create "Waiting for DRM check..."

    // Function to check if DRM (Digital Rights Management) is supported
    let setupDRM () =
        promise {
            let status = JS.Document.GetElementById("status")

            try
                // Define the DRM configuration
                let drmConfig = MediaKeySystemConfiguration(
                    InitDataTypes = [|"cenc"|], // Specifies the content encryption type
                    VideoCapabilities = [|SupportCapabilities(ContentType = "video/mp4")|] // Defines video playback capabilities
                )

                // Request DRM support access from the browser
                let! access = JS.Window.Navigator.RequestMediaKeySystemAccess("com.widevine.alpha", [|drmConfig|])

                // If successful, update the UI status
                statusMessage.Value <- $"✅ DRM supported: {access.KeySystem}"
            with ex ->
                // If an error occurs, indicate that DRM is not supported
                statusMessage.Value <- $"❌ DRM not supported: {ex.Message}"
        }

    [<SPAEntryPoint>]
    let Main () =
        IndexTemplate.Main()
            .Status(statusMessage.V) // Bind the DRM status message to the UI
            .SetupDRM(fun _ ->
                async {
                    do! setupDRM () |> Promise.AsAsync // Call the setupDRM function asynchronously
                }
                |> Async.StartImmediate
            )
            .Doc()
        |> Doc.RunById "main"
```

This example demonstrates how to check if DRM-protected media playback is supported in the browser using the Encrypted Media Extensions API.

## Important Considerations

- **DRM Provider Support**: Different browsers may use different DRM providers (e.g., Widevine for Chrome, PlayReady for Edge, FairPlay for Safari).
- **User Permissions**: DRM access may require user consent or browser settings adjustments.
- **Error Handling**: Ensure proper handling of unsupported DRM cases to provide fallback options.
