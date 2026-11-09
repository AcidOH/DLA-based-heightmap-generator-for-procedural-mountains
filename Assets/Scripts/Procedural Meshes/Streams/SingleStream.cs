using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ProceduralMeshes.Streams
{
    public struct SingleStream : IMeshStreams
    {
        [StructLayout(LayoutKind.Sequential)]
        struct Stream0 { // allows for more flexibility than just using Vertex for the stream type
            public float3 position, normal;
            public float4 tangent;
            public float2 texCoord0;
        }

        [NativeDisableContainerSafetyRestriction] // we only do this because we know streams and triangles will never overlap
        NativeArray<Stream0> stream0;
        [NativeDisableContainerSafetyRestriction] // generally, safety warnings should be heeded
        NativeArray<TriangleUInt16> triangles;

        public void Setup(Mesh.MeshData meshData, Bounds bounds, int vertexCount, int indexCount) {
            var descriptor = new NativeArray<VertexAttributeDescriptor>(
                4, Allocator.Temp, NativeArrayOptions.UninitializedMemory
                );
            descriptor[0] = new VertexAttributeDescriptor(dimension: 3);
            descriptor[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, dimension: 3);
            descriptor[2] = new VertexAttributeDescriptor(VertexAttribute.Tangent, dimension: 4);
            descriptor[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, dimension: 2);

            meshData.SetVertexBufferParams(vertexCount, descriptor);
            descriptor.Dispose();

            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(
                0, new SubMeshDescriptor(0, indexCount) {
                    bounds = bounds,
                    vertexCount = vertexCount,
                },
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices // don't do anything until the job has run and the index buffer contains non-arbitrary data
            );  // the | above is the OR operator

            stream0 = meshData.GetVertexData<Stream0>();
            triangles = meshData.GetIndexData<ushort>().Reinterpret<TriangleUInt16>(2);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)] // always insert entire code inline, instead of going for a call (faster, better for optimization)
        public void SetVertex(int index, Vertex vertex) => stream0[index] = new Stream0 {
            position = vertex.position,
            normal = vertex.normal,
            tangent = vertex.tangent,
            texCoord0 = vertex.texCoord0,
        };

        public void SetTriangle (int index, int3 triangle) => triangles[index] = triangle;
    }

}
