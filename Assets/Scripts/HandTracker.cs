using Leap;
using UnityEngine;

public class HandTracker : MonoBehaviour
{
    public LeapServiceProvider leapProvider;
    public static GameObject targetObject;
    
    public float movementThreshold = 1.5f;

    private float previousX = 0f;
    private bool initialized = false;

    private int timesMoved = 0;
    
    public static bool canMove = false;
    
    void Update()
    {
        if (canMove)
        {
            Frame frame = leapProvider.CurrentFrame;
        
            if (frame.Hands.Count == 0)
            {
                initialized = false;
                return;
            }

            Hand hand = frame.Hands[0];

            float currentX = hand.PalmPosition.x;

            if (!initialized)
            {
                previousX = currentX;
                initialized = true;
                return;
            }

            float deltaX = (currentX - previousX) * 1000f;

            // if (Mathf.Abs(deltaX) > 0.5)
            // {
            //     Debug.Log("Change: " + deltaX);
            // }

            // Debug.Log("times moved: " + timesMoved);
            if (Mathf.Abs(deltaX) > movementThreshold && timesMoved == 5)
            {
                // Moving left → increase Z, right → decrease Z
                float direction = Mathf.Sign(deltaX); // flip direction if needed
                Vector3 currentPos = targetObject.transform.position;
                currentPos.z = direction * 1;
                targetObject.transform.position = currentPos;
                timesMoved++;
                //Manager.current.StartCalculation();
                canMove = false;
            } 
            else if (Mathf.Abs(deltaX) > movementThreshold)
            {
                timesMoved++;
            }
            else
            {
                timesMoved = 0;
            }

            previousX = currentX;
        }
    }
}