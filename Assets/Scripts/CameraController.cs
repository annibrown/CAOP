using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;
    public float boostMultiplier = 2f;

    private float yaw = 0f;
    private float pitch = 0f;
    private bool cursorLocked = true;
    
    public GameObject resumeButton;

    void Start()
    {
        // Optionally reset yaw and pitch based on initial camera rotation
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // Start coroutine to delay cursor lock (prevents UI blocking too early)
        StartCoroutine(DelayedLockCursor());
    }

    
    private IEnumerator DelayedLockCursor()
    {
        yield return new WaitForSeconds(0.2f); // wait 0.2 seconds (adjust if needed)
        LockCursor(true);
    }


    void Update()
    {
        HandleCursorToggle();

        if (cursorLocked)
        {
            HandleMouseLook();
            HandleMovement();
        }
    }

    void HandleCursorToggle()
    {
        // ESC unlocks cursor
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LockCursor(false);
        }

        // Press Enter (or any other key you choose) to relock
        if (!cursorLocked && Input.GetKeyDown(KeyCode.Return)) // or KeyCode.C, etc.
        {
            LockCursor(true);
        }
    }

    
    private IEnumerator DelayedRelockCursor()
    {
        yield return new WaitForEndOfFrame();  // 🧠 key fix: wait for full UI processing
        LockCursor(true);
    }

    void LockCursor(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;

        // Show/hide the Resume button
        if (resumeButton != null)
            resumeButton.SetActive(!locked); // show when unlocked, hide when locked

        Debug.Log("Cursor is now " + (locked ? "LOCKED" : "UNLOCKED"));
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal"); // A/D
        float moveZ = Input.GetAxis("Vertical");   // W/S
        float moveY = 0f;

        if (Input.GetKey(KeyCode.Space)) moveY += 1f;       // Up
        if (Input.GetKey(KeyCode.LeftShift)) moveY -= 1f; // Down

        Vector3 moveDir = transform.right * moveX + transform.up * moveY + transform.forward * moveZ;
        float speed = moveSpeed * (Input.GetKey(KeyCode.Tab) ? boostMultiplier : 1f);
        transform.position += moveDir * speed * Time.deltaTime;
    }
    
    public void ResumeCameraControl()
    {
        LockCursor(true);
    }

}