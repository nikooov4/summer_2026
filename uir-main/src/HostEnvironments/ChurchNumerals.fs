module Mephi.Cybernetics.Race.Core.Host.Definitions.ChurchNumerals

open Mephi.Cybernetics.Race.Core.Host.Definitions
open Mephi.Cybernetics.Race.Core.Host.Definitions.Tff
open Mephi.Cybernetics.Race.Core.Host.Computing

let IsNumeral (term : ITerm) =
    let rec checkTerm (fstVar : IVariable) (sndVar : IVariable) (termToCheck : ITerm) = 
        match termToCheck with
            | :? IApplicationTerm as application ->
                if fstVar.Equals(application.LeftTerm :?> IVariable) then
                    checkTerm fstVar sndVar application.RightTerm
                else
                    false
            | :? IVariable as variable ->
                if variable.Equals sndVar then
                    true
                else 
                    false
            | _ ->
                false
        
    match term with
        | :? DefaultMultiLambda as lambda ->
            if Seq.length(lambda.Variables) = 2 then
                let firstVar = Seq.head lambda.Variables
                let secondVar = Seq.item 1 lambda.Variables
                let body = lambda.Body in
                checkTerm firstVar secondVar body
            else
                false
        | _ ->
            false

let NumeralToInt (term : ITerm) =
    let funcAbstraction = term :?> IFunctionalAbstraction
    let fstVar = Seq.head funcAbstraction.GetBoundVariables
    let sndVar = Seq.item 1 funcAbstraction.GetBoundVariables
    let rec countVar (term : ITerm) count = 
        match term with
            | :? IApplicationTerm as application ->
                if application.LeftTerm.Equals(fstVar) then
                    countVar application.RightTerm (count + 1)
                else
                    failwith("Term is not numeral")
            | :? IVariable as var ->
                if var.Equals(sndVar) then
                    count
                else
                    failwith("Term is not numeral")
            | _ ->
                failwith("Term is not numeral")
    countVar funcAbstraction.GetBody 0
                 
type ChurchNumeralType =
    | ChurchNumeralType
    interface IBasicType with
        member this.GetName = "Church numeral"
    
type ChurchNumeral(term : ITerm) =
    let numeralTerm = term
    do
        if not(IsNumeral(term)) then 
            failwith "Passed term is not a numeral"

    member this.GetTerm = term 

    new(num : int, firstVar : IVariable, secondVar : IVariable) =
        let infiniteSeq = Seq.initInfinite(fun e -> firstVar :> ITerm)
        let numeralBodyPart = Seq.take num infiniteSeq
        let numeralBodySeq = Seq.append numeralBodyPart (Seq.singleton (secondVar :> ITerm))
        let rec numeralBody (terms : seq<ITerm>) =
            if Seq.length terms = 1 then
                Seq.head terms
            else
                MultiApplicationTerm(seq [Seq.head terms; numeralBody (Seq.skip 1 terms)]) :> ITerm
             
        let term = DefaultMultiLambda(seq [firstVar; secondVar], numeralBody(numeralBodySeq)) in
        ChurchNumeral(term)

    interface ITerm
    interface ITyped with
        member this.GetTermType(typeSystem, context) =
            ChurchNumeralType :> IType
    end

type Succ =
    | Succ

    interface ITermAlias with
        //Добавить реализацию с передаваемыми переменными
        member this.GetTerm =
            let fstVar = DefaultVariable("n")
            let sndVar = DefaultVariable("s")
            let trdVar = DefaultVariable("z")
            let lambdaVars = seq [fstVar :> IVariable; sndVar :> IVariable; trdVar :> IVariable]
            let lambdaBodyPart = seq [fstVar :> ITerm; sndVar :> ITerm; trdVar :> ITerm]
            let lambdaBody = seq [ sndVar :> ITerm; MultiApplicationTerm(lambdaBodyPart) :> ITerm]
            DefaultMultiLambda(lambdaVars, MultiApplicationTerm(lambdaBody)) :> ITerm
    interface IFunction with
        member this.Arity = 1

        member this.IsReducibleWith args = 
            Seq.length args > 0 && IsNumeral(Seq.head args)

        member this.ReduceWithArguments(args, application) =
            let fstArg = Seq.head args in
            match fstArg with
                | :? IFunctionalAbstraction as abstraction ->
                    let variables = abstraction.GetBoundVariables
                    let fstVar = Seq.head variables
                    let newBodySeq = Seq.singleton( MultiApplicationTerm( seq[ fstVar :> ITerm; abstraction.GetBody ] ) :> ITerm )
                    abstraction.GetTff.CreateTerm( Seq.append (Seq.cast<ITerm> variables) newBodySeq ) :> ITerm
                | _ -> 
                    failwith("Argument is not numeral")

type Mult =
    | Mult

    interface ITermAlias with
        //Добавить реализацию с передаваемыми переменными
        member this.GetTerm =
            let n = DefaultVariable("n")
            let s = DefaultVariable("s")
            let z = DefaultVariable("z")
            let m = DefaultVariable("m")
            let lambdaVars = seq [n :> IVariable; m :> IVariable; s :> IVariable; z :> IVariable]
            let lambdaBodyPart = seq [m :> ITerm; s :> ITerm; ]
            let lambdaBody = seq [ n :> ITerm; MultiApplicationTerm(lambdaBodyPart) :> ITerm; z :> ITerm]
            DefaultMultiLambda(lambdaVars, MultiApplicationTerm(lambdaBody)) :> ITerm