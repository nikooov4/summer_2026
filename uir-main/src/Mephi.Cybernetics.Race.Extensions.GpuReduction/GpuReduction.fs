module Mephi.Cybernetics.Race.HostEnvironments.Reduction.Gpu

open Mephi.Cybernetics.Race.Core.Host
open Mephi.Cybernetics.Race.Core.Host.Definitions
open Mephi.Cybernetics.Race.Core.Host.Computing
open Mephi.Cybernetics.Race.Core.Host.Definitions.Tff

open System
open System.Collections.Generic
open SharpDX
open SharpDX.Direct3D
open SharpDX.Direct3D11
open SharpDX.DXGI
open SharpDX.D3DCompiler

type Device = SharpDX.Direct3D11.Device
type Buffer = SharpDX.Direct3D11.Buffer

// -1 - none 
//  0 - application
//  1 - functional abstraction
//  2 - variable
[<Struct>]
type LambdaTreeNode(index:int, prevIndex:int, nodeType:int, leftIndex:int, rightIndex:int) =
    override this.ToString() = "Index: " + index.ToString() + ", prev: " + prevIndex.ToString() +  ", type: " + nodeType.ToString() + ", left: " + leftIndex.ToString() + ", right: " + rightIndex.ToString()
    member this.Index = index
    member this.PrevIndex = prevIndex
    member this.NodeType = nodeType
    member this.LeftIndex = leftIndex
    member this.RightIndex = rightIndex

    type tree =
    | T of string * tree list
 
let prefMid = seq { yield "├─"; while true do yield "│ " }
let prefEnd = seq { yield "└─"; while true do yield "  " }
let prefNone = seq { while true do yield "" }
 
let c2 x y = Seq.map2 (fun u v -> String.concat "" [u; v]) x y
 
let rec visualize (T(label, children)) pre =
    seq {
        yield (Seq.head pre) + label
        if children <> [] then
            let preRest = Seq.skip 1 pre
            let last = Seq.last (List.toSeq children)
            for e in children do
                if e = last then yield! visualize e (c2 preRest prefEnd)
                else yield! visualize e (c2 preRest prefMid)
    }
 

let rec nodeDictToTree (dict : Dictionary<int,LambdaTreeNode>) index =
    let el = dict.Item index
    let list = 
        match el.NodeType with
            | 2 ->
                if el.LeftIndex = -1 then 
                    []
                else
                    [nodeDictToTree dict el.LeftIndex]
            | _ ->
                [nodeDictToTree dict el.RightIndex ; nodeDictToTree dict el.LeftIndex]
    T (el.Index.ToString() + "-" + el.NodeType.ToString(), list)

let rec termToNode (term : ITerm) (lastIndexRef : Ref<int>) (prevIndex:int) (nodeList : NetList<LambdaTreeNode>) (dict : Dictionary<IVariable, int>) =
    match term with
        | :? IApplicationTerm as application ->
            let left = application.LeftTerm
            let right = application.RightTerm
            let newIndex = lastIndexRef.Value + 1
            lastIndexRef := newIndex
            let leftNode = termToNode left lastIndexRef newIndex  nodeList dict
            let rightNode = termToNode right lastIndexRef newIndex nodeList dict
            let node = LambdaTreeNode(newIndex, prevIndex, 0, (leftNode :> LambdaTreeNode).Index, rightNode.Index)
            nodeList.Add node
            node
        
        | :? IFunctionalAbstraction as funcAbstraction ->
            let var = funcAbstraction.GetBoundVariable
            let body = funcAbstraction.GetBasicBody
            let newIndex = lastIndexRef.Value + 1
            lastIndexRef := newIndex
            let leftNode = termToNode var lastIndexRef newIndex nodeList dict
            let rightNode = termToNode body lastIndexRef newIndex nodeList dict
            
            let node = LambdaTreeNode(newIndex, prevIndex, 1, leftNode.Index, rightNode.Index)

            nodeList.Add node
            node
        
        | :? IVariable as var ->
            
            if not( dict.ContainsKey(var) ) then 
                let newIndex = lastIndexRef.Value + 1
                lastIndexRef := newIndex
                dict.Add(var, newIndex)
            
                let node = LambdaTreeNode(dict.Item(var), prevIndex, 2, -1, -1)
                nodeList.Add node
                node
            else
                nodeList.Find(fun x -> x.Index = dict.Item(var))
        
        | :? ITermAlias as alias ->
            termToNode alias.GetTerm lastIndexRef prevIndex nodeList dict
        
        | _ ->
            failwith "Can't convert term to tree to reduce on GPU"

