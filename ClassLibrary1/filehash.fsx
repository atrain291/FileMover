#r "/home/alex_new/RiderProjects/ClassLibrary1/ClassLibrary1/MetadataExtractor.dll"
#r "/home/alex_new/.nuget/packages/xmpcore/6.1.10/lib/netstandard2.0/XmpCore.dll"

open System.IO
open System.Security.Cryptography
open System
open MetadataExtractor
open MetadataExtractor.IO
//let directory = "/media/alex_new/Seagate Backup Plus Drive"
let directory = "/home/alex_new/Pictures/Test/Original"
let newdirectory = "/home/alex_new/Pictures/Test/Duplicate"
let outputdirectory = "/home/alex_new/Pictures/Test/Output"

type FoundPicture ={
    HashStr :string
    FullPath:string
    FileName:string
    CreationTimeUTC:DateTime
    LastWriteTimeUTC:DateTime
}

let printByteArray (byteArray:byte[]) =
    for i in 0 .. byteArray.Length do
        printf "%O" byteArray
        match i%4 with
           | 3 -> printf " "
           | _ -> ()
    ()
        
let ByteToHex bytes = 
    bytes 
    |> Array.map (fun (x : byte) -> System.String.Format("{0:X2}", x))
    |> String.concat System.String.Empty

let returnAllFiles (directory:DirectoryInfo) =
            printfn "Getting files from %s" directory.FullName
            let files = directory.GetFiles()
            printfn "Found %i files " (files |> Array.length)
            files
            
let returnResults (file:FileInfo) =  using (SHA256.Create()) (fun mySHA256 ->
                                                                        using (file.Open(FileMode.Open))
                                                                            (fun fileStream ->  fileStream.Position <-0L
                                                                                                let hashValue = mySHA256.ComputeHash(fileStream)
                                                                                                let hash =  (ByteToHex hashValue)                                                                
                                                                                                {HashStr=hash;FullPath=file.FullName;FileName=file.Name;CreationTimeUTC=file.CreationTimeUtc;LastWriteTimeUTC=file.LastWriteTimeUtc}
                                                                        )
                                                                 )

let returnMetaData (imagePath:string) = ImageMetadataReader.ReadMetadata(imagePath);
let dateStringFromDate (date:System.DateTime) = date.ToString("yyyy-MM-dd")
let mapOfStuff = Map.empty<string,string[]>


let makeDictionaryEntry (fileInfo:FoundPicture) =
         let dateString = dateStringFromDate fileInfo.CreationTimeUTC 
         let result = dateString,fileInfo.HashStr,fileInfo
         
         match mapOfStuff.TryFind(dateString) with
            | Some (stuff:string[]) -> Array.append stuff [|fileInfo.HashStr|] |> ignore                
            | None -> mapOfStuff.Add(dateString, [|fileInfo.HashStr|] ) |> ignore
         result   
            
let dir1= new DirectoryInfo(directory)

let processFiles (foundPicture:FoundPicture) =
    //Get Creation Date
    let creationDate = dateStringFromDate foundPicture.CreationTimeUTC
    let dir = new DirectoryInfo(outputdirectory+ "/" + creationDate)
    
    match mapOfStuff.TryFind(creationDate) with
        | Some stuff -> // check for hash
                        ()
        | None -> ()
    //See if Date exists as a folder in output
    //if not Create
    match dir.Exists with
        | true -> printfn "Directory %s already exists" dir.FullName
        | false -> dir.Create()
                   printfn "Creating Directory %s does not exist" dir.FullName
                   
        
    let filesFromDate = returnAllFiles dir |>
                        Array.map(returnResults) 
             
    //read files in folder see if hash matches
    //if not copy file to that folder

    match filesFromDate |> Array.exists(fun item -> item.HashStr = foundPicture.HashStr) with
                | true -> printfn "Found %s %s" foundPicture.FileName (foundPicture.CreationTimeUTC.ToString "yyyy/MM/dd HH:mm:ss") 
                | false ->   let newFullPath = dir.FullName + "/" + foundPicture.FileName                             
                             System.IO.File.Copy(foundPicture.FullPath, newFullPath)
                             printfn "%s is unique. Copied from\t%s\tto\t%s" foundPicture.FileName foundPicture.FullPath newFullPath

                                                    
let getFileInfosFromDirectoryPath directory =
        let dir = new DirectoryInfo(directory)
        let fileInfos = dir1.GetDirectories()
                            |> Array.map returnAllFiles
                            |> Array.concat
                            |> Array.take 14                            
                            |> Array.map returnResults
        fileInfos                                                    
                                                    
let le = function
  | [x] -> x
  | _ -> failwith "incompatible"
let lt f = function
  | [] -> failwith "incompatible"
  | x::xs -> (x, f xs) 
 

                       
let fileCreationDateStringToDateTime (s:string) =    let len = s.Length                                                        
                                                     let x = s.Substring(4,6)
                                                     let y = s.Substring(len-4,4)
                                                     let z = s.Substring(10,9)
                                                     let dateStr = x + " " + y + " " + z
                                                     DateTime.Parse(dateStr)
                       

                            
let fileDirectory = getFileInfosFromDirectoryPath directory
           
let findNameAndDateCreated (fileDirectory:Directory)= let result = fileDirectory.Tags
                                                                      |> Seq.filter(fun tag -> tag.Name = "File Name" || tag.Name ="File Modified Date")
                                                                      |> Seq.map(fun tag-> tag.Description)
                                                                      |> Seq.chunkBySize 2                    
                                                                      |> Seq.collect(fun flattenedTranslatedTagsSequence -> flattenedTranslatedTagsSequence)
                                                                      |> Seq.toList
                                                                      |> (lt le)
                                                      (snd result),(fst result)           
                            
let filterNameAndCreatedDate (picture : FoundPicture) =
                                        let metadata = returnMetaData(picture.FullPath)
                                                        |> Array.ofSeq
                                                        |> Array.filter (fun metaDataDirectory -> metaDataDirectory.Name = "File")
                                                        |> Array.map findNameAndDateCreated
                                                        |> Array.head
                                        {HashStr=picture.HashStr;FullPath=picture.FullPath;FileName=(snd metadata);CreationTimeUTC=(fileCreationDateStringToDateTime(fst metadata));LastWriteTimeUTC=picture.LastWriteTimeUTC}
                                            

let res = getFileInfosFromDirectoryPath directory
                |> Array.map(fun i-> filterNameAndCreatedDate i)
                |> Array.iter(fun file -> processFiles file)


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// 

 