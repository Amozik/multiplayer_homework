using UnityEngine;
using UnityEngine.Networking;

public class PlayerCharacter : Character
{
    [Range(0, 100)] 
    [SerializeField] 
    private int _health = 100;

    [Range(0.5f, 10.0f)] 
    [SerializeField] 
    private float _movingSpeed = 8.0f;
    
    [SerializeField] 
    private float _acceleration = 3.0f;
    
    private const float _gravity = -9.8f;
    private CharacterController _characterController;
    private MouseLook _mouseLook;

    private Vector3 _currentVelocity;
    
    [SyncVar]
    private int _currentHealth;

    protected override FireAction FireAction { get; set; }

    protected override void Initiate()
    {
        base.Initiate();
        FireAction = gameObject.AddComponent<RayShooter>();
        FireAction.Reloading();
        FireAction.OnFire += CmdTryDealDamage;
        _characterController = GetComponentInChildren<CharacterController>();
        _characterController ??= gameObject.AddComponent<CharacterController>();
        _mouseLook = GetComponentInChildren<MouseLook>();
        _mouseLook ??= gameObject.AddComponent<MouseLook>();
        _currentHealth = _health;
    }

    public override void Movement()
    {
        if (_mouseLook != null && _mouseLook.PlayerCamera != null)
        {
            _mouseLook.PlayerCamera.enabled = hasAuthority;
        }

        if (hasAuthority)
        {
            var moveX = Input.GetAxis("Horizontal") * _movingSpeed;
            var moveZ = Input.GetAxis("Vertical") * _movingSpeed;
            var movement = new Vector3(moveX, 0, moveZ);
            movement = Vector3.ClampMagnitude(movement, _movingSpeed);
            movement *= Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                movement *= _acceleration;
            }

            movement.y = _gravity;
            movement = transform.TransformDirection(movement);

            _characterController.Move(movement);
            _mouseLook.Rotation();

            var playerTransform = transform;
            CmdUpdatePositionAndRotation(playerTransform.position, playerTransform.rotation);
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, _serverPosition, ref _currentVelocity, _movingSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, _serverRotation, .5f);
        }
    }
    
    [Command]
    private void CmdTryDealDamage(Vector3 direction)
    {
        if (Physics.Raycast(transform.position, direction, out var raycastHit, 10))
        {
            var character = raycastHit.transform.GetComponentInParent<PlayerCharacter>();
            if (character != null)
            {
                character.DealDamage(FireAction.Damage);
            }
        }
    }
    
    public void DealDamage(int value)
    {
        _currentHealth -= value;

        if (_currentHealth < 0)
        {
            //Move in invisible place
            CmdUpdatePositionAndRotation(new Vector3(0,0, -300), Quaternion.identity);
            
            gameObject.SetActive(false);
            
            if (isClient)
            {
                //NetworkClient.ShutdownAll(); //При вызове выдает кучу ошибок, не смг найти другого способа, как отключить клиент
                Destroy(gameObject);
            }
        }
    }

    private void Start()
    {
        Initiate();
    }
    private void OnGUI()
    {
        if (Camera.main == null)
        {
            return;
        }

        var info = $"Health: {_currentHealth}\nClip: {FireAction.BulletCount}";
        var size = 12;
        var bulletCountSize = 50;
        var posX = Camera.main.pixelWidth / 2 - size / 4;
        var posY = Camera.main.pixelHeight / 2 - size / 2;
        var posXBul = Camera.main.pixelWidth - bulletCountSize * 2;
        var posYBul = Camera.main.pixelHeight - bulletCountSize;
        GUI.Label(new Rect(posX, posY, size, size), "+");
        GUI.Label(new Rect(posXBul, posYBul, bulletCountSize * 2, bulletCountSize * 2), info);
    }
}

