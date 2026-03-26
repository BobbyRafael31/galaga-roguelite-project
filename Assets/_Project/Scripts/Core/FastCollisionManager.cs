using System.Collections.Generic;
using UnityEngine;

public interface IAABBEntity
{
    Vector2 Position { get; }
    Vector2 Extents { get; }
    bool IsActive { get; }
    void OnCollide(IAABBEntity other);
}

public class FastCollisionManager : MonoBehaviour
{
    public static FastCollisionManager Instance { get; private set; }

    private readonly List<IAABBEntity> _playerBullets = new List<IAABBEntity>(500);
    private readonly List<IAABBEntity> _enemies = new List<IAABBEntity>(100);
    private readonly List<IAABBEntity> _enemyBullets = new List<IAABBEntity>(500);

    private IAABBEntity _player;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPlayer(IAABBEntity player) => _player = player;

    public void UnregisterPlayer(IAABBEntity player)
    {
        if (_player == player)
        {
            _player = null;
        }
    }

    public void RegisterPlayerBullet(IAABBEntity bullet) => _playerBullets.Add(bullet);
    public void UnregisterPlayerBullet(IAABBEntity bullet) => _playerBullets.Remove(bullet);

    public void RegisterEnemy(IAABBEntity enemy) => _enemies.Add(enemy);
    public void UnregisterEnemy(IAABBEntity enemy) => _enemies.Remove(enemy);
    public void RegisterEnemyBullet(IAABBEntity bullet) => _enemyBullets.Add(bullet);
    public void UnregisterEnemyBullet(IAABBEntity bullet) => _enemyBullets.Remove(bullet);

    public int GetEnemyBulletCount() => _enemyBullets.Count;

    public System.Collections.Generic.IReadOnlyList<IAABBEntity> Enemies => _enemies;
    public System.Collections.Generic.IReadOnlyList<IAABBEntity> EnemyBullets => _enemyBullets;

    private void Update()
    {
        CheckPlayerBulletsVsEnemies();
        CheckHeavyBulletsVsEnemyBullets();

        if (_player as MonoBehaviour != null && _player.IsActive)
        {
            CheckPlayerVsEnemies();
        }

        if (_player as MonoBehaviour != null && _player.IsActive)
        {
            CheckPlayerVsEnemyBullets();
        }
    }

    private void CheckPlayerBulletsVsEnemies()
    {
        for (int i = _playerBullets.Count - 1; i >= 0; i--)
        {
            var bullet = _playerBullets[i];
            if (!bullet.IsActive) continue;

            for (int j = _enemies.Count - 1; j >= 0; j--)
            {
                var enemy = _enemies[j];
                if (!enemy.IsActive) continue;

                if(CheckAABB(bullet, enemy))
                {
                    bullet.OnCollide(enemy);
                    enemy.OnCollide(bullet);
                    break;
                }

            }
        }
    }

    private void CheckHeavyBulletsVsEnemyBullets()
    {
        for (int i = _playerBullets.Count - 1; i >= 0; i--)
        {
            var pBullet = _playerBullets[i] as PlayerBullet;
            if (pBullet == null || !pBullet.IsActive || !pBullet.IsHeavy) continue;

            for (int j = _enemyBullets.Count - 1; j >= 0; j--)
            {
                var eBullet = _enemyBullets[j];
                if (!eBullet.IsActive) continue;

                if (CheckAABB(pBullet, eBullet))
                {
                    eBullet.OnCollide(pBullet);
                }
            }
        }
    }

    private void CheckPlayerVsEnemies()
    {
        for (int i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            if (!enemy.IsActive) continue;

            if (CheckAABB(_player, enemy))
            {
                _player.OnCollide(enemy);
                enemy.OnCollide(_player);

                break;
            }
        }
    }

    private void CheckPlayerVsEnemyBullets()
    {
        for (int i = _enemyBullets.Count - 1; i >= 0; i--)
        {
            var bullet = _enemyBullets[i];
            if (!bullet.IsActive) continue;

            if (CheckAABB(_player, bullet))
            {
                _player.OnCollide(bullet);
                bullet.OnCollide(_player);
                break;
            }
        }
    }

    private bool CheckAABB(IAABBEntity a, IAABBEntity b)
    {
        return Mathf.Abs(a.Position.x - b.Position.x) < (a.Extents.x + b.Extents.x) &&
               Mathf.Abs(a.Position.y - b.Position.y) < (a.Extents.y + b.Extents.y);
    }

}
