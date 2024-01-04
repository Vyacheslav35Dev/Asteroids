using System.Collections.Generic;
using Asteroids;
using Data;
using Enemy;
using Interfaces;
using Player;
using Player.Weapon;
using Player.Weapon.Bullet;
using Player.Weapon.Laser;
using Score;
using Systems;
using Systems.Input;
using UI;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIView _uiView;
    
    [Header("VIEW")]
    [SerializeField] private PlayerView _playerView;
    [SerializeField] private BulletView _bulletView;
    [SerializeField] private UFOView _ufoView;
    
    [Header("Data")]
    [SerializeField] private PlayerData _playerData;
    [SerializeField] private GameplayData _gameplayData;
    [SerializeField] private AsteroidsData _asteroidsData;
    
    private InputSystemWrapper _inputSystem;
    private BulletSystem _bulletSystem;
    private LaserSystem _laserSystem;
    private OutOfSceneSystem _outOfSceneSystem;

    private PlayerSystem _playerSystem;
    private AsteroidsSystem _asteroidsSystem;
    private UFOSystem _ufoSystem;
    
    private UISystem _uiSystem;

    private GameScore _gameScore;
    private PlayerModelDto _playerModelDto;
    
    private List<ISystemUpdate> _systemUpdatables = new List<ISystemUpdate>();

    private void Start()
    {
        _gameScore = new GameScore();
        _playerModelDto = new PlayerModelDto();

        _uiSystem = new UISystem(_uiView, _gameScore, _playerModelDto);
        
        _uiSystem.AddActionOnButtonClick(StartGame, Restart);
        _uiSystem.Init();
        
        _inputSystem = new InputSystemWrapper();
    }
    
    private void StartGame()
    {
       var playerObject = Instantiate(_playerView);
       InitializeSystems(playerObject);
    }

    private void Restart()
    {
        Clear();
        StartGame();
    }
    
    private void InitializeSystems(PlayerView player)
    {
        _outOfSceneSystem = new OutOfSceneSystem(player);
        _bulletSystem = new BulletSystem(_bulletView, player, _outOfSceneSystem, _gameplayData);
        _laserSystem = new LaserSystem(player.Laser, _playerData.LaserDuration);
        _playerSystem = new PlayerSystem(player, _inputSystem, _bulletSystem, _playerModelDto, _playerData, _laserSystem);
        _ufoSystem = new UFOSystem(_ufoView, player, _outOfSceneSystem, _gameScore, _gameplayData);
        _asteroidsSystem = new AsteroidsSystem(_outOfSceneSystem, _asteroidsData, _gameScore, _gameplayData);
        
        _systemUpdatables.Add(_outOfSceneSystem);
        _systemUpdatables.Add(_bulletSystem);
        _systemUpdatables.Add(_laserSystem);
        _systemUpdatables.Add(_playerSystem);
        _systemUpdatables.Add(_ufoSystem);
        _systemUpdatables.Add(_asteroidsSystem);
    }
    
    private void Update()
    {
        _systemUpdatables?.ForEach(x => x.OnUpdate());
    }
    
    private void Clear()
    {
        _bulletSystem?.Dispose();
        _outOfSceneSystem?.Dispose();
        _asteroidsSystem?.Dispose();
        _playerSystem?.Dispose();
        _ufoSystem?.Dispose();
        _uiSystem?.Clear();

        _asteroidsSystem = null;
        _bulletSystem = null;
        _outOfSceneSystem = null;
        _playerSystem = null;
        _ufoSystem = null;
        _laserSystem = null;
        
        _gameScore.Clear();
        _systemUpdatables.Clear();
    }
}


