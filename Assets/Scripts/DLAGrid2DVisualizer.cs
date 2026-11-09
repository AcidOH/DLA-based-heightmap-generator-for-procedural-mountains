using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DLAGridDisplay : MonoBehaviour
{
    [SerializeField]
    private Renderer targetRenderer;

    private Texture2D gridTexture;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
    }

    public void DisplayGrid(float[,] grid) 
    {
        int height  = grid.GetLength(0);
        int width = grid.GetLength(1);

        gridTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        gridTexture.filterMode = FilterMode.Point;
        gridTexture.wrapMode = TextureWrapMode.Clamp;

        WriteGridToTexture(grid);
        AssignTexture();
    }

    private void WriteGridToTexture (float[,] grid) 
    {
        int height = grid.GetLength(0);
        int width = grid.GetLength(1);

        Color[] pixels = new Color[height * width];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                float value = Mathf.Clamp01(grid[y, x]);
                pixels[y * width + x] = Color.white * value;
            }
        }

        gridTexture.SetPixels(pixels);
        gridTexture.Apply();
    }

    private void AssignTexture() 
    {
        Material material = targetRenderer.material;

        material.mainTexture = gridTexture;
        material.SetTexture("_BaseMap", gridTexture);
    }
}
