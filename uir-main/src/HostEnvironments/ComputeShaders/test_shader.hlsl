struct LambdaTreeNode {
	int index;
	int prevIndex;
	int nodeType;
	int leftIndex;
	int rightIndex;
};

StructuredBuffer<LambdaTreeNode> Data : register(t0);
StructuredBuffer<LambdaTreeNode> Data1 : register(t0);
RWStructuredBuffer<LambdaTreeNode> Output : register(u0);

groupshared bool oneReductionStepWasDone = true;
groupshared uint dataLength, dataElementSize;
groupshared LambdaTreeNode tree[500];
groupshared LambdaTreeNode lastNode = { -1, 0, 0, 0, 1 };

#define groupDim_x 1024

void copyNode(int copyIndex, int pasteIndex, RWStructuredBuffer<LambdaTreeNode> output) {
	output[pasteIndex].index = output[copyIndex].index;
	output[pasteIndex].nodeType = output[copyIndex].nodeType;
	output[pasteIndex].leftIndex = output[copyIndex].leftIndex;
	output[pasteIndex].rightIndex = output[copyIndex].rightIndex;
}

void findFuncAbstractionAndReduce(LambdaTreeNode node) {
	LambdaTreeNode possibleAbstractionNode = tree[node.leftIndex];
	bool nodeWasFound = false;

	while (!nodeWasFound) {
		if (possibleAbstractionNode.nodeType == 1) {
			tree[possibleAbstractionNode.leftIndex].leftIndex = node.rightIndex;
			tree[node.leftIndex].nodeType = -1;
			tree[node.leftIndex].leftIndex = possibleAbstractionNode.rightIndex;
			tree[possibleAbstractionNode.index].nodeType = -1;
			tree[possibleAbstractionNode.index].leftIndex = possibleAbstractionNode.rightIndex;
			tree[node.index].leftIndex = possibleAbstractionNode.rightIndex;
			tree[node.index].nodeType = -1;

			if (node.prevIndex != -1) {
				LambdaTreeNode topNode = tree[node.prevIndex];
				//bool topWasFound = false;
				//while (!topWasFound) {
					//if (topNode.nodeType >= 0) {
				//topWasFound = true;
				tree[possibleAbstractionNode.rightIndex].prevIndex = topNode.index;
				if (topNode.leftIndex == node.index)
					tree[node.prevIndex].leftIndex = possibleAbstractionNode.rightIndex;
				else
					tree[node.prevIndex].rightIndex = possibleAbstractionNode.rightIndex;
					//}
					//else {
					//	topNode = tree[topNode.leftIndex];
					//}
				//}
			}
			else tree[possibleAbstractionNode.rightIndex].prevIndex = -1;
			oneReductionStepWasDone = true;
			nodeWasFound = true;
		}
		else if ((possibleAbstractionNode.nodeType == 2 && possibleAbstractionNode.leftIndex != -1) || possibleAbstractionNode.nodeType == -1 ) {
			possibleAbstractionNode = tree[possibleAbstractionNode.leftIndex];
		}
		else nodeWasFound = true;
	}	
}

[numthreads(groupDim_x, 1, 1)]
void reduce(uint3 threadIdx : SV_GroupThreadID, uint3 groupIdx : SV_GroupID)
{
	if (threadIdx.x == 0) Data.GetDimensions(dataLength, dataElementSize);

	GroupMemoryBarrierWithGroupSync();	

	if (threadIdx.x < dataLength) {
		tree[threadIdx.x] = Data[threadIdx.x];
	}

	GroupMemoryBarrierWithGroupSync();

	while (oneReductionStepWasDone) {
		oneReductionStepWasDone = false;
		if (threadIdx.x < dataLength) {
			LambdaTreeNode cur = tree[threadIdx.x];
			if (cur.nodeType == 0) findFuncAbstractionAndReduce(cur);
		}
		GroupMemoryBarrierWithGroupSync();
	}	

	if (threadIdx.x == 0) {
		LambdaTreeNode node = { -1, 0, 0, 0, 0 };
		tree[dataLength] = lastNode;
	}

	GroupMemoryBarrierWithGroupSync();

	if (threadIdx.x <= dataLength) {
		Output[threadIdx.x] = tree[threadIdx.x];
	}
}