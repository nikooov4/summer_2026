namespace Mephi.Cybernetics.Race.Core.Host.Computing

open Mephi.Cybernetics.Race.Core.Host.Definitions

type DefaultVariable(name : string) =
    member this.Name
        with get() = name

    override this.ToString() =
        name

    override this.Equals term =
            match term with
                | :? IVariable as var ->
                    var.Name = name
                | _ ->
                    false

    interface IVariable with
        override this.Equals term =
            match term with
                | :? IVariable as var ->
                    var.Name = name
                | _ ->
                    false
                    
        member this.Name = name

//Временно, для Cemac.DocumantConverter
//TODO: убрать :-)
and VariableWithDataType(name : string, dataType : string) =
    member this.DataType
        with get() = dataType

    member this.Name
        with get() = name
    
    override this.ToString() =
        name

    override this.Equals term =
            match term with
                | :? IVariable as var ->
                    var.Name = name
                | _ ->
                    false

    interface IVariable with
        override this.Equals term =
            match term with
                | :? IVariable as var ->
                    var.Name = name
                | _ ->
                    false
                    
        member this.Name = name

and DefaultSubstitution =
    | DefaultSubstitution
    interface ISubstitution with
        member this.Substitute( term, varsAndTerms ) =
            let substitute = (this :> ISubstitution).Substitute
            match term with
                | :? IApplicationTerm ->
                    let application = term :?> IApplicationTerm in
                    //Вначале проверять на вхождение?
                    application.GetTff.CreateTerm( Seq.map (fun t -> substitute( t, varsAndTerms )) application.ToSeq) :> ITerm
                | :? ICompositeTerm ->
                    let composite = term :?> ICompositeTerm in
                    composite.GetTff.CreateTerm( Seq.map (fun t -> substitute( t, varsAndTerms )) composite.SubTerms ) :> ITerm
                | :? IVariable ->
                    if Seq.exists (fun (var, termToSubstitute) -> (term :?> IVariable).Equals var) varsAndTerms then
                        snd( Seq.find (fun (var, termToSubstitute) -> (term :?> IVariable).Equals var) varsAndTerms )
                    else
                        term
                | _ ->
                    term