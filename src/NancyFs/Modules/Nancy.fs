[<AutoOpen>]
module NancyFs

open FSharp.Management
open Nancy
open Nancy.Responses
open System

let (?) (p : obj) prop = 
  let ddv = (p :?> DynamicDictionary).[prop] :?> DynamicDictionaryValue
  match ddv.HasValue with
  | false -> None
  | _ -> ddv.TryParse<'a>() |> Some

type Views = RelativePath< ".\\Views", watch=true >

type Response<'a> = 
  | View of string * 'a option
  | File of string
  | NotFound
  | PermanentRedirect of string
  | Json of 'a

type HTTPMethod = 
  | DELETE
  | GET
  | OPTIONS
  | PATCH
  | POST
  | PUT

let (|ViewName|) (viewName : string) = viewName.Replace("\\", "/")

type NancyFsModule() = 
  inherit NancyModule()
  
  member private this.Nancify response = 
    match response with
    | View(ViewName viewName, None) -> this.View.[viewName] |> box
    | View(ViewName viewName, Some model) -> this.View.[viewName, model] |> box
    | File path -> this.Response.AsFile path |> box
    | NotFound -> HttpStatusCode.NotFound |> box
    | PermanentRedirect path -> this.Response.AsRedirect(path, RedirectResponse.RedirectType.Permanent) |> box
    | Json o -> this.Response.AsJson o |> box
  
  member this.CreateRoute httpMethod route f = 
    let f = Func<obj, obj>(fun p -> f p |> this.Nancify)
    match httpMethod with
    | DELETE -> base.Delete.[route] <- f
    | GET -> base.Get.[route] <- f
    | OPTIONS -> base.Options.[route] <- f
    | PATCH -> base.Patch.[route] <- f
    | POST -> base.Post.[route] <- f
    | PUT -> base.Put.[route] <- f
  
  member this.CreateAsyncRoute httpMethod route f = 
    let f = 
      fun p c -> 
        let computation = async { let! response = f p c
                                  return response |> this.Nancify }
        Async.StartAsTask(computation, cancellationToken = c)
    match httpMethod with
    | DELETE -> base.Delete.[route, true] <- f
    | GET -> base.Get.[route, true] <- f
    | OPTIONS -> base.Options.[route, true] <- f
    | PATCH -> base.Patch.[route, true] <- f
    | POST -> base.Post.[route, true] <- f
    | PUT -> base.Put.[route, true] <- f
