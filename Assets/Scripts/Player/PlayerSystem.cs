using System;
using Data;
using Interfaces;
using Player.Weapon.Bullet;
using Player.Weapon.Laser;
using Systems;
using Systems.Input;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Player
{
    public class PlayerSystem : ISystemUpdate, IDisposable
    {
        private readonly PlayerView _playerView;
        private readonly InputSystemWrapper _inputSystemWrapper;
        private readonly BulletSystem _bulletSystem;
        private readonly PlayerModelDto _playerModelDto;
        private readonly PlayerData _playerData;
        private readonly LaserSystem _laserSystem;

        private Vector2 _moveDirection;
        private float _bulletSpawnTimer;
        private float _laserCooldownTimer;

        private float _fireCooldown;
        private float _sensitivity;
        private float _speed;
        private int _laserCount;
        
        public PlayerSystem(PlayerView playerView, InputSystemWrapper inputSystemWrapper,
            BulletSystem bulletSystem, PlayerModelDto playerModelDto, PlayerData playerData,
            LaserSystem laserSystem)
        {
            _playerView = playerView;
            _inputSystemWrapper = inputSystemWrapper;
            _bulletSystem = bulletSystem;
            _playerModelDto = playerModelDto;
            _playerData = playerData;
            _laserSystem = laserSystem;
            
            _sensitivity = _playerData.Sensitivity;
            _speed = _playerData.Speed;
            _laserCount = _playerData.LaserCount;
            _fireCooldown = _playerData.FireCooldown;

            _playerModelDto.Init();
            _playerModelDto.ChangePlayerLaserCount(_laserCount);

            _inputSystemWrapper.FireButtonPress += OnFireButtonPress;
            _inputSystemWrapper.LaserButtonPress += OnLaserButtonPress;

            _playerView.OnHit += OnPlayerHit;
        }

        public void Dispose()
        {
            _inputSystemWrapper.FireButtonPress -= OnFireButtonPress;
            _inputSystemWrapper.LaserButtonPress -= OnLaserButtonPress;

            _playerView.OnHit -= OnPlayerHit;
            
            Object.Destroy(_playerView.gameObject);
        }

        public void OnUpdate()
        {
            if(!_playerModelDto.IsPlayerAlive)
                return;
            
            var inputDirection = _inputSystemWrapper.GetInputAxis();

            CheckBulletTimer();
            CheckLaserCount();
            Move(inputDirection.y);
            Rotate(inputDirection.x);
        }

        private void CheckLaserCount()
        {
            if (_laserCount >= _playerData.LaserCount || _laserSystem.IsEmitted)
                return;

            _laserCooldownTimer -= Time.deltaTime;

            _playerModelDto.ChangePlayerLaserCooldown(_laserCooldownTimer);

            if (!(_laserCooldownTimer <= 0))
                return;

            _laserCount++;
            _laserCooldownTimer = 0;

            _playerModelDto.ChangePlayerLaserCooldown(_laserCooldownTimer);
            _playerModelDto.ChangePlayerLaserCount(_laserCount);

            _laserCooldownTimer = _playerData.LaserCooldown;
        }

        private void CheckBulletTimer()
        {
            if (_bulletSpawnTimer <= 0 || _laserSystem.IsEmitted)
                return;

            _bulletSpawnTimer -= Time.deltaTime;
        }

        private void OnLaserButtonPress()
        {
            if (_laserCount <= 0)
            {
                return;
            }

            if (_laserSystem.LaserFire())
            {
                _laserCount--;
                _playerModelDto.ChangePlayerLaserCount(_laserCount);
                _laserCooldownTimer = _playerData.LaserCooldown;
            }
        }

        private void OnFireButtonPress()
        {
            if (_bulletSpawnTimer > 0)
            {
                return;
            }

            _bulletSystem.Spawn();
            _bulletSpawnTimer = _fireCooldown;
        }

        private void Move(float moveValue)
        {
            var playerTransform = _playerView.transform;
            var playerPosition = playerTransform.position;
           
            if (moveValue != 0)
            {
                var towardDirection = playerTransform.up * moveValue;
                _moveDirection += (Vector2)towardDirection * Time.deltaTime;
            }

            playerTransform.position = Vector3.Lerp(playerPosition,
                playerPosition + (Vector3)_moveDirection,
                _speed * Time.deltaTime);

            _playerModelDto.ChangePlayerPosition(playerTransform.position);
            _playerModelDto.ChangePlayerSpeed(_moveDirection.magnitude);
        }

        private void Rotate(float rotateAngle)
        {
            _playerView.transform.eulerAngles -= Vector3.forward * rotateAngle * _sensitivity * Time.deltaTime;
            _playerModelDto.ChangePlayerRotation(_playerView.transform.eulerAngles.z);
        }

        private void OnPlayerHit(IHitable player, IHitable target)
        {
            _playerView.gameObject.SetActive(false);
            _playerModelDto.OnPlayerDead();

            target.Hit();
        }
    }
}