using System;
using System.Collections.Generic;
using Components;
using Data;
using Interfaces;
using Player;
using Score;
using Systems;
using Systems.Pool;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Enemy
{
    public class UFOSystem : ISystemUpdate, IDisposable
    {
        private readonly OutOfSceneSystem _outOfSceneObjectSystem;
        private readonly PlayerView _playerView;
        private readonly GameScore _gameScore;
        private readonly UFOView _ufoView;

        private readonly Vector2 _spawnTimeRange;
        private readonly int _pointsForDestroy;
        private readonly float _speed;

        private PoolService<UFOView> _poolService;

        private Dictionary<UFOView, MoveComponent> _spawnedUfo = new();
        
        private float _timer;

        public UFOSystem(UFOView ufoView, PlayerView playerView, OutOfSceneSystem outOfSceneObjectSystem,
            GameScore gameScore, GameplayData gameplayData)
        {
            _ufoView = ufoView;
            _playerView = playerView;
            _outOfSceneObjectSystem = outOfSceneObjectSystem;
            _gameScore = gameScore;

            _speed = gameplayData.UfoSpeed;
            _spawnTimeRange = gameplayData.UfoSpawnTimeRange;
            _pointsForDestroy = gameplayData.PointsForUfoDestroy;
            
            _poolService = new PoolService<UFOView>(_ufoView);
        }
        
        public void Dispose()
        {
            foreach (var ufo in _spawnedUfo.Keys)
            {
                ufo.OnReturn -= OnReturnToPool;
                ufo.OnHit -= OnHit;
            }
            
            _poolService.ClearPool();
            _spawnedUfo.Clear();
        }

        public void OnUpdate()
        {
            SpawnUfo();
            MoveUfo();
        }

        private void SpawnUfo()
        {
            _timer -= Time.deltaTime;

            if (_timer <= 0)
            {
                var ufo = _poolService.GetItem();
                var position = _outOfSceneObjectSystem.GetRandomOutOfScenePosition();

                ufo.transform.position = position;
                ufo.gameObject.SetActive(true);

                _spawnedUfo.Add(ufo, new MoveComponent());
                _outOfSceneObjectSystem.AddItem(ufo);

                ufo.OnReturn += OnReturnToPool;
                ufo.OnHit += OnHit;

                _timer = Random.Range(_spawnTimeRange.x, _spawnTimeRange.y);
            }
        }

        private void MoveUfo()
        {
            foreach (var ufoParam in _spawnedUfo)
            {
                var ufo = ufoParam.Key;
                var parameters = ufoParam.Value;
                var ufoPosition = ufo.GetCurrentPosition();
                var towardDirection = (_playerView.GetCurrentPosition() - ufoPosition).normalized;
                var direction = parameters.Direction + towardDirection * Time.deltaTime;

                ufo.transform.position = Vector3.Lerp(ufoPosition,
                    ufoPosition + direction,
                    _speed * Time.deltaTime);

                parameters.ChangeDirection(direction);
            }
        }

        private void OnHit(IHitable ufoInterface, IHitable empty)
        {
            var ufo = (UFOView)ufoInterface;

            ufo.OnHit -= OnHit;
            ufo.ReturnToPool();

            _gameScore.IncreaseScore(_pointsForDestroy);
        }

        private void OnReturnToPool(UFOView ufo)
        {
            ufo.OnReturn -= OnReturnToPool;

            _spawnedUfo.Remove(ufo);
        }
    }
}