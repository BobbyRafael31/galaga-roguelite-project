using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private PlayerBullet _bulletPrefab;
    [SerializeField] private Transform _firePoint;

    [Header("Stats")]
    [Tooltip("Maximum bullets allowed on screen simultaneously.")]
    [SerializeField] private int _baseMaxBulletsOnScreen = 2;

    [Tooltip("Minimum delay between taps. Set to 0.05 for arcade-tight double shots.")]
    [SerializeField] private float _baseFireCooldown = 0.05f;

    public Stat MaxBulletsOnScreen { get; private set; }
    public Stat FireCooldown { get; private set; }

    private readonly List<PlayerBullet> _activeBullets = new List<PlayerBullet>(20);
    private float _lastFireTime;

    private void Awake()
    {
        MaxBulletsOnScreen = new Stat(_baseMaxBulletsOnScreen);
        FireCooldown = new Stat(_baseFireCooldown);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            if (MaxBulletsOnScreen != null) MaxBulletsOnScreen.BaseValue = _baseMaxBulletsOnScreen;
            if (FireCooldown != null) FireCooldown.BaseValue = _baseFireCooldown;
        }
    }

    private void OnEnable()
    {
        if (_inputReader != null)
        {
            _inputReader.OnFireEvent += HandleFire;
        }
    }

    private void OnDisable()
    {
        if (_inputReader != null)
        {
            _inputReader.OnFireEvent -= HandleFire;
        }
    }

    private void Update()
    {
        CleanActiveBulletList();
    }

    private void CleanActiveBulletList()
    {
        for (int i = _activeBullets.Count - 1; i >= 0; i--)
        {
            if (!_activeBullets[i].gameObject.activeInHierarchy)
            {
                _activeBullets.RemoveAt(i);
            }
        }
    }

    private void HandleFire()
    {
        if (_activeBullets.Count >= Mathf.FloorToInt(MaxBulletsOnScreen.Value))
            return;

        if (Time.time < _lastFireTime + FireCooldown.Value)
            return;

        _lastFireTime = Time.time;

        if (PoolManager.Instance != null)
        {
            PlayerBullet newBullet = PoolManager.Instance.Get(_bulletPrefab, _firePoint.position, Quaternion.identity);
            _activeBullets.Add(newBullet);
        }
        else
        {
            Debug.LogError("[PlayerShooter] PoolManager is missing from the scene!");
        }
    }
}