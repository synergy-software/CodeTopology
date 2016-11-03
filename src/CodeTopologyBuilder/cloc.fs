module cloc
open FSharp.Data

type cloc = CsvProvider<"SampleData\cloc.csv">
type FileOfCode={fileName:string; language:string; loc:int}

let private fileIsFromBinOrObj(fileName:string)= 
     let lowerFileName = fileName.ToLower()
     lowerFileName.Contains("\\bin\\") ||
     lowerFileName.Contains("\\obj\\") 

let private normalizePath (path:string)=
    path.Replace("\\", "/")

let getClocData (clocFilePath:string, checkoutDir) = 
    let normalCheckoutDir = normalizePath checkoutDir
    let prefixToRemove = checkoutDir
    cloc.Load(clocFilePath).Rows
    |> Seq.filter (fun x -> fileIsFromBinOrObj (x.Filename) = false)
    |> Seq.map (fun x -> 
           { FileOfCode.fileName  =  (normalizePath x.Filename).Replace(checkoutDir, "")
             language = x.Language
             loc = x.Code })
    |> Seq.toArray

let getLanguages (c:FileOfCode [])=
    c |> Array.map (fun x -> x.language) |> Array.distinct