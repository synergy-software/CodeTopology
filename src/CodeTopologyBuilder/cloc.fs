[<RequireQualifiedAccess>]
module cloc
open FSharp.Data

type cloc = CsvProvider<"SampleData\cloc.csv">
type FileOfCode={fileName:string; language:string; loc:int}

let private normalizePath (path:string)=
    path.Replace("\\", "/")

let private adjustFilePath filePath normalCheckoutDir = 
    (normalizePath filePath).Replace(normalCheckoutDir, "").TrimStart([| '/' |])

let getClocData (clocFilePath:string, checkoutDir) = 
    let normalCheckoutDir = normalizePath checkoutDir    
    cloc.Load(clocFilePath).Rows  
    |> Seq.map (fun x -> 
           { FileOfCode.fileName  =  adjustFilePath x.Filename normalCheckoutDir
             language = x.Language
             loc = x.Code })
    |> Seq.toArray

let getLanguages (c:FileOfCode [])=
    c |> Array.map (fun x -> x.language) |> Array.distinct