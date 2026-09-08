using UnityEngine;
using DwarfClone.Core;
using DwarfClone.World;
using DwarfClone.World.Chunk;

namespace DwarfClone.Entity.Animal
{
    public class AnimalSpawner : MonoBehaviour
    {
        public static AnimalSpawner Instance { get; private set; }

        private Transform animalContainer;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            GameObject cont = new GameObject("Animal_Entities");
            cont.transform.SetParent(transform);
            animalContainer = cont.transform;
        }

        public void SpawnWildlife()
        {
            var grid = WorldGrid.Instance;
            if (grid == null) return;

            int z = Constants.SURFACE_Z_LEVEL;

            // Spawn peaceful fauna
            SpawnGroup(AnimalType.Deer, 4, z);
            SpawnGroup(AnimalType.Rabbit, 6, z);

            // Spawn predators
            SpawnGroup(AnimalType.Wolf, 3, z);
            SpawnGroup(AnimalType.Bear, 2, z);
            SpawnGroup(AnimalType.BloodSpider, 3, z);

            Debug.Log("[AnimalSpawner] Wildlife populated across the world!");
        }

        private void SpawnGroup(AnimalType type, int count, int z)
        {
            var grid = WorldGrid.Instance;
            for (int i = 0; i < count; i++)
            {
                int rx = UnityEngine.Random.Range(10, Constants.WORLD_WIDTH - 10);
                int ry = UnityEngine.Random.Range(10, Constants.WORLD_HEIGHT - 10);

                if (grid.IsWalkable(rx, ry, z))
                {
                    GameObject animalObj = new GameObject($"Animal_{type}_{i}");
                    animalObj.transform.SetParent(animalContainer);
                    var ai = animalObj.AddComponent<AnimalAI>();
                    ai.Initialize(type, new Vector3Int(rx, ry, z));
                }
            }
        }
    }
}
