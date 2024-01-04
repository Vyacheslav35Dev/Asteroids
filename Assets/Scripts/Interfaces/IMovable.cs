﻿using UnityEngine;

namespace Interfaces
{
    public interface IMovable
    {
        Vector2 GetCurrentPosition();
        void OnOutOfScene();
    }
}