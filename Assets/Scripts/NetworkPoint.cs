using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class NetworkPoint
{
    public int2 position;
    public int parentIndex = -1;
    public HashSet<int> childIndices = new();

    public int branchID;
    public int weight = 0;

    public NetworkPoint (int2 position)
    {
        this.position = position;
    }
}
