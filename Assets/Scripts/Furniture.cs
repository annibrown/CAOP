using UnityEngine;

public class Furniture : MonoBehaviour
{
    public GameObject chairToCreate;
    public GameObject tableToCreate;
    void Start()
    {
        // create chairs
        for (int i = 0; i < Parameters.numberOfChairs; i++)
        {
            GameObject newChair = Instantiate(chairToCreate, new Vector3(i - 58, -65.4299f, 64), Quaternion.identity);
            newChair.tag = "Chair";
            Layout.F.Add(newChair);
        }
        
        // create tables
        for (int i = 0; i < Parameters.numberOfTables; i++)
        {
            GameObject newTable = Instantiate(tableToCreate, new Vector3(i, 0.2f, 0), Quaternion.identity);
            newTable.tag = "Table";
            Layout.F.Add(newTable);
        }
    }
}
