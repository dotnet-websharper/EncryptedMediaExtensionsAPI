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

    let statusMessage = Var.Create "Waiting for DRM check..."
    
    let setupDRM () =
        promise {
            let navigator = As<Navigator>(JS.Window.Navigator)
            let status = JS.Document.GetElementById("status")

            try

                let drmConfig = MediaKeySystemConfiguration(
                    InitDataTypes = [|"cenc"|],
                    VideoCapabilities = [|SupportCapabilities(ContentType = "video/mp4")|]
                )

                let! access = navigator.RequestMediaKeySystemAccess("com.widevine.alpha", [|drmConfig|])

                statusMessage.Value <- $"✅ DRM supported: {access.KeySystem}"
            with ex ->
                statusMessage.Value <- $"❌ DRM not supported: {ex.Message}"
        }

    [<SPAEntryPoint>]
    let Main () =
        IndexTemplate.Main()
            .Status(statusMessage.V)
            .SetupDRM(fun _ ->
                async {
                    do! setupDRM () |> Promise.AsAsync
                }
                |> Async.StartImmediate
            )
            .Doc()
        |> Doc.RunById "main"
