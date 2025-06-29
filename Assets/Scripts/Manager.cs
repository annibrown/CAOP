using UnityEngine;

public class Manager : MonoBehaviour
{
    public FloorGridGenerator floorGrid; // assign this in the Inspector

    public void Calculate()
    {
        floorGrid.UpdateTileColors();
        Costs.TotalCost();
    }
}
