module cloc
open FSharp.Data

type cloc = CsvProvider<"SampleData\cloc.csv">
type FileOfCode={fileName:string; language:string; loc:int}

let private shouldIgnoreFile(fileName:string)= 
     let lowerFileName = fileName.ToLower()
     lowerFileName.Contains("\\bin\\") ||
     lowerFileName.Contains("\\obj\\") ||
     lowerFileName.Contains("\\packages\\")

let private normalizePath (path:string)=
    path.Replace("\\", "/")

let private adjustFilePath filePath normalCheckoutDir = 
    (normalizePath filePath).Replace(normalCheckoutDir, "").TrimStart([| '/' |])

let getClocData (clocFilePath:string, checkoutDir) = 
    let normalCheckoutDir = normalizePath checkoutDir    
    cloc.Load(clocFilePath).Rows
    |> Seq.filter (fun x -> shouldIgnoreFile (x.Filename) = false)
    |> Seq.map (fun x -> 
           { FileOfCode.fileName  =  adjustFilePath x.Filename normalCheckoutDir
             language = x.Language
             loc = x.Code })
    |> Seq.toArray

let getLanguages (c:FileOfCode [])=
    c |> Array.map (fun x -> x.language) |> Array.distinct