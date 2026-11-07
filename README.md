# WebSharper.EncryptedMediaExtensions

[![NuGet](https://img.shields.io/nuget/v/WebSharper.EncryptedMediaExtensions.svg)](https://www.nuget.org/packages/WebSharper.EncryptedMediaExtensions)

WebSharper bindings for the browser [Encrypted Media Extensions (EME) API](https://developer.mozilla.org/en-US/docs/Web/API/Encrypted_Media_Extensions_API) —
the W3C API that lets web applications talk to content decryption modules (CDMs) and play DRM-protected media.

This repository contains:

| Project | Target | Purpose |
|---|---|---|
| `WebSharper.EncryptedMediaExtensions` | `netstandard2.0` | The binding: typed F#/C# wrappers for `MediaKeys`, `MediaKeySession`, `MediaKeySystemAccess`, `MediaKeySystemConfiguration`, `MediaKeyStatusMap`, `MediaEncryptedEvent`, `MediaKeyMessageEvent`, and `MediaKeysPolicy`. |
| `WebSharper.EncryptedMediaExtensions.Extensions` | `netstandard2.0` | Hand-written extensions for the entry points the core binding cannot model: `Navigator.requestMediaKeySystemAccess`, `HTMLMediaElement.mediaKeys`, `setMediaKeys`, and `onencrypted`. |
| `WebSharper.EncryptedMediaExtensions.Sample` | `net10.0` | A runnable WebSharper 10 ASP.NET Core sample that detects EME support and exercises `requestMediaKeySystemAccess`, `createMediaKeys`, and `getConfiguration`. |

Both binding assemblies ship in the single `WebSharper.EncryptedMediaExtensions` NuGet package.

## Live demos

- **Binding sample (this repo):** https://dotnet-websharper.github.io/EncryptedMediaExtensionsAPI/
- **Source repository:** https://github.com/dotnet-websharper/EncryptedMediaExtensionsAPI
- **WebSharper bindings:** https://websharper.com/bindings
- **WebSharper:** https://websharper.com

> EME is only available in a **secure context** (`https://` or `localhost`). DRM providers differ by browser (Widevine in Chrome/Edge, PlayReady in Edge, FairPlay in Safari), and actual playback needs encrypted media plus a license server. The sample therefore only performs support detection and degrades gracefully when no key system or CDM is available.

## Install

```bash
dotnet add package WebSharper.EncryptedMediaExtensions
```

Or with Paket:

```fsharp
nuget WebSharper.EncryptedMediaExtensions
```

The binding targets `netstandard2.0` and works with any WebSharper 10 project.

## Usage

All types live in the `WebSharper.EncryptedMediaExtensions` namespace. The sample's `Client.fs` checks for Widevine support and exercises the main EME entry points:

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
    type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

    let status = Var.Create "Not checked yet."
    let secureContext = Var.Create "unknown"
    let emeApi = Var.Create "unknown"
    let keySystem = Var.Create "—"

    let isSecureContext () =
        let location = JS.Window.Location
        location.Href.StartsWith("https:")
        || location.Hostname = "localhost"
        || location.Hostname = "127.0.0.1"

    let checkDrm () =
        status.Value <- "Checking DRM support…"
        secureContext.Value <- if isSecureContext () then "yes" else "no"

        promise {
            try
                let config =
                    MediaKeySystemConfiguration(
                        InitDataTypes = [| "cenc" |],
                        VideoCapabilities = [| SupportCapabilities(ContentType = "video/mp4") |]
                    )

                let! access =
                    JS.Window.Navigator.RequestMediaKeySystemAccess("com.widevine.alpha", [| config |])

                keySystem.Value <- access.KeySystem
                emeApi.Value <- "yes"

                // Exercise the rest of the EME surface: create the MediaKeys object
                // and read the negotiated configuration. Actual playback would then
                // require encrypted media and a license server.
                let! mediaKeys = access.CreateMediaKeys()
                mediaKeys |> ignore
                access.GetConfiguration() |> ignore

                status.Value <- "✅ Widevine is available and MediaKeys were created."
            with ex ->
                let message = ex.Message
                if emeApi.Value = "yes" then
                    status.Value <- "❌ Widevine is available, but MediaKeys could not be created: " + message
                elif message.Contains("is not a function") then
                    emeApi.Value <- "no"
                    keySystem.Value <- "—"
                    status.Value <- "⚠️ The Encrypted Media Extensions API is not available in this browser."
                else
                    emeApi.Value <- "yes"
                    keySystem.Value <- "com.widevine.alpha (unavailable)"
                    status.Value <- "❌ Widevine is not available: " + message
        }

    [<SPAEntryPoint>]
    let Main () =
        IndexTemplate.Main()
            .Status(status.V)
            .SecureContext(secureContext.V)
            .EmeApi(emeApi.V)
            .KeySystem(keySystem.V)
            .Check(fun _ -> checkDrm () |> ignore)
            .Doc()
        |> Doc.RunById "main"

        checkDrm () |> ignore
```

### Notes on the API

- The Extensions assembly adds `Navigator.RequestMediaKeySystemAccess`, `HTMLMediaElement.MediaKeys`, `HTMLMediaElement.SetMediaKeys`, and `HTMLMediaElement.OnEncrypted`.
- `MediaKeySession.GenerateRequest(initDataType, initData)` now takes the init-data type and the init data. The legacy no-argument overload is kept for source compatibility and is marked obsolete.
- `MediaKeySystemAccess.CreateMediaKeys()` returns `Promise<MediaKeys>` and `GetConfiguration()` returns `MediaKeySystemConfiguration`.
- The promise-returning members (`MediaKeySession.Closed`, `.Load`, `.Update`, `.Close`, `.Remove`) are bound. `Remove` is retained for compatibility.
- The sample shows how to detect support and how to degrade gracefully; a complete license flow needs encrypted content and a license server.

## Build from source

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) 10.0 or later. The binding and Extensions projects target `netstandard2.0`; the sample targets `net10.0`.

### Build

```bash
dotnet build WebSharper.EncryptedMediaExtensions.sln -c Release
```

No Paket restore is needed; the repository uses plain NuGet.

### Run the sample

```bash
cd WebSharper.EncryptedMediaExtensions.Sample
dotnet run -c Release
```

Then open the URL printed in the console (see `Properties/launchSettings.json`;
by default `https://localhost:51591`, with `http://localhost:51592` also configured).
The sample runs the support check on load; the **Check DRM support** button runs it again.
In headless Chrome, or when no CDM is installed, the sample reports that Widevine is unavailable and continues to render the page.

### Deployment

Pushing to `main` triggers `.github/workflows/ghpages.yml`, which builds the sample in Release on `ubuntu-latest` with the .NET 10 SDK and publishes `WebSharper.EncryptedMediaExtensions.Sample/wwwroot` to the `gh-pages` branch via GitHub Pages. GitHub Pages must be enabled for the repository.

## Repository layout

```
WebSharper.EncryptedMediaExtensions/            # the binding (WIG definition)
  Main.fs                                       # types + [<assembly: Extension>] registration
WebSharper.EncryptedMediaExtensions.Extensions/ # hand-written extensions
  Library.fs                                    # Navigator / HTMLMediaElement extensions
WebSharper.EncryptedMediaExtensions.Sample/     # WebSharper 10 ASP.NET Core sample (net10.0)
  Client.fs                                     # Vars + template holes + EME support check
  Startup.fs                                    # ASP.NET Core host
  wwwroot/index.html                            # template (ws-replace / ws-onclick)
  wsconfig.json                                 # "spa" project, outputDir wwwroot/Content
.github/workflows/ghpages.yml                   # build + deploy sample to gh-pages
```

## License

Apache 2.0 — see [LICENSE.md](LICENSE.md).
