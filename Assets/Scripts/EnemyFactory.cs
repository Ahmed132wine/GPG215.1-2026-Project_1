using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    [Header("Enemy Templates")]
    public GameObject gruntPrefab;
    //public GameObject elitePrefab;
    //public GameObject medicPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnLocations;

    [Header("Spawner Settings")]
    public float spawnInterval = 3f;
    private float nextSpawnTime;

    private void Update()
    {
        // 5-minute continuous gameplay loop
        if (Time.timeSinceLevelLoad < 300f)
        {
            if (Time.time >= nextSpawnTime)
            {
                SpawnRandomEnemy();
                nextSpawnTime = Time.time + spawnInterval;
            }
        }
    }

    public void SpawnRandomEnemy()
    {
        if (spawnLocations.Length == 0) return;

        // Choose a random location
        Transform location = spawnLocations[Random.Range(0, spawnLocations.Length)];

        // Factory logic: choose what type to generate
        float roll = Random.value;
        GameObject enemyObject;

        if (roll < 0.6f) // 60% chance to spawn Grunt
        {
            enemyObject = Instantiate(gruntPrefab, location.position, Quaternion.identity);
            enemyObject.GetComponent<ShootingTarget>().type = ShootingTarget.TargetType.Grunt;
            enemyObject.GetComponent<ShootingTarget>().health = 1;
        }
        //else if (roll < 0.85f) // 25% chance to spawn Elite
        {
           // enemyObject = Instantiate(elitePrefab, location.position, Quaternion.identity);
           // enemyObject.GetComponent<ShootingTarget>().type = ShootingTarget.TargetType.Elite;
          //  enemyObject.GetComponent<ShootingTarget>().health = 3;
        }
        //else // 15% chance to spawn Medic
        {
            //enemyObject = Instantiate(medicPrefab, location.position, Quaternion.identity);
           // enemyObject.GetComponent<ShootingTarget>().type = ShootingTarget.TargetType.Medic;
           // enemyObject.GetComponent<ShootingTarget>().health = 2;
        }
    }
}
