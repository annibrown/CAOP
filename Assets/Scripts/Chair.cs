using UnityEngine;

public class Chair : MonoBehaviour
{
    public GameObject prefabToCreate;
    void Start()
    {
        for (int i = 0; i < Parameters.numberOfChairs; i++)
        {
            GameObject newChair = Instantiate(prefabToCreate, new Vector3(i * 60.0f, 0, 0), Quaternion.identity);
            newChair.tag = "Chair";
            Layout.F.Add(newChair);
        }
    }
}
