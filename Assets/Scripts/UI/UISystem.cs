using System;
using Interfaces;
using Player;
using Score;
using UnityEngine;

namespace UI
{
    public class UISystem : IDisposable
    {
        private readonly UIView _view;
        private readonly GameScore _score;
        private readonly PlayerModelDto _playerModelDto;

        public UISystem(UIView view, GameScore score, PlayerModelDto playerModelDto)
        {
            _view = view;
            _score = score;
            _playerModelDto = playerModelDto;
        }

        public void Init()
        {
            _score.ScoreChanged += ChangeScoreText;

            _playerModelDto.PlayerChangePosition += OnPlayerChangePosition;
            _playerModelDto.PlayerChangeEuler += OnPlayerChangeEuler;
            _playerModelDto.PlayerChangeSpeed += OnPlayerChangeSpeed;
            _playerModelDto.PlayerLaserCooldownChanged += OnPlayerLaserCooldownChanged;
            _playerModelDto.PlayerLaserCountChanged += OnPlayerLaserCountChanged;
            _playerModelDto.PlayerDead += OnGameOver;
        }

        public void Clear()
        {
            ClearUi();
        }

        public void AddActionOnButtonClick(Action onClick, Action onClickRestart)
        {
            _view.StartGameButton.onClick.AddListener(onClick.Invoke);
            _view.StartGameButton.onClick.AddListener(OnGameStart);

            _view.RestartGameButton.onClick.AddListener(onClickRestart.Invoke);
            _view.RestartGameButton.onClick.AddListener(OnGameStart);
        }
        
        private void OnGameStart()
        {
            _view.StartGamePanel.SetActive(false);
            _view.GameplayPanel.SetActive(true);
            _view.EndGamePanel.SetActive(false);
        }

        private void OnPlayerLaserCountChanged(int count)
        {
            _view.PlayerLaserCountText.text = "";

            for (var i = 0; i < count; i++)
            {
                _view.PlayerLaserCountText.text += "▩";
            }
        }

        private void OnPlayerLaserCooldownChanged(float value)
        {
            var cooldown = Math.Round(value, 1);
            _view.PlayerLaserCooldownText.text = cooldown + "s";
        }

        private void OnPlayerChangeSpeed(float value)
        {
            var speed = Math.Round(value, 1);
            _view.PlayerSpeedText.text = speed.ToString();
        }

        private void OnPlayerChangeEuler(float value)
        {
            var angle = Math.Round(value, 1);
            _view.PlayerRotationText.text = angle.ToString();
        }

        private void OnPlayerChangePosition(Vector2 value)
        {
            var x = Math.Round(value.x, 1);
            var y = Math.Round(value.y, 1);
            _view.PlayerPositionText.text = "x = " + x + " y = " + y;
        }

        private void OnGameOver()
        {
            _view.EndGamePanel.SetActive(true);
            _view.EndGameScoreText.text = _view.ScoreText.text;
        }

        private void ClearUi()
        {
            _view.ScoreText.text = "0";
        }

        private void ChangeScoreText(int currentScore)
        {
            _view.ScoreText.text = currentScore.ToString();
        }

        public void Dispose()
        {
            _score.ScoreChanged -= ChangeScoreText;
            
            _playerModelDto.PlayerChangePosition -= OnPlayerChangePosition;
            _playerModelDto.PlayerChangeEuler -= OnPlayerChangeEuler;
            _playerModelDto.PlayerChangeSpeed -= OnPlayerChangeSpeed;
            _playerModelDto.PlayerLaserCooldownChanged -= OnPlayerLaserCooldownChanged;
            _playerModelDto.PlayerLaserCountChanged -= OnPlayerLaserCountChanged;
            _playerModelDto.PlayerDead += OnGameOver;
        }
    }
}