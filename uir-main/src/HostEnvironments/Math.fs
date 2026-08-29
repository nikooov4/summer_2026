module Mephi.Cybernetics.Race.Core.Host.Definitions.Math
//
//open Mephi.Cybernetics.Race.Core.Host.Definitions
//open TypeSystem
//open System
//open SharpDX
//open SharpDX.Direct3D11
//open SharpDX.DXGI
//open SharpDX.Direct3D
//
//type NumType() =
//    override this.ToString() = "Num"
//    interface IBasicType with
//        member this.GetName = "Num"
//
//type Num = 
//    | Int of int
//    | Float of float
//    interface ITerm
//    interface ITyped with 
//        member this.GetType(typrSys, context) = NumType() :> IType
//                    
//and Multiply =
//    | MultiPly
//    interface ITyped with
//        member this.GetType(typeSys, context) =
//            (FunctionalTypeTff() :> IFunctionalTypeTff).CreateTerm( [NumType() :> ITerm; NumType() :> ITerm; NumType() :> ITerm] :> seq<ITerm>) :> IType
//    interface IReducible with
//        member this.Arity 
//            with get () : int = 2
//
//        member this.IsReducibleWith args =
//            Seq.length args >= 2
//
//        member this.ReduceWithArguments( args, applicationTff) = (Seq.head  
//            
//
//
//    interface IHasSpecificReductionStrategy with
//        member this.GetReductionStrategy = Applicative
//
//and Factorial =
//    | Factorial
//
//    interface ITyped with
//        member this.GetType(typeSys, context) = 
//            FunctionalType([NumType() :> IType; NumType() :> IType] :> seq<IType>) :> IType
//    interface IReducible with
//        member this.Arity
//            with get () : int = 1
//
//        member this.IsReducibleWith args =
//            Seq.length args = 1
//        
//        member this.ReduceWithArguments( args, applicationTff) = 
//            let firstArgValue = 
//                match (Seq.head args) :?> Num with
//                    | Int(value) -> value
//                    | _ -> raise (new NotImplementedException()) //?
//            let secondArg =
//                if firstArgValue = 0 then
//                    Int(1) :> ITerm
//                else
//                    applicationTff.CreateTerm( Seq.append (Seq.singleton (Factorial :> ITerm)) ( Seq.singleton ( Int(firstArgValue - 1) :> ITerm ) ) ) :> ITerm
//
//            applicationTff.CreateTerm( Seq.cast [(MultiPly :> ITerm); (Int(firstArgValue) :> ITerm); secondArg] ) :> ITerm