using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.Mathematics.math;

namespace ProceduralMeshes.Generators
{
    public struct SharedSquareGrid : IMeshGenerator // provide drawing instructions for the mesh
    {
        public int Resolution { get; set; }
        public int VertexCount => (Resolution + 1) * (Resolution + 1); // share vertices
        public int IndexCount => 6 * Resolution * Resolution; // 6 = two triangles
        public int JobLength => Resolution + 1; // rows of vertices instead of rows of quads
        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams { // could leave this empty if wanted an empty mesh
            int vi = (Resolution + 1) * z, ti = 2 * Resolution * (z - 1);

            var vertex = new Vertex();
            vertex.normal.y = 1f;
            vertex.tangent.xw = float2(1f, -1f);

            vertex.position.x = -0.5f;
            vertex.position.z = (float)z / Resolution - 0.5f;
            vertex.texCoord0.y = (float)z / Resolution;
            streams.SetVertex(vi, vertex);
            vi += 1;

            for (int x = 1; x <= Resolution; x++, vi++, ti +=2) {
                vertex.position.x = (float)x / Resolution - 0.5f;
                vertex.texCoord0.x = (float)x / Resolution;
                streams.SetVertex(vi, vertex);

                if (z > 0) { // no quads below bottom row
                    streams.SetTriangle(ti + 0, vi + int3(-Resolution - 2, -1, -Resolution - 1));
                    streams.SetTriangle(ti + 1, vi + int3(-Resolution - 1, -1, 0));
                }
            }
        }
        //public Bounds Bounds => new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));
        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0f, 1f)); // fixed bounds, centered
    }
}
