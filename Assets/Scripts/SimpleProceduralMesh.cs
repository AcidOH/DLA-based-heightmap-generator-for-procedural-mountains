using UnityEngine;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]

public class SimpleProceduralMesh : MonoBehaviour
{
    private void OnEnable()
    {
        // define mesh
        var mesh = new Mesh {
            name = "Procedural Mesh"
        };

        // define vertices (only first 3 for tris)
        mesh.vertices = new Vector3[] {
            Vector3.zero, Vector3.right, Vector3.up, new Vector3(1f, 1f)
        };

        // define triangles (only first 3 for tris)
        mesh.triangles = new int[] {
            //0, 1, 2 // visible from Z- direction
            0, 2, 1, 1, 2, 3 // visible from Z+ direction, could also swap vertices instead of indices
            };

        // assign normals (only first 3 for tris)
        mesh.normals = new Vector3[] {
            Vector3.back, Vector3.back, Vector3.back, Vector3.back //surface normal from interpolating vertex normals
        };

        // assign an array to texture's uv property (only first 3 for tris)
        mesh.uv = new Vector2[] {
            Vector2.zero, Vector2.right, Vector2.up, Vector2.one
        };

        // convert mesh coordinates from texture to world space by assigning tangents (only first 3 for tris)
        mesh.tangents = new Vector4[] {
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f),
            new Vector4(1f, 0f, 0f, -1f)
        };

        GetComponent<MeshFilter>().mesh = mesh;

    }
}

