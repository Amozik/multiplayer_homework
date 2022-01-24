using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public abstract class FireAction : MonoBehaviour
{
    [SerializeField]
    private GameObject _bulletPrefab;
    [SerializeField]
    private int _startAmmunition = 20;

    public string BulletCount { get; protected set; } = string.Empty;
    public int Damage { get; protected set; }
    public Action<Vector3> OnFire { get; set; }
    
    protected Queue<GameObject> _bullets = new Queue<GameObject>();
    protected Queue<GameObject> _ammunition = new Queue<GameObject>();
    protected bool _reloading = false;

    protected virtual void Start()
    {
        for (var i = 0; i < _startAmmunition; i++)
        {
            GameObject bullet;
            if (_bulletPrefab == null)
            {
                bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bullet.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                bullet.AddComponent<Rigidbody>();
            }
            else
            {
                bullet = Instantiate(_bulletPrefab);
            }
            bullet.SetActive(false);
            _ammunition.Enqueue(bullet);
        }
    }

    public virtual async void Reloading()
    {
        _bullets = await Reload();
    }

    protected virtual void Shooting()
    {
        if (_bullets.Count == 0)
        {
            Reloading();
        }
    }

    private async Task<Queue<GameObject>> Reload()
    {
        if (!_reloading)
        {
            _reloading = true;
            StartCoroutine(ReloadingAnim());
            return await Task.Run(delegate
            {
                var cage = 10;
                if (_bullets.Count < cage)
                {
                    Thread.Sleep(3000);
                    var bullets = this._bullets;
                    while (bullets.Count > 0)
                    {
                        _ammunition.Enqueue(bullets.Dequeue());
                    }
                    cage = Mathf.Min(cage, _ammunition.Count);
                    if (cage > 0)
                    {
                        for (var i = 0; i < cage; i++)
                        {
                            var sphere = _ammunition.Dequeue();
                            bullets.Enqueue(sphere);
                        }
                    }
                }
                _reloading = false;
                return _bullets;
            });
        }
        else
        {
            return _bullets;
        }
    }

    private IEnumerator ReloadingAnim()
    {
        while (_reloading)
        {
            BulletCount = " | ";
            yield return new WaitForSeconds(0.01f);
            BulletCount = @" \ ";
            yield return new WaitForSeconds(0.01f);
            BulletCount = "---";
            yield return new WaitForSeconds(0.01f);
            BulletCount = " / ";
            yield return new WaitForSeconds(0.01f);
        }
        BulletCount = _bullets.Count.ToString();
        yield return null;
    }
}

