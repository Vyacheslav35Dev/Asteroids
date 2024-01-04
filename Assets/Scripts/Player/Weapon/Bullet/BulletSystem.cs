using System;
using System.Collections.Generic;
using Data;
using Interfaces;
using Systems;
using Systems.Pool;
using UnityEngine;

namespace Player.Weapon.Bullet
{
    public class BulletSystem : ISystemUpdate, IDisposable
    {
        private readonly BulletView _bullet;
        private readonly PlayerView _playerView;
        private readonly OutOfSceneSystem _outOfSceneSystem;
        private readonly GameplayData _gamePlayData;

        private PoolService<BulletView> _bulletPool;

        private Dictionary<BulletView, Vector2> _spawnedBullets = new();

        public BulletSystem(BulletView bullet, PlayerView playerView, OutOfSceneSystem outOfSceneSystem, GameplayData gamePlayData)
        {
            _bullet = bullet;
            _playerView = playerView;
            _outOfSceneSystem = outOfSceneSystem;
            _gamePlayData = gamePlayData;
            _bulletPool = new PoolService<BulletView>(_bullet);
        }

        public void Dispose()
        {
            foreach(var bullet in _spawnedBullets.Keys)
            {
                bullet.OnReturn -= RemoveBullet;
                bullet.OnHit -= OnHit;
            }

            _spawnedBullets.Clear();
            _bulletPool.ClearPool();
        }

        public void OnUpdate()
        {
            foreach (var bullet in _spawnedBullets)
            {
                var bulletTransform = bullet.Key.transform;

                var towardPosition = bulletTransform.position + (Vector3)bullet.Value;

                bulletTransform.position = Vector2.MoveTowards(bulletTransform.position, towardPosition,
                    Time.deltaTime * _gamePlayData.BulletSpeed);
            }
        }

        public void Spawn()
        {
            var bullet = _bulletPool.GetItem();
            var bulletTransform = bullet.transform;
            var direction = _playerView.transform.up;

            bullet.gameObject.SetActive(true);
            bulletTransform.position = _playerView.BulletSpawnPosition.position;
            bulletTransform.rotation = _playerView.transform.rotation;

            _spawnedBullets.Add(bullet, direction);

            _outOfSceneSystem.AddItem(bullet);

            bullet.OnReturn += RemoveBullet;
            bullet.OnHit += OnHit;
        }

        private void RemoveBullet(BulletView bullet)
        {
            bullet.OnReturn -= RemoveBullet;

            _spawnedBullets.Remove(bullet);
        }

        private void OnHit(IHitable bulletInterface, IHitable target)
        {
            var bullet = (BulletView)bulletInterface;

            bullet.OnHit -= OnHit;

            bullet.ReturnToPool();
            bullet.gameObject.SetActive(false);

            target.Hit();
        }
    }
}