namespace WebSharper.EncryptedMediaExtensions

open WebSharper
open WebSharper.JavaScript
open WebSharper.JavaScript.Dom
open WebSharper.InterfaceGenerator

module Definition =

    let TypedArray = T<Int8Array> + T<Uint8Array> + T<Uint8ClampedArray> + T<Int16Array> + T<Uint16Array>
                        + T<Int32Array> + T<Uint32Array> + T<Float64Array> + T<Float32Array>

    let BufferData = T<ArrayBuffer> + TypedArray + T<DataView>

    let MediaEncryptedEventInit =
        Pattern.Config "MediaEncryptedEventInit" {
            Required = []
            Optional = [
                "initDataType", T<string> 
                "message", T<ArrayBuffer> 
            ]
        }

    let MediaEncryptedEvent =
        Class "MediaEncryptedEvent"
        |=> Inherits T<Event> 
        |+> Static [
            Constructor (T<string>?eventType * !?MediaEncryptedEventInit?options) 
        ]
        |+> Instance [
            "initDataType" =? T<string> 
            "initData" =? T<ArrayBuffer> 
        ]

    let MediaKeyMessageEventInit =
        Pattern.Config "MediaKeyMessageEventInit" {
            Required = []
            Optional = [
                "messageType", T<string> 
                "message", T<ArrayBuffer> 
            ]
        }

    let MediaKeyMessageEvent =
        Class "MediaKeyMessageEvent"
        |=> Inherits T<Event> 
        |+> Static [
            Constructor (T<string>?eventType * !?MediaKeyMessageEventInit?options) 
        ]
        |+> Instance [
            "messageType" =? T<string> 
            "message" =? T<ArrayBuffer> 
        ]

    let MediaKeyStatusMap =

        let forEachCallback = (T<obj>?currentValue * !?T<obj>?index * !?T<obj>?array) ^-> T<unit>

        Class "MediaKeyStatusMap"
        |+> Instance [
            "size" =? T<int> 
            
            "entries" => T<unit> ^-> T<obj> 
            "forEach" => forEachCallback?callbackFn * !?T<obj>?thisArg ^-> T<unit> 
            "get" => T<obj>?key ^-> T<string> 
            "has" => T<obj>?key ^-> T<bool> 
            "keys" => T<unit> ^-> T<obj> 
            "values" => T<unit> ^-> T<obj> 
        ]

    let MediaKeySession =
        Class "MediaKeySession"
        |=> Inherits T<EventTarget> 
        |+> Instance [
            "sessionId" =? T<string> 
            "expiration" =? T<float> 
            "closed" =? T<Promise<_>>[T<unit>] 
            "keyStatuses" =? MediaKeyStatusMap 

            "generateRequest" => T<unit> ^-> T<Promise<_>>[T<unit>] 
            "load" => T<string>?sessionId ^-> T<Promise<_>>[T<bool>] 
            "update" => BufferData?response ^-> T<Promise<_>>[T<unit>] 
            "close" => T<unit> ^-> T<Promise<_>>[T<unit>] 
            "remove" => T<unit> ^-> T<Promise<_>>[T<unit>] 

            "onkeystatuseschange" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnKeyStatusesChange instead"
            "onkeystatuseschange" =@ T<Event> ^-> T<unit>
            |> WithSourceName "OnKeyStatusesChange"

            "onmessage" =@ T<unit> ^-> T<unit>
            |> ObsoleteWithMessage "Use OnMessage instead"
            "onmessage" =@ MediaKeyMessageEvent ^-> T<unit> 
            |> WithSourceName "OnMessage"
        ]

    let MediaKeysPolicy =
        Pattern.Config "MediaKeysPolicy" {
            Required = []
            Optional = [
                "minHdcpVersion", T<string> 
            ]
        }

    let MediaKeys =
        Class "MediaKeys"
        |+> Instance [
            "createSession" => !?T<string>?mediaKeySessionType ^-> T<Promise<_>>[MediaKeySession] 
            "setServerCertificate" => BufferData?serverCertificate ^-> T<Promise<_>>[T<bool>] 
            "getStatusForPolicy" => !?MediaKeysPolicy?policy ^-> T<Promise<_>>[T<string>] 
        ]

    let MediaKeySystemAccess =
        Class "MediaKeySystemAccess"
        |+> Instance [
            "keySystem" =? T<string> 

            "createMediaKeys" => T<unit> ^-> T<Promise<_>>[T<obj>] 
            "getConfiguration" => T<unit> ^-> T<obj> 
        ]

    let SupportCapabilities = 
        Pattern.Config "SupportCapabilities" {
            Required = []
            Optional = [
                "contentType", T<string>
                "encryptionScheme", T<obj>
                "robustness", T<obj>
            ]
        }

    let MediaKeySystemConfiguration =
        Pattern.Config "MediaKeySystemConfiguration" {
            Required = []
            Optional = [
                "label", T<string> 
                "initDataTypes", !|T<string> 
                "audioCapabilities", !|SupportCapabilities 
                "videoCapabilities", !|SupportCapabilities 
                "distinctiveIdentifier", T<string> 
                "persistentState", T<string> 
                "sessionTypes", !|T<string> 
            ]
        }

    let Assembly =
        Assembly [
            Namespace "WebSharper.EncryptedMediaExtensions" [
                MediaEncryptedEvent
                MediaKeyMessageEvent
                MediaKeys
                MediaKeySession
                MediaKeySystemAccess
                MediaKeySystemConfiguration
                SupportCapabilities
                MediaKeysPolicy
                MediaKeyMessageEventInit
                MediaEncryptedEventInit
                MediaKeyStatusMap
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member _.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
