using System.Collections.Generic;
using UnityEngine;

public static class Parameters
{
    public static int numberOfChairs = 4;
    public static int numberOfTables = 1;

    public static int numberOfGroups = 2;

    public static float floorSizeX = 7.0f; // in meters
    public static float floorSizeZ = 9.0f; // in meters

    // weights                                          ORIGINAL VALUES
    // public static float w_clearanceViolation = 6.0f;    // 2    // 4        2 < x < 10
    // public static float w_circulation = 5.0f;           // 1    
    // public static float w_pairwiseDistance = 2.0f;      // 2    // 3        2 < x
    // public static float w_pairwiseAngle = 5.0f;         // 2
    // public static float w_conversationDistance = 2.0f;  // 2    // 4        3 < x
    // public static float w_conversationAngle = 6.0f;     // 2    // 7        6 < x
    // public static float w_balance = 2.0f;               // 1.5              1 < x
    // public static float w_alignment = 2.5f;             // 2.5              1 < x
    // public static float w_wallAlignment = 2.5f;         // 2.5              1 < x
    // public static float w_symmetry = 1.0f;              // 1
    // public static float w_emphasis = 2.0f;              // 4                1 < x
    
    public static float w_clearanceViolation = 3.0f;    // 2        // 2
    public static float w_circulation = 5.0f;           // 1        // 5
    public static float w_pairwiseDistance = 3.0f;      // 2        // 2
    public static float w_pairwiseAngle = 5.0f;         // 2        // 5
    public static float w_conversationDistance = 2.0f;  // 2        // 2
    public static float w_conversationAngle = 6.0f;     // 2        // 2
    public static float w_balance = 1.0f;               // 1.5      // 0.5
    public static float w_alignment = 1.0f;             // 2.5      // 1
    public static float w_wallAlignment = 1.0f;         // 2.5      // 1
    public static float w_symmetry = 1.0f;              // 1        // 1
    public static float w_emphasis = 2.0f;              // 4        // 1
    
    // for modifying layouts
    public static float position = 0.2f;    // 0.2                          0.1 < x < 0.3
    public static float rotation = 5f;     // 20
    
    // testing
    // public static List<float> xPositions = new List<float> { -1.0f, 0.0f,  0.0f,   0.0f,   0.0f,  0.0f };
    // public static List<float> zPositions = new List<float> { -2.0f, 0.0f,  0.0f,   0.0f,   0.0f,  0.0f };
    // public static List<float> rotations = new List<float>  { 0.0f,  90.0f, 180.0f, -90.0f, 90.0f, 0.0f };
    // public static List<int> fIndex = new List<int>         { 4,     0,     2,      3,      4,     0, 2, 3 }; // what item
    // public static List<int> modifications = new List<int>  { 0,     1,     1,      1,      1,     0, 0, 0}; // translation or rotation

    public static List<float> xPositions = new List<float> { 0.0f, -0.1f, 0.0f, 0.1f, 0.0f, 0.0f, 0.0f };
    public static List<float> zPositions = new List<float> { 0.0f, 1.8f, -1.0f, -1.8f, 0.0f, 0.0f, 0.0f };
    public static List<float> rotations = new List<float>  { 180.0f, 0.0f, 0.0f, 0.0f, -10.0f, 10.0f, -10.0f, 10.0f, -10.0f, 10.0f, -10.0f, 10.0f, -10.0f, 10.0f };
    public static List<int> fIndex = new List<int>         { 3, 0, 4, 3, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 }; // what item
    public static List<int> modifications = new List<int>  { 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }; // translation or rotation

    
    
    // iterations of MCMC 2000
    public static int iterations = 2000;
    
    // for cycling through furniture to modify
    public static int roundsPerObject = 10;

    // Max distance to snap position at the end
    public static float maxDist = 0.5f;



}
