using System;
using System.Collections.Generic;
using Components;
using Data;
using Interfaces;
using Score;
using Systems;
using Systems.Pool;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy
{
    public class AsteroidsSystem : ISystemUpdate, IDisposable
    {
        private readonly OutOfSceneSystem _outOfSceneObjectSystem;
        private readonly AsteroidsData _asteroidsData;
        private readonly GameScore _gameScore;

        private readonly Vector2 _spawnTimeRange;
        private readonly int _pointsForDestroy;
        private readonly float _speed;

        private PoolService<AsteroidView> _littleAsteroidsPool;
        private PoolService<AsteroidView> _mediumAsteroidsPool;
        private PoolService<AsteroidView> _largeAsteroidsPool;

        private Dictionary<AsteroidView, MoveComponent> _spawnedAsteroids = new();

        private float _timer;

        private const float DestroyedAsteroidChangeAngle = 30;

        public AsteroidsSystem(OutOfSceneSystem outOfSceneObjectSystem, AsteroidsData asteroidsData,
            GameScore gameScore, GameplayData gameplayData)
        {
            _outOfSceneObjectSystem = outOfSceneObjectSystem;
            _asteroidsData = asteroidsData;
            _gameScore = gameScore;

            _speed = gameplayData.AsteroidsSpeed;
            _spawnTimeRange = gameplayData.AsteroidSpawnTimeRange;
            _pointsForDestroy = gameplayData.PointsForAsteroidDestroy;
            
            _littleAsteroidsPool = new PoolService<AsteroidView>(_asteroidsData.LittleAsteroid);
            _mediumAsteroidsPool = new PoolService<AsteroidView>(_asteroidsData.MediumAsteroid);
            _largeAsteroidsPool = new PoolService<AsteroidView>(_asteroidsData.LargeAsteroid);
        }

        public void Dispose()
        {
            foreach (var asteroid in _spawnedAsteroids.Keys)
            {
                asteroid.OnReturn -= OnReturnToPool;
                asteroid.OnHit -= OnHit;
            }

            _spawnedAsteroids.Clear();

            _littleAsteroidsPool.ClearPool();
            _mediumAsteroidsPool.ClearPool();
            _largeAsteroidsPool.ClearPool();
        }
        
        public void OnUpdate()
        {
            SpawnAsteroids();
            MoveAsteroids();
        }

        private void SpawnAsteroids()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0)
            {
                var pool = GetRandomPoolService();
                var asteroid = pool.GetItem();
                var position = _outOfSceneObjectSystem.GetRandomOutOfScenePosition();
                var randomPositionInCenter = _outOfSceneObjectSystem.GetRandomPositionInCenter();
                var direction = (randomPositionInCenter - position).normalized * _speed;
                var moveComponents = new MoveComponent(direction);

                asteroid.transform.position = position;
                asteroid.gameObject.SetActive(true);

                _spawnedAsteroids.Add(asteroid, moveComponents);
                _outOfSceneObjectSystem.AddItem(asteroid);

                asteroid.OnReturn += OnReturnToPool;
                asteroid.OnHit += OnHit;

                _timer = Random.Range(_spawnTimeRange.x, _spawnTimeRange.y);
            }
        }

        private void MoveAsteroids()
        {
            foreach (var ufoParam in _spawnedAsteroids)
            {
                var ufo = ufoParam.Key;
                var parameters = ufoParam.Value;
                var ufoPosition = ufo.GetCurrentPosition();

                ufo.transform.position = Vector3.Lerp(ufoPosition,
                    ufoPosition + parameters.Direction,
                    _speed * Time.deltaTime);
            }
        }

        private PoolService<AsteroidView> GetRandomPoolService()
        {
            var random = Random.Range(0, 100);

            return random switch
            {
                < 33 => _littleAsteroidsPool,
                < 66 => _mediumAsteroidsPool,
                _ => _largeAsteroidsPool
            };
        }

        private void OnHit(IHitable asteroidInterface, IHitable empty)
        {
            var asteroid = (AsteroidView)asteroidInterface;
            var asteroidIndex = asteroid.AsteroidLevel;
            var pool = GetPoolByIndex(asteroidIndex);
            var direction = _spawnedAsteroids[asteroid].Direction;
            var position = asteroid.GetCurrentPosition();

            asteroid.ReturnToPool();

            _gameScore.IncreaseScore(_pointsForDestroy);

            if (pool is null)
                return;

            for (var i = -1; i < 2; i += 2)
            {
                var item = pool.GetItem();
                var newDirection = GetNewRotatedVector(direction, DestroyedAsteroidChangeAngle * i);
                var moveComponents = new MoveComponent(newDirection);

                item.transform.position = position;
                item.gameObject.SetActive(true);

                _spawnedAsteroids.Add(item, moveComponents);
                _outOfSceneObjectSystem.AddItem(item);

                item.OnReturn += OnReturnToPool;
                item.OnHit += OnHit;
            }
        }

        private void OnReturnToPool(AsteroidView asteroid)
        {
            asteroid.OnReturn -= OnReturnToPool;
            asteroid.OnHit -= OnHit;

            _spawnedAsteroids.Remove(asteroid);
        }

        private PoolService<AsteroidView> GetPoolByIndex(int index)
        {
            return index switch
            {
                < 1 => null,
                < 2 => _littleAsteroidsPool,
                < 3 => _mediumAsteroidsPool,
                _ => _largeAsteroidsPool
            };
        }

        private Vector2 GetNewRotatedVector(Vector2 vector, float angle)
        {
            var piAngle = Mathf.PI * angle / 180f;

            var x = vector.x * Mathf.Cos(piAngle) - vector.y * Mathf.Sin(piAngle);
            var y = vector.x * Mathf.Sin(piAngle) + vector.y * Mathf.Cos(piAngle);

            return new Vector2(x, y);
        }
    }
}