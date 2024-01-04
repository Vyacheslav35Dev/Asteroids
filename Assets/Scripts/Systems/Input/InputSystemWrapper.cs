using System;
using Asteroids;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems.Input
{
    public class InputSystemWrapper : IDisposable
    {
        private readonly InputControls _input;

        public event Action FireButtonPress = delegate {  };
        public event Action LaserButtonPress = delegate {  }; 

        public InputSystemWrapper()
        {
            _input = new InputControls();
            _input.Enable();
            
            _input.Player.Shot.performed += Shot;
            _input.Player.Laser.performed += Laser;
        }
        private void Shot(InputAction.CallbackContext obj)
        {
            FireButtonPress.Invoke();
        }
        
        private void Laser(InputAction.CallbackContext obj)
        {
            LaserButtonPress.Invoke();
        }
       
        public Vector2 GetInputAxis()
        {
            Vector2 moveDirection = _input.Player.Move.ReadValue<Vector2>();
            
            var y = Mathf.Clamp(moveDirection.y, 0, 1);

            var inputDirection = new Vector2(moveDirection.x, y);
            
            return inputDirection;
        }

        public void Dispose()
        {
            _input?.Dispose();
        }
    }
}