let nodeListToArray (nodeList : NetList<LambdaTreeNode>) =
    let (arr : LambdaTreeNode array) = Array.zeroCreate nodeList.Count
    nodeList.ForEach(fun el -> arr.[el.Index] <- el)
//    for x in arr do
//        Console.WriteLine x
    arr

let termToNodeArray term = 
    let list = new NetList<LambdaTreeNode>()
    let dict = new Dictionary<IVariable, int>()
    let index = ref -1
    let node = termToNode term index -1 list dict
    nodeListToArray list

let rec ReadNodeList (dataStream : DataStream) (dict : Dictionary<int, LambdaTreeNode>) min =
    let node = dataStream.Read<LambdaTreeNode>()
    //Console.WriteLine node
    if node.Index <> -1 then
        if node.NodeType >= -1 then 
            dict.Add(node.Index, node)
            ReadNodeList dataStream dict (if node.Index < min then node.Index else min)
        else
            ReadNodeList dataStream dict min
    else
        (dict, min)

let rec NodeDictToTerm (node : LambdaTreeNode) (dict : Dictionary<int, LambdaTreeNode>) =

    //Add enum       
    match node.NodeType with
        | -1 ->
            NodeDictToTerm (dict.Item node.LeftIndex) dict
        | 0 ->
            let left = NodeDictToTerm (dict.Item node.LeftIndex) dict
            let right = NodeDictToTerm (dict.Item node.RightIndex) dict
            SingleApplicationTerm(left, right) :> ITerm

        | 1 ->
            let var = NodeDictToTerm (dict.Item node.LeftIndex) dict
            let body = NodeDictToTerm (dict.Item node.RightIndex) dict

            if var :? IVariable then
                DefaultSimpleLambda(var :?> IVariable, body) :> ITerm
            else 
                failwith "IVariable was expected"
        | 2 ->
            if node.LeftIndex = -1 then
                DefaultVariable("x" + node.Index.ToString()) :> ITerm
            else
                NodeDictToTerm (dict.Item node.LeftIndex) dict

        | _ ->
            failwith "Unexpected type code"
            
                
            
type GpuReduction() =
    let device = new Device(DriverType.Hardware, DeviceCreationFlags.Debug)
    let context = device.ImmediateContext
    let outBuf = new Buffer(device, sizeof<LambdaTreeNode> * 500, ResourceUsage.Default, BindFlags.UnorderedAccess,
                                CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, 20)

    let cpuBuf = new Buffer(device, sizeof<LambdaTreeNode> * 500, ResourceUsage.Staging, BindFlags.None, CpuAccessFlags.Read, ResourceOptionFlags.None, 20)
    let bytecode = ShaderBytecode.CompileFromFile("ComputeShaders\\test_shader.hlsl", "reduce", "cs_5_0", 
                                                        ShaderFlags.Debug ||| ShaderFlags.SkipOptimization ||| ShaderFlags.PreferFlowControl)
    let computeShader = new ComputeShader(device, bytecode.Bytecode.Data)
    member this.Evaluate termToEvaluate : ITerm =
        
        
        let nodes = termToNodeArray termToEvaluate
        
        let mutable result = new DataStream(sizeof<LambdaTreeNode> * 50, true, true)

        let gpuData = new DataStream(sizeof<LambdaTreeNode> * nodes.Length, true, true)
       
        ignore (Array.map (fun el -> gpuData.Write<LambdaTreeNode> el) nodes)
        gpuData.Position <- (int64)0
       
        let buffer = new Buffer(device, gpuData, sizeof<LambdaTreeNode> * nodes.Length, ResourceUsage.Default, BindFlags.ShaderResource, 
                                CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, 20)
        
        
        let elementsView = new ShaderResourceView(device, buffer)

        let mutable uavd = new UnorderedAccessViewDescription()
        uavd.Format <- Format.Unknown
        uavd.Dimension <- UnorderedAccessViewDimension.Buffer
        uavd.Buffer.ElementCount <- 500

        let unorderedAccessView = new UnorderedAccessView(device, outBuf, uavd)
        
        
        

        context.ComputeShader.Set computeShader
        context.ComputeShader.SetShaderResource(0, elementsView)
        context.ComputeShader.SetUnorderedAccessView(0, unorderedAccessView)
        
        context.Dispatch(1, 1, 1)
        
        context.CopyResource(outBuf, cpuBuf)
        ignore (context.MapSubresource(cpuBuf, MapMode.Read, MapFlags.None, &result))
        
        let nodeDict, firstIndex = ReadNodeList result (new Dictionary<int, LambdaTreeNode>()) Microsoft.FSharp.Core.int.MaxValue
        
        let term = NodeDictToTerm (nodeDict.Item firstIndex) nodeDict
        visualize (nodeDictToTree nodeDict firstIndex) prefNone
        |> Seq.iter (printfn "%s")
        term

