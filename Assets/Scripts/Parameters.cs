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
    private static float w_clearanceViolation = 2.0f;
    private static float w_circulation = 1.0f;
    private static float w_pairwiseDistnace = 2.0f;
    private static float w_pa = 2.0f;
    private static float w_cd = 2.0f;
    private static float w_ca = 2.0f;
    private static float w_vd = 1.5f;
    private static float w_fa = 2.5f;
    private static float w_wa = 2.5f;
    private static float w_sy = 1.0f;
    private static float w_ef = 4.0f;
    



}
