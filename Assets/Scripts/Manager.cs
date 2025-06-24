using UnityEngine;

public class Manager : MonoBehaviour
{
    public static bool readyToCalculate = false;
    private bool calculated = false;

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Manager Update Called");
        if (readyToCalculate == true && calculated == false)
        {
            //Debug.Log("Ready to calculate!");
            Costs.TotalCost();
            calculated = true;
        }
    }
}    
