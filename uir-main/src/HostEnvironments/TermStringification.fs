namespace Mephi.Cybernetics.Race.Objects

open System
open System.Xml
open System.Reflection
open System.Text.RegularExpressions
open Mephi.Cybernetics.Race.Core.Host.Definitions
open Mephi.Cybernetics.Race.Core.Host.Definitions.Tff
open Mephi.Cybernetics.Race.Core.Host.Computing

type ITermParser = interface
    abstract Parse: string -> ITerm
end

and TffTypeAttribute = class
    inherit Attribute
    val private _type : Type
    new (tffType : Type) = 
        { _type = tffType }

    member this.TffType 
        with get() = this._type
end

and XmlTermParser() = 
    interface ITermParser with
        member this.Parse xml = 
            let assembly = Assembly.GetExecutingAssembly()
            let types = assembly.DefinedTypes
            let rec parse (el : XmlElement) =               

                match el.Name with
                    | "variable" ->
                        DefaultVariable(el.GetAttribute("name")) :> ITerm

                    | "template" ->
                        DefaultMultiLambda((Seq.cast<IVariable>(Seq.map parse (Seq.cast<XmlElement>(el.SelectSingleNode("params").ChildNodes))), 
                                            parse(el.SelectSingleNode("document") :?> XmlElement))) :> ITerm
                    | _ ->
                        // Нужный класс можно указать в атрибуте datatype xml элемента
                        let customTypeName =  
                            if el.HasAttribute("datatype") then
                                el.GetAttribute("datatype")
                            else
                                el.Name
                
                        let possibleType = Seq.tryFind (fun (t : TypeInfo) -> t.Name = customTypeName) types
                        match possibleType with 
                            // Указанный в атрибуте класс найден
                            | Some customType ->
                            // Необходимо проверить относится ли найденный класс термов к составным
                                let interfaces = customType.ImplementedInterfaces
                                if (Seq.exists ((=) typeof<ICompositeTerm>) interfaces) then
                                    // Даннай функция проверяет есть ли у класса тоф конструктор без аргументов.
                                    // Если такой констуктор существуует, то создаем тоф и создаем с помощью нее терм
                                    let createTerm (tffType : Type) =
                                        let tffConstructor = tffType.GetConstructor(Array.empty<Type>)
                                        if tffConstructor <> null && tffType.GetInterface("ITff") <> null then
                                            let tff = tffConstructor.Invoke(Array.empty<obj>)
                                            let tffInterface = typeof<ITff>
                                            let tffMethod = tffInterface.GetMethod("CreateTerm")
                                            let tffParams = [|(Seq.append 
                                                                        (seq[StringTerm(el.Name) :> ITerm]) 
                                                                        ((Seq.map parse (Seq.cast<XmlElement> el.ChildNodes)))) :?> obj|]
                                            tffMethod.Invoke(tff, tffParams) :?> ITerm
                                            
//                                            tffInterface.InvokeMember("CreateTerm", 
//                                                                 BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.InvokeMethod, 
//                                                                 null, 
//                                                                 tff, 
                                                                 
                                        else
                                            failwith "Tff type should have constructor with no arguments to parse xml composite term"

                                    // Проверяем указан ли в аннотации класс тоф для найденного составного терма.
                                    // Если не указан, то пытаемя найти тоф по названию класса (имя класса + Tff)                                
                                    let attrs = customType.GetCustomAttributes()
                                    let possibleTffAttr = Seq.tryFind (fun (attr : Attribute) -> attr :? TffTypeAttribute) attrs
                                    match possibleTffAttr with
                                        | Some tffAttr ->
                                            let tffType = (tffAttr :?> TffTypeAttribute).TffType
                                            createTerm tffType :?> ITerm
                                        | None ->
                                            let possibleTffType = Seq.tryFind (fun (t : TypeInfo) -> t.Name = customTypeName + "Tff") types
                                            match possibleTffType with
                                                | Some tffType ->
                                                    createTerm(tffType.AsType()) :?> ITerm
                                                | None ->
                                                    failwith ("Can't find tff for " + customType.Name + " class")
                                else
                                    // Если терм является атомом, то пытаемся вызвать его конструктор с параметрами, 
                                    // соответствующими элементам текущего xml элемента
                                    //let argsArray = 
                                    //   Array.ofSeq(Seq.map (fun (childNode : XmlElement) -> childNode.InnerText :> obj) (Seq.cast<XmlElement> el.ChildNodes))
                                    customType.GetConstructor([|typeof<String>|]).Invoke([|el.InnerText :> obj|]) :?> ITerm
                    
                            | None -> 
                                
                                let varRegex = Regex("(<variable name=\"(?<name>\\w+)\"( datatype=\"(?<datatype>\\w+)\")? />)")
                                let subterms =  Regex.Split(el.InnerXml, "(<variable name=\"(?:\\w+)\"(?: datatype=\"\\w+\")? />)")
                                                |> Seq.map (fun substring -> 
                                                                        let strMatch = varRegex.Match(substring)
                                                                        if  strMatch.Success then
                                                                            DefaultVariable(strMatch.Groups.["name"].Value) :> ITerm
                                                                        else
                                                                            StringTerm(substring) :> ITerm)
                                NLSituationDescription(StringTerm(el.Name), subterms) :> ITerm
                                
                
                            
                    
            let xmlDoc = new XmlDocument()
            xmlDoc.LoadXml xml
            parse xmlDoc.DocumentElement
    end

