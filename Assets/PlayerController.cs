using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float forwardSpeed = 5f;
    public float laneDistance = 5f;
    public float jumpForce = 10f;
    public float gravity = -20f;
    public float jumpDriftCompensation = -0.5f;

    private CharacterController controller;
    private int currentLane = 1;
    private Vector3 moveDirection;
    private bool isJumping = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("CharacterController not found!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && currentLane > 0)
        {
            currentLane--;
        }
        if (Input.GetKeyDown(KeyCode.D) && currentLane < 2)
        {
            currentLane++;
        }

        float targetX = (currentLane - 1) * laneDistance;
        float currentX = transform.position.x;
        float newX = Mathf.MoveTowards(currentX, targetX, 15f * Time.deltaTime);

        float xMovement = newX - currentX;
        
        if (isJumping)
        {
            xMovement += jumpDriftCompensation * Time.deltaTime;
        }

        moveDirection.z = forwardSpeed;

        if (controller.isGrounded)
        {
            isJumping = false;
            moveDirection.y = -1.5f;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                moveDirection.y = jumpForce;
                isJumping = true;
            }
        }
        else
        {
            moveDirection.y += gravity * Time.deltaTime;
        }

        Vector3 motion = new Vector3(xMovement, moveDirection.y * Time.deltaTime, moveDirection.z * Time.deltaTime);
        controller.Move(motion);
    }
}