type GpuReduction1() =
    member this.Evaluate termToEvaluate : ITerm =
        
        let device = new Device(DriverType.Hardware, DeviceCreationFlags.Debug)
        let context = device.ImmediateContext
        let nodes = termToNodeArray termToEvaluate
        
        let mutable result = new DataStream(sizeof<LambdaTreeNode> * 50, true, true)

        let stopWatch = System.Diagnostics.Stopwatch.StartNew()
        let gpuData = new DataStream(sizeof<LambdaTreeNode> * nodes.Length, true, true)
        stopWatch.Stop()
        printfn "Время создания массива: %f" (stopWatch.Elapsed.TotalMilliseconds) 

        stopWatch.Restart()
        ignore (Array.map (fun el -> gpuData.Write<LambdaTreeNode> el) nodes)
        gpuData.Position <- (int64)0
        stopWatch.Stop()
        printfn "Время записи массива: %f" (stopWatch.Elapsed.TotalMilliseconds)
        
        stopWatch.Restart()
        let buffer = new Buffer(device, gpuData, sizeof<LambdaTreeNode> * nodes.Length, ResourceUsage.Default, BindFlags.ShaderResource, 
                                CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, 20)
        let outBuf = new Buffer(device, sizeof<LambdaTreeNode> * 500, ResourceUsage.Default, BindFlags.UnorderedAccess,
                                CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, 20)

        let cpuBuf = new Buffer(device, sizeof<LambdaTreeNode> * 500, ResourceUsage.Staging, BindFlags.None, CpuAccessFlags.Read, ResourceOptionFlags.None, 20)
        
        let elementsView = new ShaderResourceView(device, buffer)

        let mutable uavd = new UnorderedAccessViewDescription()
        uavd.Format <- Format.Unknown
        uavd.Dimension <- UnorderedAccessViewDimension.Buffer
        uavd.Buffer.ElementCount <- 500

        let unorderedAccessView = new UnorderedAccessView(device, outBuf, uavd)
        stopWatch.Stop()
        printfn "Создание буферов и вьюх: %f" (stopWatch.Elapsed.TotalMilliseconds)

        stopWatch.Restart()
        let bytecode = ShaderBytecode.CompileFromFile("C:\\Users\\Vas\\Documents\\Visual Studio 2013\\Projects\\race\\src\\HostEnvironments\\ComputeShaders\\test_shader.hlsl", "reduce", "cs_5_0", 
                                                        ShaderFlags.Debug ||| ShaderFlags.SkipOptimization ||| ShaderFlags.PreferFlowControl)
        let computeShader = new ComputeShader(device, bytecode.Bytecode.Data)
        

        context.ComputeShader.Set computeShader
        context.ComputeShader.SetShaderResource(0, elementsView)
        context.ComputeShader.SetUnorderedAccessView(0, unorderedAccessView)
        stopWatch.Stop()
        printfn "Компилирование и создание шейдера: %f" (stopWatch.Elapsed.TotalMilliseconds)

        stopWatch.Restart()
        context.Dispatch(1, 1, 1)
        stopWatch.Stop()
        printfn "Время выполнения: %f" (stopWatch.Elapsed.TotalMilliseconds) 
    
        stopWatch.Restart()
        context.CopyResource(outBuf, cpuBuf)
        ignore (context.MapSubresource(cpuBuf, MapMode.Read, MapFlags.None, &result))
        stopWatch.Stop()
        printfn "Копирование и маппинг: %f" (stopWatch.Elapsed.TotalMilliseconds) 

        stopWatch.Restart()
        let nodeDict, firstIndex = ReadNodeList result (new Dictionary<int, LambdaTreeNode>()) Microsoft.FSharp.Core.int.MaxValue
        stopWatch.Stop()
        printfn "Создание словаря: %f" (stopWatch.Elapsed.TotalMilliseconds) 

        stopWatch.Restart()
        let term = NodeDictToTerm (nodeDict.Item firstIndex) nodeDict
        stopWatch.Stop()
        printfn "Создание терма: %f" (stopWatch.Elapsed.TotalMilliseconds) 
        term