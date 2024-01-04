using UnityEngine;

namespace Components
{
    public class MoveComponent
    {
        public Vector2 Direction { get; private set; }

        public MoveComponent() { }

        public MoveComponent(Vector2 direction)
        {
            Direction = direction;
        }

        public void ChangeDirection(Vector2 direction)
        {
            Direction = direction;
        }
    }
}