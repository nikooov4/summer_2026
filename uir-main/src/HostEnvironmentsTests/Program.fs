module Mephi.Cybernetics.Race.HostEnvironments.Tests.Program

open Mephi.Cybernetics.Race.Core.Host
open Mephi.Cybernetics.Race.Core.Host.Computing
open Mephi.Cybernetics.Race.Core.Host.Computing.TypeSystem
open Mephi.Cybernetics.Race.Core.Host.Definitions
open Mephi.Cybernetics.Race.Core.Host.Definitions.TypeSystem
open Mephi.Cybernetics.Race.Core.Host.Definitions.Tff
open Mephi.Cybernetics.Race.Objects


open System.Xml


[<EntryPoint>]
let main argv = 
    //let intVal = 3
    //
    //
    //
//    let applicationTff = MultiApplicationTff :> IApplicationTff
    //
    //let list = [(Factorial :> ITerm) ; (Int(intVal) :> ITerm)]
    //let fac = applicationTff.CreateTerm( Seq.cast list ) :> ITerm
    //
//    let reduction = SimpleReduction(applicationTff) :>IReduction
    //let typeVarProvider = TypeVariableProvider()
    //let subst = Substitution()
    //let termType = TermType(typeVarProvider, subst)
    //let avalDep = [termType; TypeType; TypeTerm] :> seq<TermTypeDependency>
    //let contextProvider = ContextProvider()
    //let funcTff = FunctionalTypeTff()
    //let typeSys = TypeSystem(avalDep, reduction, contextProvider :> IContextProvider, funcTff )
    //
    //let stopWatch1 = System.Diagnostics.Stopwatch.StartNew()
    //let redex = reduction.Evaluate(fac)
    //
    //stopWatch1.Stop()
    //printfn "1.1) %f" stopWatch.Elapsed.TotalMilliseconds
    //printfn "1.2) %f" stopWatch1.Elapsed.TotalMilliseconds
    //System.Console.WriteLine(TypeVariable("s") = TypeVariable("s"))
    ////System.Console.WriteLine("var : " + ((typeSys :> ITypeSystem).GetTermType (typeVarProvider :> ITypeVariableProvider).CreateVariable).ToString())
    //
    //stopWatch.Restart()
    //
    //let rec facFun num = 
    //    if num = 0 then 1
    //    else num * facFun (num - 1)
    //
    //let value = facFun intVal
    //
    //stopWatch.Stop()
//    let gpu = new GpuReduction()
//    
//    let numeral = ChurchNumeral(2, DefaultVariable("s1"), DefaultVariable("z1"))
//    let zero = ChurchNumeral(3, DefaultVariable("s2"), DefaultVariable("z2"))
//    let a = DefaultVariable("a")
//    let b = DefaultVariable("b")
//    let c = DefaultVariable("c")
//    let d = DefaultVariable("d")
//    let plus = DefaultMultiLambda(seq [a; b; c; d], MultiApplicationTerm(seq [a; c; MultiApplicationTerm(seq [b; c; d])]))
//    let mult = (Mult :> ITermAlias).GetTerm
//    let termToEvaluate = MultiApplicationTerm(seq [plus; numeral.GetTerm; zero.GetTerm ])
//    let stopWatch = System.Diagnostics.Stopwatch.StartNew()
//
//    let newGpuNumeral = gpu.Evaluate(termToEvaluate)
//    stopWatch.Stop()
//
//    stopWatch.Restart()
//
//    let newNumeral = reduction.Evaluate(termToEvaluate)
//    stopWatch.Stop()
//       
//
//    let gpuTime =({1..5} |> 
//    Seq.map (fun x -> stopWatch.Restart()
//                      ignore(gpu.Evaluate(termToEvaluate))
//                      stopWatch.Stop()
//                      stopWatch.Elapsed.TotalMilliseconds) |>
//    Seq.fold (fun acc el -> acc + el) (float(0))) / float(5)
//
//    let cpuTime =({1..5} |> 
//    Seq.map (fun x -> stopWatch.Restart()
//                      ignore(reduction.Evaluate(termToEvaluate))
//                      stopWatch.Stop()
//                      stopWatch.Elapsed.TotalMilliseconds) |>
//    Seq.fold (fun acc el -> acc + el) (float(0))) / float(5)
//
//    printfn "Время gpu: %f, терм: %s" (gpuTime) (newGpuNumeral.ToString())
//    printfn "Время cpu: %f, терм: %s" (cpuTime) (newNumeral.ToString())


    System.Console.ReadLine()
    0
    //let newNumeral = reduction.Evaluate(MultiApplicationTerm(seq [Succ :> ITerm; numeral.GetTerm ]))
    //let i = NumeralToInt( reduction.Reduce( MultiApplicationTerm(seq [succ :> ITerm; numeral :> ITerm]) ) )
