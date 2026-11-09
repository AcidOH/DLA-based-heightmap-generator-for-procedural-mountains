using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using System;

public class DLAHeightTextureGenerator : MonoBehaviour
{
    // using (x, y) coordinate convention (because of vectors)

    [SerializeField, Min(1)] private uint seed;
    private static Random rnd;

    private DLAGridDisplay gridDisplay;

    private float[,] grid;

    [SerializeField, Min(4)]
    private int maximumGridDimension = 4;

    private int initialWidth = 4, initialHeight = 4;
    private int currentWidth, currentHeight;

    [SerializeField]
    private int2 originPos = new int2(0, 0);

    [SerializeField, Range(0, 1)]
    private float density = 0.5f; // percent filled (must be <= 1)

    [SerializeField, Min(16)]
    private int maxSteps = 512;

    [SerializeField]
    private bool strahlerMethod;
    
    [SerializeField]
    private bool smoothFalloff;

    private static readonly int2[] directions = new int2[4] { new int2(0, 1), new int2(1, 0), new int2(0, -1), new int2(-1, 0)};
    private PointNetwork network = new PointNetwork(); // record of points that make up the aggregate network


    private void Awake()
    {
        gridDisplay = GetComponent<DLAGridDisplay>();
    }

    private void Start()
    {
        GenerateAndDisplayGrid();
    }

    private void OnValidate() // regenerates grid if inspector parameters are changed
    {
        if (Application.isPlaying && gridDisplay != null)
            GenerateAndDisplayGrid();
    }



    private void GenerateAndDisplayGrid()
    {
        rnd = new Random(seed);
        network.ClearPoints();

        originPos.x = Mathf.Clamp(originPos.x, 0, initialWidth - 1);
        originPos.y = Mathf.Clamp(originPos.y, 0, initialHeight - 1);

        grid = new float[initialWidth, initialHeight]; // automatically populates with 0f

        currentWidth = initialWidth; currentHeight = initialHeight;

        network.AddInitialNode(originPos);

        FormAggregate();

        while (currentWidth <= maximumGridDimension && currentHeight <= maximumGridDimension) {
            int2 newDimensions = network.UpscaleByFactorTwo(currentWidth, currentHeight);
            currentWidth = newDimensions.x; currentHeight = newDimensions.y;
            FormAggregate();
        }

        if (strahlerMethod) {
            network.ApplyStrahlerWeights();
        }
        else {
            network.ApplyShreveWeights();
        }

        grid = new float[currentWidth, currentHeight];
        PopulateGrid();

        gridDisplay.DisplayGrid(grid);
    }
     
    private void PopulateGrid() // where [0,0] is on bottom left
    {
        float intensity = 0f;

        foreach (NetworkPoint node in network.Points) {

            if (smoothFalloff) {
                intensity = 1f - (1f / (1f + (float)node.weight));
            }
            else {
                intensity = (float)node.weight / (float)network.Points[0].weight;
            }

            grid[node.position.x, node.position.y] = intensity; // weight normalized between 0 and 1
        }  
    }

    private int2 GeneratePos() 
    {
        if (network.Points.Count >= currentWidth * currentHeight)
            throw new InvalidOperationException("Grid is full.");

        int2 newPos = new int2(rnd.NextInt(0, currentWidth), rnd.NextInt(0, currentHeight));

        while (network.ContainsPoint(newPos)) {
            newPos = new int2(rnd.NextInt(0, currentWidth), rnd.NextInt(0, currentHeight));
        }
        
        return newPos;
    }

    private int2 SelectRandomDirection() 
    {
        return directions[rnd.NextInt(4)];
    }

    private void AddParticle() 
    {
        int2 initialPos = GeneratePos();
        int2 currentPos = initialPos;

        for (int steps = 0; steps < maxSteps; steps++) {
            if (!CheckWithinBounds(currentPos)) { // reset postion and steps
                currentPos = initialPos;
                steps = 0;
            }

            if (CheckForNeigbours(currentPos, out int neighbourIndex)) {
                network.AddNode(currentPos, neighbourIndex);
                return;
            }

            int2 newPos = currentPos + SelectRandomDirection();

            if (newPos.x < currentWidth && newPos.y < currentHeight) {
                currentPos = newPos;
            }
        }
    }

    private bool CheckWithinBounds(int2 pos)
    {
        if (pos.x < 0 || pos.x > currentWidth - 1 || pos.y < 0 || pos.y > currentHeight -1 )
            return false;
        return true;
    }

    private bool CheckForNeigbours(int2 pos, out int selectedIndex) 
    {
        List<int> neighbourIndices = new();

        foreach (int2 direction in directions) {
            if (network.TryGetPoint(pos + direction, out int pointIndex))
                neighbourIndices.Add(pointIndex);
        }

        if (neighbourIndices.Count > 0) {
            selectedIndex = neighbourIndices[rnd.NextInt(0, neighbourIndices.Count)];
            return true;
        }
        selectedIndex = -1;
        return false;
        
    }

    private void FormAggregate() 
    {
        int maxParticles = (int)Math.Round(density * currentWidth * currentHeight);

        while (network.Points.Count < maxParticles) {
            AddParticle();
        }
    }
}
