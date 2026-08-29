namespace Mephi.Cybernetics.Race.Core.Host.Computing

open Mephi.Cybernetics.Race.Core.Host.Definitions

open System



//Version 1

type NetList<'T> = System.Collections.Generic.List<'T>

type SimpleReduction( applicationTff : IApplicationTff ) =
    let rec seqWithSingleReducedItem terms reduceWithArgs = 
        Seq.fold (
                    fun ( termSeq : seq<ITerm>, isReduced : bool ) elem -> 
                        let contractOrValue = reduceWithArgs(elem, Seq.empty, isReduced) in
                        (Seq.append termSeq (Seq.singleton (fst contractOrValue)), snd contractOrValue) 
                 )
                 (Seq.empty, false)
                 terms 

    let rec reduceWithArgs( term : ITerm, args : seq<ITerm>, isReduceStepWasMade : bool ) =
        if isReduceStepWasMade then
            if Seq.length args = 0 then 
                (term, true)
            else
                raise (new Exception ("Probably function was called with bad arguments"))
        else
            match term with
                | :? IApplicationTerm ->
                    let application = term :?> IApplicationTerm
                    let func = application.Function
                    let funcArgs = Seq.append application.Arguments args in
                    match func with
                        | :? IApplicationTerm ->
                            reduceWithArgs( func, funcArgs, false )
                        | :? IFunction ->
                            let reducibleObj = (func :?> IFunction)
                            let arity = reducibleObj.Arity in
                            if arity < Seq.length funcArgs then
                                let contract = Seq.singleton( reducibleObj.ReduceWithArguments( Seq.take arity funcArgs, applicationTff ) ) in
                                ( applicationTff.CreateTerm( Seq.append contract (Seq.skip reducibleObj.Arity funcArgs) ) :> ITerm, true )
                            else if reducibleObj.Arity = Seq.length args then
                                ( reducibleObj.ReduceWithArguments( funcArgs, applicationTff ), true )
                            else
                                let sequenceAndReduceResult = seqWithSingleReducedItem funcArgs reduceWithArgs in
                                ( applicationTff.CreateTerm( Seq.append (Seq.singleton func) (fst sequenceAndReduceResult) ) :> ITerm, snd sequenceAndReduceResult )
                        | _ ->
                            let sequenceAndReduceResult = seqWithSingleReducedItem funcArgs reduceWithArgs in
                                ( applicationTff.CreateTerm( Seq.append (Seq.singleton func) (fst sequenceAndReduceResult) ) :> ITerm, snd sequenceAndReduceResult )
                | _ -> (term, false)

    interface IReduction with
        member this.Evaluate( term ) =
            (this :> IReduction).EvaluateWithArguments( term, Seq.empty )      
                
        member this.EvaluateWithArguments( term :ITerm, args : seq<ITerm> ) =            
            let reduction = this :> IReduction in
            match term with
                | :? IApplicationTerm ->
                    let application = term :?> IApplicationTerm
                    let func = application.Function
                    let argsContracts = Seq.map reduction.Evaluate (Seq.append application.Arguments args) in
                    match func with
                        | :? IFunction ->
                            let reducibleObj = (func :?> IFunction) in
                            if reducibleObj.Arity < Seq.length argsContracts then                                
                                let funcContract = reducibleObj.ReduceWithArguments( Seq.take reducibleObj.Arity argsContracts, applicationTff )
                                let remainingContracts = Seq.skip reducibleObj.Arity argsContracts in
                                reduction.EvaluateWithArguments( funcContract, remainingContracts )
                            else if reducibleObj.Arity = Seq.length argsContracts then
                                reduction.Evaluate ( reducibleObj.ReduceWithArguments( argsContracts, applicationTff ) )
                            else 
                                if reducibleObj.IsReducibleWith argsContracts then
                                    reduction.Evaluate ( reducibleObj.ReduceWithArguments( argsContracts, applicationTff ) )
                                else
                                    applicationTff.CreateTerm( Seq.append ( Seq.singleton func ) argsContracts ) :> ITerm
                        | _ ->
                            applicationTff.CreateTerm( Seq.append ( Seq.singleton func ) argsContracts ) :> ITerm
                | :? ICompositeTerm as composite -> 
                    composite.GetTff.CreateTerm( Seq.append (Seq.cast<ITerm>(composite.GetBoundVariables)) (Seq.map reduction.Evaluate composite.SubTerms) ) :> ITerm
                | _ ->
                    term

        member this.GetArity( term : ITerm ) =
            match term with
                | :? IApplicationTerm ->
                    let application = term :?> IApplicationTerm
                    let func = application.Function
                    let funcArgs = application.Arguments
                    let arity = (this :> IReduction).GetArity( func ) - Seq.length funcArgs in
                    if arity > 0 then 
                        arity
                    else 
                        0
                | :? IFunction ->
                    (term :?> IFunction).Arity
                | _ -> 0      

        member this.HasRedex( term : ITerm ) = 
            (this :> IReduction).IsReducibleWith( term, Seq.empty )
                                
        member this.IsReducibleWith( term : ITerm, args : seq<ITerm> ) =
             match term with
                | :? IApplicationTerm ->
                    let application = term :?> IApplicationTerm
                    let func = application.Function
                    let funcArgs = Seq.append application.Arguments args in
                    (this :> IReduction).IsReducibleWith( func, funcArgs )
                | :? IFunction ->
                    let reducibleObj = term :?> IFunction in
                    if reducibleObj.Arity <= Seq.length args then
                        true
                    else
                        Seq.exists (this :> IReduction).HasRedex args
                | _ ->
                    Seq.exists (this :> IReduction).HasRedex args
                            
        member this.Reduce( term : ITerm ) =
            fst ( reduceWithArgs( term, Seq.empty, false ) )
        
        member this.ReduceWithArguments( term : ITerm, args : seq<ITerm> ) =
            fst ( reduceWithArgs( term, Seq.take ( (this :> IReduction).GetArity( term ) ) args, false ) )  //?
    end

    type Reduction = 
        static member SimpleReduction (applicationTff : IApplicationTff) : IReduction = 
            SimpleReduction applicationTff :> IReduction
    //Version 2

