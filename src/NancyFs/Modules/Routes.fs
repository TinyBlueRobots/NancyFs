module Routes

open Nancy.ModelBinding

type Routes() as this = 
  inherit NancyFsModule()
  do 
    (fun _ -> Home.get()) |> this.CreateRoute GET "/"
    (fun _ -> this.Bind<NameModel>() |> Home.post) |> this.CreateRoute POST "/"
    (fun _ -> this.Request.Query?name |> About.get) |> this.CreateRoute GET "/about"
    StaticFile.get |> this.CreateRoute GET "/{file}"
    (fun p -> p?redirect |> Redirect.get) |> this.CreateRoute GET "/redirect/{redirect}"
