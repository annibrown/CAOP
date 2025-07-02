using UnityEngine;

public static class Parameters
{
    public static int numberOfChairs = 0;
    public static int numberOfTables = 0;
    public static int numberOfFurnitureItems = 4;
    public static int numberOfWalls = 4;
    public static int numberOfGroups = 1;

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
    public static float position = 1.0f; // 0.2
    public static float rotation = 20f;
    
    // iterations of MCMC 2000
    public static int iterations = 10;




}
