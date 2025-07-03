using System.Collections.Generic;
using UnityEngine;

public static class Parameters
{
    public static int numberOfChairs = 4;
    public static int numberOfTables = 1;

    public static float floorSizeX = UnitConverter.MetersToInches(5.9f); // in inches
    public static float floorSizeZ = UnitConverter.MetersToInches(7.4f); // in inches

    // weights
    public static float w_clearanceViolation = 2.0f;
    public static float w_circulation = 1.0f;
    public static float w_pairwiseDistance = 2.0f;
    public static float w_pairwiseAngle = 2.0f;
    public static float w_conversationDistance = 2.0f;
    public static float w_conversationAngle = 2.0f;
    public static float w_balance = 1.5f;
    public static float w_alignment = 2.5f;
    public static float w_wallAlignment = 2.5f;
    public static float w_symmetry = 1.0f;
    public static float w_emphasis = 4.0f;
    
    // for modifying layouts
    public static float position = 0.2f; // 0.2
    public static float rotation = 20f;
    
    // testing
    public static List<float> xPositions = new List<float> { -1.0f, 0.0f,  0.0f,   0.0f,   0.0f,  0.0f };
    public static List<float> zPositions = new List<float> { -2.0f, 0.0f,  0.0f,   0.0f,   0.0f,  0.0f };
    public static List<float> rotations = new List<float>  { 0.0f,  90.0f, 180.0f, -90.0f, 90.0f, 0.0f };
    public static List<int> fIndex = new List<int>         { 4,     0,     2,      3,      4,     0, 2, 3 }; // what item
    public static List<int> modifications = new List<int>  { 0,     1,     1,      1,      1,     0, 0, 0}; // translation or rotation

    
    // iterations of MCMC 2000
    public static int iterations = 2000;




}
