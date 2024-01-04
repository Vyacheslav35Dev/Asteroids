using System;
using Interfaces;
using UnityEngine;

namespace Player.Weapon.Laser
{
    public class LaserSystem: ISystemUpdate, IDisposable
    {
        private readonly LaserView _view;
        private readonly float _laserDuration;

        public bool IsEmitted => _isEmitted;

        private bool _isEmitted;
        private float _laserTimer;

        public LaserSystem(LaserView view, float laserDuration)
        {
            _view = view;
            _laserDuration = laserDuration;
            _view.OnHit += OnLaserHit;
        }

        public void OnUpdate()
        {
            if(!_isEmitted)
                return;

            _laserTimer -= Time.deltaTime;

            if (_laserTimer <= 0)
            {
                _isEmitted = false;
                _view.gameObject.SetActive(false);
            }
        }

        public bool LaserFire()
        {
            if(_isEmitted)
                return false;

            _isEmitted = true;
            _laserTimer = _laserDuration;
            _view.gameObject.SetActive(true);

            return true;
        }

        private void OnLaserHit(IHitable laser, IHitable target)
        {
            target.Hit();
        }

        public void Dispose()
        {
            _view.OnHit -= OnLaserHit;
        }
    }
}