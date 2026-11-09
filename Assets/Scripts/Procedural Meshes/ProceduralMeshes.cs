using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ProceduralMeshes
{
    public interface IMeshStreams // Set up vertex and index buffers, hide streams and data formats
    {
        void Setup(Mesh.MeshData data, Bounds bounds, int vertexCount, int indexCount); // initialize mesh data
        void SetVertex(int index, Vertex data); // copy vertex to mesh's vertex buffer (regardless of streams amount and data format)
        void SetTriangle(int index, int3 triangle);
    }
}
