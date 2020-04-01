using UnityEngine;

public enum CameraMode { Unknown = 0, FreeFlight = 1 }

public class GhostCameraController : MonoBehaviour
{
    public bool Enabled { get; protected set; }

    public CameraMode Mode { get; set; }

    protected Vector3 HomePosition;
    protected Quaternion HomeRotation;

    protected float InitialSpeed = 5f;
    protected float Acceleration = 50f;
    protected float MaxSpeed = 500f;

    protected float CursorSensitivity = 0.005f;
    protected float RotationSpeed = 2f;

    protected float CurrentSpeed = 0f;
    protected bool Moving = false;

    protected void Start()
    {
        HomePosition = transform.position;
        HomeRotation = transform.rotation;
    }

    protected void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            Enabled = !Enabled;
        }

        Mode = CameraMode.FreeFlight;

        if (Enabled)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mode == CameraMode.FreeFlight)
            UpdateFreeFlight();
    }

    protected void UpdateFreeFlight()
    {
        if (!Enabled)
            return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            Moving = false;
            CurrentSpeed = 0;
            transform.position = HomePosition;
            transform.rotation = HomeRotation;
            return;
        }

        // update movement
        bool lastMoving = Moving;
        Vector3 deltaPosition = Vector3.zero;

        if (Moving)
        {
            CurrentSpeed = Mathf.Min(MaxSpeed, CurrentSpeed + Acceleration * Time.deltaTime);
        }

        Moving = false;

        CheckTranslation(KeyCode.W, ref deltaPosition, Vector3.forward);
        CheckTranslation(KeyCode.S, ref deltaPosition, -Vector3.forward);
        CheckTranslation(KeyCode.D, ref deltaPosition, Vector3.right);
        CheckTranslation(KeyCode.A, ref deltaPosition, -Vector3.right);
        CheckTranslation(KeyCode.E, ref deltaPosition, Vector3.up);
        CheckTranslation(KeyCode.Q, ref deltaPosition, -Vector3.up);

        if (Moving)
        {
            if (Moving != lastMoving)
                CurrentSpeed = InitialSpeed;

            transform.position += transform.localRotation * deltaPosition.normalized * CurrentSpeed * Time.deltaTime;
        }
        else
        {
            CurrentSpeed = 0f;
        }

        // update rotation
        Vector3 angles = transform.eulerAngles;
        angles.x += -Input.GetAxis("Mouse Y") * 359f * CursorSensitivity;
        angles.y += Input.GetAxis("Mouse X") * 359f * CursorSensitivity;

        CheckRotation(KeyCode.LeftArrow, ref angles, -Vector3.up);
        CheckRotation(KeyCode.RightArrow, ref angles, Vector3.up);
        CheckRotation(KeyCode.UpArrow, ref angles, -Vector3.right);
        CheckRotation(KeyCode.DownArrow, ref angles, Vector3.right);

        if (angles.x > 89 && angles.x < 180)
            angles.x = 89;
        else if (angles.x < 271 && angles.x > 180)
            angles.x = 271;

        transform.eulerAngles = angles;
    }

    protected void CheckTranslation(KeyCode keyCode, ref Vector3 deltaPosition, Vector3 directionVector)
    {
        if (Input.GetKey(keyCode))
        {
            Moving = true;
            deltaPosition += directionVector;
        }
    }

    protected void CheckRotation(KeyCode keyCode, ref Vector3 eulerAngles, Vector3 axis)
    {
        if (Input.GetKey(keyCode))
        {
            eulerAngles += RotationSpeed * axis;
        }
    }
}