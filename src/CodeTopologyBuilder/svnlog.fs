[<RequireQualifiedAccess>]
module svnlog

open FSharp.Data
open System.Xml.Linq
open System.IO
open System.Collections.Generic
type CommitsByAuthor = 
    { author : string
      commits : int }

type Commit = 
    { fileName : string
      commits : CommitsByAuthor [] }

type svnData = XmlProvider<"SampleData\svnlogfile.xml">
type CommitsPerAuthor = Dictionary<string,int>
type FilesCommits = Dictionary<string,CommitsPerAuthor>
type private MoveMap = Dictionary<string,string>

type private Removals()=
     let fileRemovals = MoveMap()
     let dirRemovals = MoveMap()
     let (|Dir|File|) (path:svnData.Path)=
        if path.Kind = "dir" then Dir else File

     let mapDirPath (path:string)=
        match dirRemovals.Keys |> Seq.tryFind (fun x -> path.StartsWith(x)) with
        | None -> path
        | Some(x) -> path.Replace(x, dirRemovals.[x])

     let mapFilePath (path: svnData.Path)=
        match fileRemovals.TryGetValue(path.Value) with
        | true, value -> value
        | false, _ -> path.Value
        |> mapDirPath

     let addMove (moveSet:MoveMap) path copyFrom=
        let currentPath = match moveSet.TryGetValue(path) with
                                      | true, value -> value
                                      | false,_ -> path
        moveSet.[copyFrom] <- currentPath

     member this.update (paths: svnData.Path [])=
        for c in paths |> Seq.filter(fun x -> Option.isSome x.CopyfromPath) do
            let copyFrom = Option.get c.CopyfromPath            
            if paths |> Seq.exists (fun x -> x.Action = "D" && x.Value = copyFrom) then                
                match c with
                | Dir -> addMove dirRemovals c.Value copyFrom
                | File -> addMove fileRemovals c.Value copyFrom

     member this.mapPaths (paths: svnData.Path [])=
        paths |> Array.map mapFilePath
 
let private getCommitsData (state:FilesCommits) path=
        if state.ContainsKey(path) then
            state.[path]
        else
        let x = new CommitsPerAuthor()
        state.Add(path, x)
        x

let getCommitInfoPerFile(logFilePath:string, repoPrefix, userMappingFunc: (string->string)) =    
    let removals = Removals()

    let addCommitData (state : FilesCommits) (commitData : svnData.Logentry) =         
        let author = userMappingFunc commitData.Author
        removals.update commitData.Paths
        for path in commitData.Paths |> removals.mapPaths do       
            let commitInfo = getCommitsData state path
            commitInfo.[author] <- match commitInfo.ContainsKey(author) with
                                    | true -> commitInfo.[author] + 1
                                    | false -> 1
        state

    let mapToCommit (filePath : string) (commitsPerAuthor : CommitsPerAuthor) = 
        { Commit.fileName = filePath.Replace(repoPrefix, "")
          commits = 
              commitsPerAuthor
              |> Seq.map (fun (KeyValue(author, numberOfCommits)) -> 
                     { CommitsByAuthor.author = author
                       commits = numberOfCommits })
              |> Seq.toArray }
    
    
    let logentries = svnData.Load(logFilePath).Logentries
    
    let commitDataPerFile = 
        logentries
        |> Array.fold (fun state commitData -> addCommitData state commitData) (new FilesCommits())
        |> Seq.map (fun  (KeyValue(x, y)) -> mapToCommit x y)
        |> Seq.toArray
    
    (logentries.Length, commitDataPerFile)

let authorsList (commits:Commit[]) =
    commits
    |> Seq.collect (fun x-> x.commits)
    |> Seq.map (fun x-> x.author)    
    |> Seq.distinct
    |> Seq.sort
    |> Seq.toArray