//type SimpleReduction() =
//    let rec seqWithSingleReducedItem terms reduceWithArgs = 
//        Seq.fold (
//                    fun ( termSeq : seq<ITerm>, isReduced : bool ) elem -> 
//                        let contractOrValue = reduceWithArgs(elem, Seq.empty, isReduced) in
//                        (Seq.append termSeq (Seq.singleton (fst contractOrValue)), snd contractOrValue) 
//                 )
//                 (Seq.empty, false)
//                 terms 
//
//    let rec reduceWithArgs( term : ITerm, args : seq<ITerm>, isReduceStepWasMade : bool ) =
//        if isReduceStepWasMade then
//            if Seq.length args = 0 then 
//                (term, true)
//            else
//                raise (new Exception ("Probably function was called with bad arguments"))
//        else
//            match term with
//                | :? IApplicationTerm ->
//                    let application = term :?> IApplicationTerm
//                    let termSeq = application.ToSeq
//                    let func = Seq.head termSeq
//                    let funcArgs = Seq.skip 1 termSeq in
//                    match func with
//                        | :? IApplicationTerm ->
//                            reduceWithArgs( func, funcArgs, false )
//                        | :? IReducible ->
//                            let reducibleObj = (func :?> IReducible) in
//                            if reducibleObj.Arity < Seq.length funcArgs then
//                                let contract = Seq.singleton( reducibleObj.ReduceWithArguments( Seq.take reducibleObj.Arity funcArgs ) ) in
//                                ( application.GetApplicationTFF.CreateTerm( Seq.append contract (Seq.skip reducibleObj.Arity funcArgs) ), true )
//                            else if reducibleObj.Arity = Seq.length args then
//                                ( reducibleObj.ReduceWithArguments( funcArgs ), true )
//                            else
//                                let sequenceAndReduceResult = seqWithSingleReducedItem funcArgs reduceWithArgs in
//                                ( application.GetApplicationTFF.CreateTerm( Seq.append (Seq.singleton func) (fst sequenceAndReduceResult) ), snd sequenceAndReduceResult )
//                        | _ ->
//                            let sequenceAndReduceResult = seqWithSingleReducedItem funcArgs reduceWithArgs in
//                                ( application.GetApplicationTFF.CreateTerm( Seq.append (Seq.singleton func) (fst sequenceAndReduceResult) ), snd sequenceAndReduceResult )
//                | _ -> (term, false)
//
//    interface IReduction with
//        member this.Evaluate( term ) =
//            match term with
//                | :? IApplicationTerm ->
//                    let application = term :?> IApplicationTerm
//                    let termSeq = application.ToSeq
//                    let func = Seq.head termSeq
//                    let argsContracts = Seq.map (this :> IReduction).Evaluate (Seq.skip 1 termSeq) in
//                    match func with
//                        | :? IReducible ->
//                            let reducibleObj = (func :?> IReducible) in
//                            if reducibleObj.Arity < Seq.length argsContracts then                                
//                                let funcContract = Seq.singleton( reducibleObj.ReduceWithArguments( Seq.take reducibleObj.Arity argsContracts ) )
//                                let remainingContracts = Seq.skip reducibleObj.Arity argsContracts in
//                                application.GetApplicationTFF.CreateTerm( Seq.append funcContract remainingContracts )
//                            else if reducibleObj.Arity = Seq.length argsContracts then
//                                reducibleObj.ReduceWithArguments argsContracts
//                            else
//                                application.GetApplicationTFF.CreateTerm( Seq.append ( Seq.singleton func ) argsContracts )
//                        | _ ->
//                            application.GetApplicationTFF.CreateTerm( Seq.append ( Seq.singleton func ) argsContracts )
//                | _ -> term                            
//
//        member this.GetArity( term ) =
//            match term with
//                | :? IApplicationTerm ->
//                    let termSeq = (term :?> IApplicationTerm).ToSeq
//                    let func = Seq.head termSeq
//                    let funcArgs = Seq.skip 1 termSeq
//                    let arity = (this :> IReduction).GetArity( func ) - Seq.length funcArgs in
//                    if arity > 0 then 
//                        arity
//                    else 
//                        0
//                | :? IReducible ->
//                    (term :?> IReducible).Arity
//                | _ -> 0      
//
//        member this.HasRedex( term : ITerm ) = 
//            (this :> IReduction).IsReducibleWith( term, Seq.empty )
//                                
//        member this.IsReducibleWith( term : ITerm, args : seq<ITerm> ) =
//             match term with
//                | :? IApplicationTerm ->
//                    let termSeq = (term :?> IApplicationTerm).ToSeq
//                    let func = Seq.head termSeq
//                    let funcArgs = Seq.skip 1 termSeq in
//                    (this :> IReduction).IsReducibleWith( func, funcArgs )
//                | :? IReducible ->
//                    let reducibleObj = term :?> IReducible in
//                    if reducibleObj.Arity <= Seq.length args then
//                        true
//                    else
//                        Seq.exists (this :> IReduction).HasRedex args
//                | _ ->
//                    Seq.exists (this :> IReduction).HasRedex args
//                            
//        member this.Reduce( term : ITerm ) =
//            fst ( reduceWithArgs( term, Seq.empty, false ) )
//        
//        member this.ReduceWithArguments( term : ITerm, args : seq<ITerm> ) =
//            fst ( reduceWithArgs( term, Seq.take ( (this :> IReduction).GetArity( term ) ) args, false ) ) 
//    end

//type ReductionWithTypeCheck( simpleReduction: IReduction, typeSystem : ITypeSystem ) =
//    //interface ITypedReduction
//    member this.IsTermValide( term : ITerm ) =
//        typeSystem.IsValidTerm( term )
//
//    interface IReduction with
//        member this.Evaluate( term ) =
//            if typeSystem.IsValidTerm term    
//                
//        member this.EvaluateWithArguments( term :ITerm, args : seq<ITerm> ) =
//            
//
//        member this.GetArity( term : ITerm ) =
//            
//
//        member this.HasRedex( term : ITerm ) = 
//           
//                                
//        member this.IsReducibleWith( term : ITerm, args : seq<ITerm> ) =
//            
//                            
//        member this.Reduce( term : ITerm ) =
//           
//        
//        member this.ReduceWithArguments( term : ITerm, args : seq<ITerm> ) =
//           
//    end


