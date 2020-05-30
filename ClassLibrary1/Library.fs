namespace ClassLibrary1
open System
open System.IO
open System.Security.Cryptography
open MetadataExtractor
open MetadataExtractor.IO
//let directory = "/media/alex_new/Seagate Backup Plus Drive"
type FoundPicture ={
    HashStr :string
    Name:string
    CreationTimeUTC:DateTime
    LastWriteTimeUTC:DateTime
}
module Say =
    let directory = "/home/alex_new/Pictures/Test/Original"
    
    let m (q:Directory) = q.Tags |> Seq.filter(fun item -> item.Name = "" || item.Name ="")


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
    let returnAllFiles (directory:DirectoryInfo) = directory.GetFiles()
    let returnResults (file:FileInfo) =  using (SHA256.Create()) (fun mySHA256 ->
                                                                        using (file.Open(FileMode.Open))
                                                                            (fun fileStream ->  fileStream.Position <-0L
                                                                                                let hashValue = mySHA256.ComputeHash(fileStream)
                                                                                                let hash =  (ByteToHex hashValue)                                                                
                                                                                                hash, {HashStr=hash;Name=file.FullName;CreationTimeUTC=file.CreationTimeUtc;LastWriteTimeUTC=file.LastWriteTimeUtc}
                                                                        )
                                                                 )    
            
    let findFiles directory =
        
        let dir = new DirectoryInfo(directory)
        
        dir.GetDirectories()
            |> Array.map returnAllFiles
            |> Array.concat
            
            |> Array.map returnResults                                   
                                     
    
    let dateStringFromDate (date:System.DateTime) = date.ToString("yyyy-MM-dd")                
        
             
                        
    let m = findFiles directory |> Map.ofArray
    let hello name =
        printfn "Hello %s" name
