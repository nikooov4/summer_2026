module Mephi.Cybernetics.Race.Core.Host.Definitions.Tff

open System
open Mephi.Cybernetics.Race.Core.Host.Definitions
open Mephi.Cybernetics.Race.Core.Host.Computing
open Mephi.Cybernetics.Race.Core.Host.Computing.TypeSystem

exception NoVariableInLambdaException
exception BadTermNumber

type SimpleLambdaTff( substitution : ISubstitution) =
    
    interface ITff with
        member this.CreateTerm ( terms : seq<ITerm> ) =
            let term = 
                terms |>
                    Seq.reduceBack 
                        (fun (t : ITerm) (v: ITerm) ->
                                if v :? IVariable then
                                    SimpleLambda (v :?> IVariable, t, substitution) :> ITerm
                                else
                                    failwith "")
            term :?> ICompositeTerm
            (*if Seq.length terms = 2 then
                let possibleVariable = Seq.head terms
                let body = Seq.nth 1 terms in

                if possibleVariable :? IVariable then
                    SimpleLambda(possibleVariable :?> IVariable, body, substitution) :> ICompositeTerm
                else
                    failwith "First term is not a variable"
            else
                failwith "Bad terms number"*)
                  
and DefaultSimpleLambdaTff() =
    inherit SimpleLambdaTff(DefaultSubstitution)      

and MultiLambdaTff( substitution : ISubstitution ) =
    
    interface ITff with
        member this.CreateTerm ( terms : seq<ITerm> ) =
            try
                let possibleVariables = Seq.cast<IVariable> (Seq.take (Seq.length terms - 1) terms)
                let term = Seq.last terms in
                
                if Seq.length possibleVariables = 0 then
                    raise (NoVariableInLambdaException)
                else
                    MultiLambda (possibleVariables, term, substitution) :> ICompositeTerm
            with
                | :? NoVariableInLambdaException ->
                    raise (new NotImplementedException())
                | _ ->
                    raise (new NotImplementedException())

and DefaultMultiLambdaTff() =
    inherit MultiLambdaTff(DefaultSubstitution)

and SimpleLambda( variable : IVariable, body : ITerm, substitution : ISubstitution ) =
    
    override this.ToString() =
        "\\" + variable.ToString() + " -> " + body.ToString()

    member this.Body
        with get() = body

    member this.Variable
        with get() = variable

    interface ITerm

    interface IFunctionalAbstraction with
        member this.GetBasicBody = body
        member this.GetBody = body
        member this.GetBoundVariable = variable
        member this.GetBoundVariables = Seq.singleton variable
    end

    interface ICompositeTerm with
        member this.GetBoundVariables = 
            Seq.singleton variable

        member this.GetTff =
            SimpleLambdaTff(substitution) :> ITff

        member this.SubTerms =
            Seq.singleton body
    end

and MultiLambda( variables : seq<IVariable>, term : ITerm, substitution : ISubstitution ) =   
//
//    member x.Body
//        with get () : ITerm = 
//            let (MultiLambda (_, body)) = x in
//            body
//    member x.Variables
//        with get () : seq<IVariable> =
//            let (MultiLambda (vars, _)) = x in
//            vars
    override this.ToString() =
        "\\" + String.Join(", ", Seq.map (fun el -> el.ToString()) variables) + " -> " + term.ToString()
    member this.Body
        with get() = term
    
    member this.Variables
        with get() = variables

    interface ITerm
    
    interface IFunctionalAbstraction with
        member this.GetBody =
            term
        member this.GetBasicBody =
            if Seq.length variables > 1 then
                MultiLambda( Seq.skip 1 variables, term, substitution) :> ITerm
            else
                term
        member this.GetBoundVariables = 
            variables

        member this.GetBoundVariable = 
            Seq.head variables

    interface IFunction with
        member this.Arity = 
            Seq.length variables

        member this.IsReducibleWith( args ) =
            Seq.length args > 0

        member this.ReduceWithArguments( args, applicationTff ) =
            if Seq.length args = 0 then
                raise (new Exception("It's not possible to reduce functional abstraction without arguments"))
            else
                let argsNum = Seq.length args
                let varsNum = Seq.length variables
                let newTerm = substitution.Substitute( term, Seq.zip variables args ) in
                
                if argsNum < varsNum then
                    MultiLambda( Seq.cast<IVariable>(Seq.skip argsNum variables), newTerm, substitution ) :> ITerm
                else if argsNum = varsNum then                    
                    newTerm
                else
                    applicationTff.CreateTerm( Seq.append (Seq.singleton newTerm) (Seq.skip (argsNum - varsNum) args) ) :> ITerm
    end

    interface ICompositeTerm with
        member this.GetBoundVariables = 
            variables

        member this.GetTff =
            MultiLambdaTff(substitution) :> ITff

        member this.SubTerms =
            Seq.singleton term
                        
//    interface ITerm with
//        member x.Arity
//            with get () =
//                let (MultiLambda (v, body)) = x in
//                Seq.length v + body.Arity
//        member x.VariableOccurs (variable : IVariable) : bool =
//            x.Body.VariableOccurs variable
//        member x.Substitute (variable : IVariable, term : ITerm) : ITerm = 
//            let (MultiLambda (vars, body)) = x in
//            if Seq.exists (fun v -> variable = v) vars then
//                x :> ITerm
//            else
//                MultiLambda (vars, body.Substitute (variable, term)) :> ITerm
//    end    

    interface ITyped with
        member this.GetTermType( typeSystem, context ) = 
            // What about lazy?
            let returnType = typeSystem.GetTermType( term, context )

            let returnTypeSeq = 
                match returnType with
                    | :? IFunctionalType ->
                        (returnType :?> IFunctionalType).SubTerms
                    | _ ->
                        Seq.singleton (returnType :> ITerm)

            let contain = context.IsContain (Seq.head variables)
            let variablesTypes = 
                Seq.map
                    (fun var -> if context.IsContain var then context.GetTermType var else (TypeVariable(DateTime.Now.ToString()) :> IType))
                    variables
            FunctionalType( Seq.append variablesTypes (Seq.cast<IType> returnTypeSeq) ) :> IType
    end
//
//    interface ITFF with 
//        member x.SubTerms 
//            with get () : seq<ITerm>  = 
//                [|x.Body|] :> seq<ITerm>
//        member x.BindsVariables
//            with get () : seq<IVariable> =   
//                x.Variables
//        member x.IsReducibleWith (args : seq<ITerm>) : bool =
//            Seq.length args = (x :> ITerm).Arity
//        member x.ReduceWithArguments (args : seq<ITerm>, application : IApplication) : ITerm =
//            if (x :> ITFF).IsReducibleWith args then
//                Seq.zip x.Variables args
//                |> Seq.fold (fun term (variable,argument) -> term.Substitute (variable,argument)) x.Body
//            else
//                raise (new Exception ("Cannot reduce a multi-abstraction when the number of arguments differs from the arity of the term"))
//    end

and DefaultMultiLambda( variables : seq<IVariable>, term : ITerm ) =
    inherit MultiLambda(variables, term, DefaultSubstitution)

and DefaultSimpleLambda( variable : IVariable, term : ITerm ) =
    inherit SimpleLambda( variable, term, DefaultSubstitution )