using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class RayShooter : FireAction
{
    private Camera _camera;
    private int _damage = 50;

    protected override void Start()
    {
        base.Start();
        _camera = GetComponentInChildren<Camera>();
        Damage = _damage;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shooting();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reloading();
        }

        if (Input.anyKey && !Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    protected override void Shooting()
    {
        base.Shooting();
        if (_bullets.Count > 0)
        {
            StartCoroutine(Shoot());
        }
    }

    private IEnumerator Shoot()
    {
        if (_reloading)
        {
            yield break;
        }
        var point = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0);
        var ray = _camera.ScreenPointToRay(point);
        if (!Physics.Raycast(ray, out var hit))
        {
            yield break;
        }

        var forward = transform.TransformDirection(Vector3.forward) * 10;
        Debug.DrawRay(hit.point, forward, Color.red);
        
        var shoot = _bullets.Dequeue();
        BulletCount = _bullets.Count.ToString();
        _ammunition.Enqueue(shoot);
        shoot.GetComponent<Rigidbody>().velocity = forward;
        shoot.SetActive(true);
        shoot.transform.position = hit.point;
        shoot.transform.parent = hit.transform;
        
        OnFire?.Invoke(forward);

        yield return new WaitForSeconds(2.0f);
        shoot.SetActive(false);
    }

    
}

