using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct SquareGrid : IMeshGenerator // provide drawing instructions for the mesh
    {
        public int Resolution { get; set; }
        public int VertexCount => 4 * Resolution * Resolution;
        public int IndexCount => 6 * Resolution * Resolution; // 6 = two triangles
        public int JobLength => Resolution;
        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams { // could leave this empty if wanted an empty mesh
            int vi = 4 * Resolution * z, ti = 2 * Resolution * z; // vertex and triangle indices/offsets

            for (int x = 0; x < Resolution; x++, vi += 4, ti += 2) {
                //var coordinates = float4(x, x + 1f, z, z + 1f); // smaller addition to leave gaps b/w quads (touching = 1)
                // grid is fixed size, centered about origin
                var xCoordinates = float2(x, x + 1f) / Resolution - 0.5f;
                var zCoordinates = float2(z, z + 1f) / Resolution - 0.5f;

                var vertex = new Vertex();
                vertex.normal.y = 1f; // normal points up
                vertex.tangent.xw = float2(1f, -1f); // swizzle operation

                vertex.position.x = xCoordinates.x;
                vertex.position.z = zCoordinates.x;
                streams.SetVertex(vi + 0, vertex);

                vertex.position.x = xCoordinates.y;
                vertex.texCoord0 = float2(1f, 0f);
                streams.SetVertex(vi + 1, vertex);


                vertex.position.x = xCoordinates.x;
                vertex.position.z = zCoordinates.y;
                vertex.texCoord0 = float2(0f, 1f);
                streams.SetVertex(vi + 2, vertex);

                vertex.position.x = xCoordinates.y;
                vertex.texCoord0 = 1f;
                streams.SetVertex(vi + 3, vertex);


                streams.SetTriangle(ti + 0, vi + int3(0, 2, 1));
                streams.SetTriangle(ti + 1, vi + int3(1, 2, 3));
            }
        }
        //public Bounds Bounds => new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));
        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0f, 1f)); // fixed bounds, centered
    }
}
