using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;

public class PointNetwork
{// starting at zero, the indices of the nodes correspond to the order they were created in
 // stored with this index, is the position
 // can access the node based on its position in the grid
 // can access the indices of its neighbours
 // can get the positions of its neighbours from the set of the neighbour's indices
    
    
    private List<NetworkPoint> _points = new(); // full network
    private Dictionary<int2, int> _pointAtPosition = new(); // <position, index>

    public IReadOnlyList<NetworkPoint> Points => _points; // network can only be changed using methods defined here

    public void AddInitialNode(int2 pos)
    {
        _points.Add(new NetworkPoint(pos));
        _pointAtPosition.Add(pos, 0); 
    }

    public int AddNode(int2 pos, int existingIndex)
    { // adds new node and connects it to existing nodes, returns -1 if can't be added
        if (_pointAtPosition.ContainsKey(pos)) // if position is already occupied
            return -1;

        if (existingIndex < 0 || existingIndex >= _points.Count) // if index refers to an existing node
            return -1;

        int newIndex = _points.Count; // nodes are only appended, list count is the new node index

        _points.Add(new NetworkPoint(pos)); // create node
        _pointAtPosition.Add(pos, newIndex);

        Connect(existingIndex, newIndex);

        return newIndex;
    }

    private void Connect(int parentIndex, int childIndex) 
    {
        _points[parentIndex].childIndices.Add(childIndex);
        _points[childIndex].parentIndex = parentIndex;
    }

    public bool ContainsPoint(int2 pos) 
    {
        return _pointAtPosition.ContainsKey(pos);
    }
    
    public bool TryGetPoint(int2 pos, out int pointIndex) 
    {
        return _pointAtPosition.TryGetValue(pos, out pointIndex);
    }

    public void ClearPoints() 
    {
        _points.Clear();
        _pointAtPosition.Clear();
    }

    public void ApplyStrahlerWeights() {
        CalculateStrahlerWeight(0);
    }

    public void ApplyShreveWeights() {
        CalculateShreveWeight(0);
    }

    private int CalculateStrahlerWeight(int index)
    {   // the point is assigned a weight one higher than the largest weight of its children,
        // only if there are at least two children with that highest weight.
        // otherwise, it is assigned the same weight as the largest weighted child.

        NetworkPoint point = _points[index];

        if (point.childIndices.Count == 0) // end of the branch if no children
            return point.weight = 1;

        int largest = 0; // largest weight amongst the children
        int largestCount = 0; // how many of the children share the largest weight

        foreach (int childIndex in point.childIndices) {
            if (_points[childIndex].weight == 0) {

                int childWeight = CalculateStrahlerWeight(childIndex); // be careful here!! 

                if (childWeight > largest) {
                    largest = childWeight;
                    largestCount = 1;
                }
                else if (childWeight == largest) {
                    largestCount++;
                }
            } 
        }

        return point.weight = largestCount >= 2 ? largest + 1 : largest;
    }

    private int CalculateShreveWeight(int index)
    {
        NetworkPoint point = _points[index];

        if (point.childIndices.Count == 0)
            return point.weight = 1;

        int weight = 0;

        foreach (int childIndex in point.childIndices) {
            if (_points[childIndex].weight == 0) {
                weight += CalculateShreveWeight(childIndex);
            }
        }

        return point.weight = weight;
    }

    public int2 UpscaleByFactorTwo(int width, int height) 
    {
        int originalCount = _points.Count;
        int[] oldParents = new int[originalCount]; // empty array for the old parents, populated later

        _pointAtPosition.Clear();

        for (int i = 0; i < originalCount; i++) {
            oldParents[i] = _points[i].parentIndex; // storing the old parent in the oldParents array

            _points[i].position *= 2;
            _points[i].parentIndex = -1; // int, reset connections
            _points[i].childIndices.Clear(); // list, removes connections

            _pointAtPosition.Add(_points[i].position, i); // update pos, the index hasn't changed
        }

        for (int childIndex = 0; childIndex < originalCount; childIndex++) { // one midpoint child added for every original point
            int parentIndex = oldParents[childIndex]; // original parent of this point

            if (parentIndex == -1) // the origin's original parent index (has no parent)
                continue;

            int2 midpointPosition = (_points[parentIndex].position + _points[childIndex].position) / 2;
            int midpointIndex = _points.Count; // adding the midpoints one at a time to _points

            NetworkPoint midpoint = new NetworkPoint(midpointPosition)
            {
                parentIndex = parentIndex, // connect midpoint to the original parent
                //branchID = _points[childIndex].branchID // belongs to same branch as original child
            };

            midpoint.childIndices.Add(childIndex); // connect to the original child index

            _points.Add(midpoint);
            _pointAtPosition.Add(midpointPosition, midpointIndex);

            _points[parentIndex].childIndices.Add(midpointIndex); // add the midpoint as the child of the original parent
            _points[childIndex].parentIndex = midpointIndex; // add the midpoint as the parent of the original child
        }

        return new int2(width * 2, height * 2);
    }

    private void UpscaleScramble()
    {
    
    }
}
