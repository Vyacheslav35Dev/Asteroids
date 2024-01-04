﻿using System;

namespace Score
{
    public class GameScore
    {
        public event Action<int> ScoreChanged = delegate { };

        private int _score = 0;
        
        public void IncreaseScore(int a)
        {
            _score += a;
            
            ScoreChanged.Invoke(_score);
        }

        public void Clear()
        {
            _score = 0;
        }
    }
}