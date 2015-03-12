[<AutoOpen>]
module Modules

module Home = 
  let get() = View(Views.``Home.cshtml``, None)
  let post nameModel = View(Views.``Welcome.cshtml``, Some nameModel)

module About = 
  let get (name : string option) =
    View(Views.``About.cshtml``, name)

module StaticFile = 
  let get parameters = 
    match parameters?file with
    | Some file -> File file
    | None -> NotFound

module Redirect = 
  let get redirect = 
    match redirect with
    | Some true -> PermanentRedirect "/about"
    | _ -> View(Views.``Home.cshtml``, None)
