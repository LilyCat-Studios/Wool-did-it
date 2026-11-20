using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class playerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float turnSpeed;
    
    [Header("Camera")]
    [SerializeField] [Range(0, 5)] private float cameraSpeed;
    [SerializeField] private float minAngle;
    [SerializeField] private float maxAngle;

    [Header("UI")] 
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject clueBook;
    
    [Header("Clues")]
    [SerializeField] private float clueDistance;
    [SerializeField] private Transform clueCheckOrigin;

    private Camera _boardCam;
    private bool _inBoard;
    
    private CharacterController _controller;
    private Transform _camPivot;
    private Transform _character;
    private Animator _animator;
    
    // input
    private Vector3 _moveDirection = Vector3.zero;
    private bool _looking;
    private bool _pauseMenu;

    private Vector2 _lookVector;
    private float characterAngle;
    private Vector2 velocity;
    
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _camPivot = transform.Find("CameraPivot");
        _character = transform.Find("Character");
        _animator = _character.GetComponent<Animator>();
        
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        _controller.SimpleMove(_character.rotation * _moveDirection);
        
        velocity = SmoothVelocity(velocity, _moveDirection, Time.deltaTime);
        
        _animator.SetFloat("speed", velocity.y);
        _animator.SetFloat("Strafe", velocity.x);

        UpdateRotation();
    }

    public void OnPickup(InputValue value) {
        if(_inBoard) {
            _inBoard = false;
            _boardCam.gameObject.SetActive(false);
            GetComponentInChildren<Camera>(true).gameObject.SetActive(true);
            UpdateTimeScale();
        } else {
            _animator.SetTrigger("pickup");
        }
    }

    public void OnPause() {
        _pauseMenu = !_pauseMenu;
        pauseMenu.SetActive(_pauseMenu);
        UpdateTimeScale();
    }

    private void UpdateTimeScale() {
        bool pauseTime = ShouldPauseTime(_inBoard, _pauseMenu);
        Time.timeScale = pauseTime ? 0 : 1;
        Cursor.lockState = pauseTime ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void Quit() {
        Application.Quit();
    }

    public void CheckForClues() 
    {
        Collider[] clueChecks = Physics.OverlapSphere(clueCheckOrigin.position, clueDistance);
        foreach(Collider clue in clueChecks) 
        {
            if(clue.gameObject.name == "GuessBoard") 
            {
                print("Guessboard");
                _boardCam = clue.gameObject.GetComponentInChildren<Camera>(true);
                _boardCam.gameObject.SetActive(true);
                GetComponentInChildren<Camera>(true).gameObject.SetActive(false);
                _inBoard = true;
                UpdateTimeScale();
            }
            if(!clue.CompareTag("Clue")) 
            {
                continue;
            }

            AddClue(clue.gameObject);
        }
    }

    private void OnDrawGizmos() 
    {
        Gizmos.color = Color.rebeccaPurple;
        Gizmos.DrawWireSphere(clueCheckOrigin.position, clueDistance);
        
    }

    private void AddClue(GameObject clue) 
    {
        RawImage[] images = clueBook.GetComponentsInChildren<RawImage>(true);
        foreach(RawImage image in images) {
            string bookClueName = image.gameObject.transform.parent.parent.gameObject.name;
            if(image.gameObject.transform.parent.gameObject.activeSelf) {
                continue;
            }
            if(ClueMatchesPage(bookClueName, clue.name)) {
                clueBook.SetActive(true);
                image.gameObject.transform.parent.gameObject.SetActive(true);
            }
        }
    }

    private void OnClues() {
        clueBook.SetActive(!clueBook.activeSelf);
    }

    
    private void OnMove(InputValue value)
    {
        Vector2 move = value.Get<Vector2>();
        
        Vector3 direction = CalculateMoveDirection(move, speed);
        _moveDirection.x = direction.x;
        _moveDirection.z = direction.z;
        _animator.SetBool("moving", move != Vector2.zero);
    }

    private void OnMouseMove(InputValue value)
    {
        _lookVector = ClampLook(_lookVector + value.Get<Vector2>() * cameraSpeed, minAngle, maxAngle);
        _camPivot.rotation = Quaternion.Euler(-_lookVector.y, _lookVector.x, 0f);
    }


    private void UpdateRotation()
    {
        characterAngle = CalculateCharacterAngle(_lookVector.x, characterAngle, Time.deltaTime, turnSpeed);
        _character.rotation = Quaternion.Euler(0, characterAngle, 0);
    }

    internal static Vector3 CalculateMoveDirection(Vector2 move, float speed) 
    {
        Vector3 direction = Vector3.zero;
        direction.x = move.x * speed;
        direction.z = move.y * speed;
        if (direction.z < 0) direction.z *= 0.5f;
        direction.x *= 0.5f;
        return direction;
    }

    internal static Vector2 ClampLook(Vector2 look, float minAngle, float maxAngle) 
    {
        if (look.y < minAngle) look.y = minAngle;
        else if (look.y > maxAngle) look.y = maxAngle;
        return look;
    }

    internal static Vector2 SmoothVelocity(Vector2 velocity, Vector3 moveDirection, float deltaTime) 
    {
        velocity.y += (moveDirection.z - velocity.y) * deltaTime * 10;
        velocity.x += (moveDirection.x - velocity.x) * deltaTime * 10;
        return velocity;
    }

    internal static float CalculateCharacterAngle(float lookAngle, float characterAngle, float deltaTime, float turnSpeed) 
    {
        float delta = lookAngle - characterAngle;
        delta %= 360f;
        characterAngle = lookAngle - delta;
        
        characterAngle += delta * deltaTime * turnSpeed;
        return characterAngle;
    }

    internal static bool ShouldPauseTime(bool inBoard, bool pauseMenu) 
    {
        return inBoard | pauseMenu;
    }

    internal static bool ClueMatchesPage(string bookClueName, string clueName) 
    {
        return (bookClueName == "Weapon" && clueName == "shovel") || (bookClueName == "Wool" && clueName == "Wool") || (bookClueName == "Location" && clueName == "bodytape");
    }
}