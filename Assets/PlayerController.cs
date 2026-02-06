using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 28f;
    public float laneDistance = 5f;
    public float jumpForce = 10f;
    public float gravity = -20f;
    public float jumpDriftCompensation = -0.5f;

    [Header("UI Binding")]
    public TMP_InputField myInputField;
    public GameObject resultPanel;
    public TextMeshProUGUI finalMessageText;
    public GameObject openInputButton;
    public TextMeshProUGUI openButtonText;

    private CharacterController controller;
    private Animator animator;
    private int currentLane = 1;
    private Vector3 moveDirection;
    private bool isJumping = false;
    private bool isDead = false;
    private bool isGameStarted = false;

    // --- Key for saving data ---
    private const string PASSWORD_KEY = "SavedPassword";

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        if (animator != null) animator.speed = 0f; 
        
        if (resultPanel != null) resultPanel.SetActive(false);
        if (myInputField != null) myInputField.gameObject.SetActive(false);

        // --- Step 1: Load saved string on start ---
        if (myInputField != null && PlayerPrefs.HasKey(PASSWORD_KEY))
        {
            myInputField.text = PlayerPrefs.GetString(PASSWORD_KEY);
            Debug.Log("Loaded saved password: " + myInputField.text);
        }

        if (controller == null) Debug.LogError("CharacterController not found!");
    }

    public void ToggleInputBox()
    {
        if (myInputField == null) return;
        bool isActive = !myInputField.gameObject.activeSelf;
        myInputField.gameObject.SetActive(isActive);
        if (openButtonText != null)
        {
            openButtonText.text = isActive ? "CLOSE" : "SETTINGS";
        }
    }

    public void TogglePasswordVisibility()
    {
        if (myInputField == null) return;
        myInputField.contentType = (myInputField.contentType == TMP_InputField.ContentType.Password) ? 
            TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
        myInputField.ForceLabelUpdate();
    }

    public void StartTheGame()
    {
        if (isDead) return;

        // --- Step 2: Save the string automatically when start is clicked ---
        if (myInputField != null)
        {
            PlayerPrefs.SetString(PASSWORD_KEY, myInputField.text);
            PlayerPrefs.Save(); // Force write to disk
            Debug.Log("Password saved: " + myInputField.text);
        }

        isGameStarted = true;
        if (animator != null) animator.speed = 1f; 

        if (myInputField != null) myInputField.gameObject.SetActive(false);
        if (openInputButton != null) openInputButton.SetActive(false);
    }

    void Update()
    {
        if (!isGameStarted || isDead) return;

        if (Input.GetKeyDown(KeyCode.A) && currentLane > 0) currentLane--;
        if (Input.GetKeyDown(KeyCode.D) && currentLane < 2) currentLane++;

        float targetX = (currentLane - 1) * laneDistance;
        float currentX = transform.position.x;
        float newX = Mathf.MoveTowards(currentX, targetX, 15f * Time.deltaTime);
        float xMovement = newX - currentX;
        
        if (isJumping) xMovement += jumpDriftCompensation * Time.deltaTime;

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

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle")) Die();
        else if (hit.gameObject.CompareTag("Finish")) Win();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        isGameStarted = false;
        forwardSpeed = 0;
        if (animator != null)
        {
            animator.speed = 2.5f;
            animator.Play("Armature|Armature|Armature|Armature|Dead|baselayer");
        }
        Invoke("RestartGame", 1f);
    }

    void Win()
    {
        if (isDead) return;
        isDead = true;
        isGameStarted = false;
        forwardSpeed = 0;
        if (resultPanel != null && myInputField != null)
        {
            resultPanel.SetActive(true);
            finalMessageText.text = "FINISH!\nYour Office Wi-Fi SSID is:\n\"" + myInputField.text + "\"";
        }
        Invoke("LoadNextLevel", 5f);
    }

    void RestartGame() { UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); }
    void LoadNextLevel() { UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); }
}