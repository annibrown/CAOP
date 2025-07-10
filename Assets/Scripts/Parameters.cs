using System.Collections.Generic;
using UnityEngine;

public static class Parameters
{
    public static int numberOfChairs = 4;
    public static int numberOfTables = 1;

    public static float floorSizeX = UnitConverter.MetersToInches(5.9f); // in inches
    public static float floorSizeZ = UnitConverter.MetersToInches(7.4f); // in inches

    // weights                                          ORIGINAL VALUES
    public static float w_clearanceViolation = 2.0f;    // 2
    public static float w_circulation = 5.0f;           // 1
    public static float w_pairwiseDistance = 2.0f;      // 2
    public static float w_pairwiseAngle = 5.0f;         // 2
    public static float w_conversationDistance = 2.0f;  // 2
    public static float w_conversationAngle = 6.0f;     // 2
    public static float w_balance = 1.0f;               // 1.5
    public static float w_alignment = 0.5f;             // 2.5
    public static float w_wallAlignment = 1.0f;         // 2.5
    public static float w_symmetry = 1.0f;              // 1
    public static float w_emphasis = 1.0f;              // 4
    
    // for modifying layouts
    public static float position = 0.2f;    // 0.2
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




}
