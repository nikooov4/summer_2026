module Mephi.Cybernetics.Race.HostEnvironments.Tests.SemanticDocumentTests

open System.Xml
open Mephi.Cybernetics.Race.Objects
open Mephi.Cybernetics.Race.Core.Host.Computing
open Mephi.Cybernetics.Race.Core.Host.Definitions
open Mephi.Cybernetics.Race.Core.Host.Computing.TypeSystem

let DocumentTest = 
    let xmlDoc = new XmlDocument()
    xmlDoc.Load("DocumentTemplate.xml")

    let parser = new XmlTermParser()
    let term = (parser :> ITermParser).Parse xmlDoc.InnerXml

    let applicationTff = MultiApplicationTff :> IApplicationTff
    
    let reduction = SimpleReduction(applicationTff) :>IReduction
    let typeVarProvider = TypeVariableProvider()
    let subst = DefaultSubstitution
    let termType = TermType(typeVarProvider, subst)
    let avalDep = [termType; TypeType; TypeTerm] :> seq<TermTypeDependency>
    let contextProvider = ContextProvider()
    let funcTff = FunctionalTypeTff()
    let typeSys = TypeSystem(avalDep, reduction, contextProvider :> IContextProvider, funcTff )

    let ttype = (typeSys :> ITypeSystem).GetTermType term
    System.Console.WriteLine ttype
