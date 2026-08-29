#r "bin/Debug/Mephi.Cybernetics.Race.Core.Host.dll"
#r "bin/Debug/SharpDX.Direct3D11.dll"
#r "bin/Debug/SharpDX.dll"
#r "bin/Debug/SharpDX.D3DCompiler.dll"
#r "bin/Debug/SharpDX.DXGI.dll"
open Mephi.Cybernetics.Race.Core.Host.Definitions
open Application
open Math
open Reduction
open Tff
open TypeSystem
open Substitution
open ChurchNumerals


//let intVal = 3
//
//
//
let applicationTff = MultiApplicationTff :> IApplicationTff
//
//let list = [(Factorial :> ITerm) ; (Int(intVal) :> ITerm)]
//let fac = applicationTff.CreateTerm( Seq.cast list ) :> ITerm
//
let reduction = SimpleReduction(applicationTff) :>IReduction
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
let gpu = new GpuReduction()
let numeral = ChurchNumeral(1, DefaultVariable("s"), DefaultVariable("z"))
let zero = ChurchNumeral(0, DefaultVariable("s"), DefaultVariable("z"))
let a = DefaultVariable("a")
let b = DefaultVariable("b")
let c = DefaultVariable("c")
let d = DefaultVariable("d")
let plus = DefaultMultiLambda(seq [a; b; c; d], MultiApplicationTerm(seq [a; c; MultiApplicationTerm(seq [b; c; d])]))
let termToEvaluate = MultiApplicationTerm(seq [plus :> ITerm; numeral.GetTerm; numeral.GetTerm ])
let stopWatch = System.Diagnostics.Stopwatch.StartNew()

let newGpuNumeral = gpu.Evaluate(termToEvaluate)
stopWatch.Stop()
printfn "Время gpu: %f, терм: %s" (stopWatch.Elapsed.TotalMilliseconds) (newGpuNumeral.ToString())

stopWatch.Restart()

let newNumeral = reduction.Evaluate(termToEvaluate)
stopWatch.Stop()
printfn "Время cpu: %f, терм: %s" (stopWatch.Elapsed.TotalMilliseconds) (newNumeral.ToString())
//let newNumeral = reduction.Evaluate(MultiApplicationTerm(seq [Succ :> ITerm; numeral.GetTerm ]))
//let i = NumeralToInt( reduction.Reduce( MultiApplicationTerm(seq [succ :> ITerm; numeral :> ITerm]) ) )