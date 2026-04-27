using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public Transform cameraTarget;

    private struct Inputs
    {
        public Vector2 mov;
        public Vector2 rotation;
    }

    private float xRot;
    private float yRot;

    public float speed;
    private Inputs input;
    void Start()
    {
        input = new Inputs();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        SetRotation();
    }

    void GetInputs()
    {
        input.mov = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input.rotation = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        cameraTarget.transform.position += GetMovementVector(input.mov) * speed * Time.deltaTime;
    }

    Vector3 GetMovementVector(Vector2 inputVector)
    {
        Vector3 movement = cameraTarget.transform.forward * input.mov.y + cameraTarget.transform.right * input.mov.x;
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }
        return movement;
    }

    void SetRotation()
    {
        xRot += input.rotation.x;
        yRot += input.rotation.y;
        yRot = Mathf.Clamp(yRot, -90, 90);
        cameraTarget.transform.rotation = Quaternion.Euler(-yRot, xRot, 0);
    }
}
