open System
open System.IO
 
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
 
#r "packages/FSharp.Compiler.Service/lib/net45/FSharp.Compiler.Service.dll"
//#r "FSharp.Compiler.Service.dll" //switch to this to try with fix

// Open the namespace with InteractiveChecker type
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices

// Create an interactive checker instance (ignore notifications)
let checker = FSharpChecker.Create()

let parseWithTypeInfo (file, input) = 
    let checkOptions = checker.GetProjectOptionsFromScript(file, input) |> Async.RunSynchronously
    //printfn "%A" checkOptions
    
    match checker.ParseAndCheckFileInProject(file, 0, input, checkOptions) |> Async.RunSynchronously with 
    | untypedRes, FSharpCheckFileAnswer.Succeeded(res) -> 
        untypedRes, res
        
    | _, res -> failwithf "Parsing did not finish... (%A)" res

let input = 
  """
#r "packages/FSharp.Management/lib/net40/FSharp.Management.dll"
open FSharp.Management
type Local = FileSystem< ".", watch = true >
Local.
  """
let inputLines = input.Split('\n')
let file = Path.Combine(Environment.CurrentDirectory, "Test.fsx")
let untyped, parsed = parseWithTypeInfo (file, input)
//printfn "%A" inputLines

// Get declarations (autocomplete) for a location

//printfn "%A" untyped.Errors

let decls = 
    parsed.GetDeclarationListInfo(Some untyped, 5, 7, inputLines.[4], ["Local"], "", fun (_,_) -> false) 
    |> Async.RunSynchronously

for item in decls.Items do
    printfn " - %s" item.Name
for item in decls.Items do
    printfn " - %s - %A" item.Name item.DescriptionText
