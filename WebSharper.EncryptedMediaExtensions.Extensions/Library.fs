namespace WebSharper.EncryptedMediaExtensions

open WebSharper
open WebSharper.JavaScript

[<JavaScript; AutoOpen>]
module Extensions =

    type Navigator with
        [<Inline "$this.requestMediaKeySystemAccess($keySystem, $supportedConfigurations)">]
        member this.RequestMediaKeySystemAccess
            (
                keySystem: string,
                supportedConfigurations: MediaKeySystemConfiguration[]
            ) : Promise<MediaKeySystemAccess> =
                X<Promise<MediaKeySystemAccess>>

    type HTMLMediaElement with

        [<Inline "$this.mediaKeys">]
        member this.MediaKeys with get(): MediaKeys = X<MediaKeys>
        
        [<Inline "$this.setMediaKeys($mediaKeys)">]
        member this.SetMediaKeys(mediaKeys: MediaKeys): Promise<unit> = X<Promise<unit>>

        [<Inline "$this.onencrypted">]
        member this.OnEncrypted with get(): (MediaEncryptedEvent -> unit) = ignore
        [<Inline "$this.onencrypted = $callback">]
        member this.OnEncrypted with set(callback: MediaEncryptedEvent -> unit) = ()
