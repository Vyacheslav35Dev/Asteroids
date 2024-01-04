using Enemy;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "AsteroidsData", menuName = "ScriptableObjects/AsteroidsData", order = 1)]
    public class AsteroidsData: ScriptableObject
    {
        [field: SerializeField] public float DestroyedAsteroidChangeAngle = 30;
        [field: SerializeField] public AsteroidView LittleAsteroid{ get; private set; } 
        [field: SerializeField] public AsteroidView MediumAsteroid { get; private set; }
        [field: SerializeField] public AsteroidView LargeAsteroid { get; private set; }
    }
}