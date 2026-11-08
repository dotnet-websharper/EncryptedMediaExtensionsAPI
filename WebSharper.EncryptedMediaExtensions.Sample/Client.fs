namespace WebSharper.EncryptedMediaExtensions.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.EncryptedMediaExtensions

[<JavaScript>]
module Client =
    // The template is loaded from the DOM, so you can edit index.html and refresh
    // the browser without recompiling unless you add or remove holes.
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
