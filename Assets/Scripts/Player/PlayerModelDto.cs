using System;
using UnityEngine;

namespace Player
{
    public class PlayerModelDto
    {
        public Action<Vector2> PlayerChangePosition;
        public Action<float> PlayerChangeSpeed;
        public Action<float> PlayerChangeEuler;
        public Action<int> PlayerLaserCountChanged;
        public Action<float> PlayerLaserCooldownChanged;
        public Action PlayerDead;

        public bool IsPlayerAlive { get; private set; }

        public void Init()
        {
            IsPlayerAlive = true;
        }
        
        public void ChangePlayerPosition(Vector2 currentPosition)
        {
            PlayerChangePosition?.Invoke(currentPosition);
        }

        public void ChangePlayerSpeed(float speed)
        {
            PlayerChangeSpeed?.Invoke(speed);
        }

        public void ChangePlayerRotation(float angle)
        {
            PlayerChangeEuler?.Invoke(angle);
        }

        public void ChangePlayerLaserCount(int count)
        {
            PlayerLaserCountChanged?.Invoke(count);
        }

        public void ChangePlayerLaserCooldown(float cooldown)
        {
            PlayerLaserCooldownChanged?.Invoke(cooldown);
        }

        public void OnPlayerDead()
        {
            IsPlayerAlive = false;
            PlayerDead?.Invoke();
        }
    }
}