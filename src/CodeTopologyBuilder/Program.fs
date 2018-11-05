open System
open System.Collections.Generic
open Newtonsoft
type File = 
    { path : string
      name : string
      authors : svnlog.CommitsByAuthor []
      loc : int
      totalCommits: int
      language : string }

type Node = 
    { name : string
      children : ResizeArray<Node>
      data : Option<File> }

type RootModel = 
    { Authors : string []
      Languages: string []
      TotalCommitCount: int
      MaxCommitCount: int
      Data : Node }

let tryGetChild node childName =
    node.children
    |> Seq.filter (fun x -> x.name = childName)
    |> Seq.tryHead

let createLeaf name data = 
    { Node.name = name
      children = null
      data = Some data }

let createNode name = 
    { Node.name = name
      children = List<Node>()
      data = None }

let rec buildTree (root : Node) (subPaths : string list) (data : File) = 
    match subPaths with
    | [] -> ()
    | [ part ] -> 
        let node = createLeaf part data            
        root.children.Add(node)      
    | part :: rest ->
        let node = 
            match tryGetChild root part with
            | None -> 
                let newNode = createNode part                   
                root.children.Add(newNode)
                newNode
            | Some x -> x
        
        buildTree node rest data    

let getAuthors fileName (commits:seq<svnlog.Commit>)=
    let fileCommits = commits |> Seq.filter(fun c -> c.fileName = fileName)|> Seq.toList
    match fileCommits with                                               
    | [el] -> el.commits
    | _ -> [||]
      

let mergeFilesData (files:seq<cloc.FileOfCode>) (commits:seq<svnlog.Commit>) =     
    files |> Seq.map (fun x ->  
                                let commitsByAuthor = getAuthors x.fileName commits                                           
                                {
                                    File.path = x.fileName;
                                    name = IO.Path.GetFileName(x.fileName); 
                                    authors = commitsByAuthor;
                                    loc = x.loc;
                                    totalCommits = commitsByAuthor |> Array.sumBy (fun x -> x.commits);
                                    language = x.language
                                 })      
           |> Seq.toList


let buildFilesTree fileList=
    let root = createNode "."
    fileList |> Seq.iter (fun x -> 
                                 let pathParts = Array.toList (x.path.Split([|'/'|]))
                                 buildTree root pathParts x |> ignore
                                 )
    root

let toJson obj=
    let settings = Json.JsonSerializerSettings()
    settings.NullValueHandling <- Json.NullValueHandling.Ignore
    Newtonsoft.Json.JsonConvert.SerializeObject(obj,Json.Formatting.None, settings)

open FSharp.Data

type UserMapping = JsonProvider<"SampleData\UserMapping.json">

let createUserMappingFunc (mappingFilePath:string)=
    if String.IsNullOrWhiteSpace(mappingFilePath) then 
        id
    else
       let mapping =  UserMapping.Load(mappingFilePath);
       (fun username ->
            let select = mapping.Mapping  |> Array.tryFind (fun el -> el.Alias |> Array.contains username )
            match select with
             | None -> username
             | Some x -> x.Name
            )
    


[<EntryPoint>]
let main argv =    
    let checkoutDir = argv.[0]
    let repoPrefix = argv.[1]
    let logFilePath = argv.[2]
    let clocFilPath = argv.[3]
    let outputFilePath = argv.[4]
    let userMappingPath = if Array.length argv = 6 then argv.[5] else  null;
    let userMappingFunc = createUserMappingFunc userMappingPath

    let commitCount, svnData = svnlog.getCommitInfoPerFile(logFilePath, repoPrefix, userMappingFunc)
    let maxCommitCount = svnData |> Seq.map (fun x -> x.commits |> Seq.sumBy (fun y -> y.commits)) |> Seq.max
    let authors = svnlog.authorsList svnData    
    let clocData = cloc.getClocData(clocFilPath, checkoutDir)
    let languages = cloc.getLanguages clocData      
    let filesTree = mergeFilesData clocData svnData |>  buildFilesTree    
    let serializedData = toJson { RootModel.Authors = authors
                                                      Languages = languages
                                                      TotalCommitCount = commitCount
                                                      MaxCommitCount = maxCommitCount
                                                      Data = filesTree }
    System.IO.File.WriteAllText(outputFilePath, serializedData, System.Text.Encoding.UTF8)    
    0