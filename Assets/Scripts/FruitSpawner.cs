using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;


public class FruitSpawner : MonoBehaviour
{
    [Header("Fruit Prefabs")]
    public GameObject[] WholeFruits;
    // spawn fruit object with wholelayer


    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public Vector3 velocityToSet;
    public float minSpawnInterval = 0.2f; // Minimum spawn interval
    public float maxSpawnInterval = 2f;   // Maximum spawn interval

    [Header("Group Spawn Settings")]
    public bool useGroupSpawning = true;
    public int minFruitsPerGroup = 2;
    public int maxFruitsPerGroup = 4;
    public float bombProbability = 0.2f; // preset 20% chance of spawning a bomb per group (old) 
    public float groupSpawnDelay = 0.1f; // Delay between spawning fruits in a group

    [Header("Dynamic Difficulty Settings")]
    public bool useDynamicDifficulty = true;
    public int baseMinFruits = 1; // Starting minimum fruits
    public int baseMaxFruits = 2; // Starting maximum fruits
    public int maxMinFruits = 3;  // Maximum minimum fruits (at high scores)
    public int maxMaxFruits = 5;  // Maximum maximum fruits (at high scores)
    public int scoreThreshold = 100; // Score needed to reach max difficulty
    public float baseBombProbability = 0.1f; // Starting bomb probability (10%)
    public float maxBombProbability = 0.25f;  // Maximum bomb probability (30%)

    private Coroutine spawnCoroutine;

    [Header("Bonus Phase Settings")]
    public GameObject whalePrefab; // Arrastra tu prefab de Ballena aquí en el Inspector

    // --- SISTEMA DE OBJECT POOLING ---
    public static FruitSpawner Instance;
    private Dictionary<GameObject, Queue<GameObject>> fruitPools = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartSpawning();
    }

    public GameObject GetFruit(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!fruitPools.ContainsKey(prefab)) fruitPools[prefab] = new Queue<GameObject>();

        GameObject fruitObj;
        if (fruitPools[prefab].Count > 0)
        {
            // Sacamos un pez reciclado
            fruitObj = fruitPools[prefab].Dequeue();
            fruitObj.transform.position = position;
            fruitObj.transform.rotation = rotation;
            fruitObj.SetActive(true);
        }
        else
        {
            // Si no hay reciclados, creamos uno nuevo por primera vez
            fruitObj = Instantiate(prefab, position, rotation);
            Fruit f = fruitObj.GetComponent<Fruit>();
            if (f != null) f.originalPrefab = prefab; // Le decimos de qué familia viene
        }

        // Reseteamos sus físicas para que vuelva a estar "fresco"
        Fruit fruitScript = fruitObj.GetComponent<Fruit>();
        if (fruitScript != null) fruitScript.ResetFruit();

        return fruitObj;
    }

    public void ReturnFruit(GameObject fruitObj, GameObject prefab)
    {
        // 1. EL SEGURO: Si ya está apagado, ignóralo para evitar duplicados
        if (!fruitObj.activeSelf) return;
        
        fruitObj.SetActive(false); // Lo apagamos en lugar de destruirlo
        if (prefab == null) 
        {
            Destroy(fruitObj); return; // Seguridad
        }

        if (!fruitPools.ContainsKey(prefab)) fruitPools[prefab] = new Queue<GameObject>();
        fruitPools[prefab].Enqueue(fruitObj); // Lo metemos en la pila
    }

    public void StartSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnFruitRoutine());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    public void SpawnWhaleBonus()
    {
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Elevamos a la ballena 1.5 metros extra al nacer para compensar su tamaño
        Vector3 safeSpawnPos = randomSpawnPoint.position + new Vector3(0, 1.5f, 0);
        GameObject whale = GetFruit(whalePrefab, safeSpawnPos, transform.rotation);
        
        Rigidbody rb = whale.GetComponent<Rigidbody>();
        Fruit fruitScript = whale.GetComponent<Fruit>();

        if (rb != null)
        {
            // Usamos la misma fuerza que los peces normales para que suba a la misma altura
            rb.linearVelocity = new Vector3(Random.Range(-0.2f, 0.2f), 2f, 0f); 
            rb.angularVelocity = GetRandAngVel() * 0.1f; 
        }

        if (fruitScript != null)
        {
            // ¡EL TRUCO DE MATRIX!
            // 1. Dejamos su gravityScale normal para que suba y frene de forma realista.
            
            // 2. Le decimos que cuando alcance su punto más alto (Apex), se quede ahí 8 segundos
            fruitScript.apexHoverTime = 8f; 
            
            // 3. Le quitamos la gravedad SOLO mientras está flotando arriba
            fruitScript.apexGravityMultiplier = 0f; 
            
            fruitScript.maxFallSpeed = -0.5f; 
            fruitScript.isWhaleBonus = true;  
        }
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private void OnEnable()
    {
        StartSpawning();
    }

    private IEnumerator SpawnFruitRoutine()
    {
        while (true)
        {
            // wait for a rand interval between min and max
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);

            // Choose between single spawn or group spawn
            if (useGroupSpawning)
            {
                yield return StartCoroutine(SpawnFruitGroup());
            }
            else
            {
                float currentBombProb = GetCurrentBombProbability();
                if (Random.value < currentBombProb)
                {
                    SpawnBomb();
                }
                else SpawnFruit();
            }
        }
    }

    private IEnumerator SpawnFruitGroup()
    {
        // Get dynamic values based on current score from GameManager
        int minFruits, maxFruits;
        float currentBombProb;
        GetDynamicSpawnValues(out minFruits, out maxFruits, out currentBombProb);

        int fruitsToSpawn = Random.Range(minFruits, maxFruits + 1);

        // spawn fruits in the group
        for (int i = 0; i < fruitsToSpawn; i++)
        {
            SpawnFruit();
            if (i < fruitsToSpawn - 1) // Don't wait after the last fruit
            {
                yield return new WaitForSeconds(groupSpawnDelay);
            }
        }

        // Check if we should spawn a bomb
        if (Random.value < currentBombProb)
        {
            yield return new WaitForSeconds(groupSpawnDelay);
            SpawnBomb();
        }
    }

    public void SpawnFruit()
    {
        // Check if we have fruit prefabs
        if (WholeFruits == null || WholeFruits.Length == 0)
        {
            Debug.LogWarning("No fruit prefabs assigned to FruitSpawner!");
            return;
        }
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned!");
            return;
        }

        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject selectedFruit = WholeFruits[Random.Range(0, WholeFruits.Length-1)]; // not including bomb

        GameObject fruit = GetFruit(selectedFruit, randomSpawnPoint.position, transform.rotation);
        Rigidbody rb = fruit.GetComponent<Rigidbody>();

        // adjust fruits gravity
        if (rb != null)
        {
            rb.angularVelocity = GetRandAngVel();

            rb.linearVelocity = velocityToSet.magnitude > 0 ? velocityToSet : new Vector3(Random.Range(-0.2f, 0.2f), 3f, 0f);
        }
    }

    public void SpawnBomb()
    {
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject selectedBomb = WholeFruits[6];
        GameObject bomb = GetFruit(selectedBomb, randomSpawnPoint.position, transform.rotation);
        Rigidbody rb = bomb.GetComponent<Rigidbody>();
        // Apply same physics as fruits
        if (rb != null)
        {
            rb.angularVelocity = GetRandAngVel();
            rb.linearVelocity = velocityToSet.magnitude > 0 ? velocityToSet : new Vector3(Random.Range(-0.2f, 0.2f), 3f, 0f);
        }
    }

    // Method to calculate dynamic spawn values based on current score from GameManager
    private void GetDynamicSpawnValues(out int minFruits, out int maxFruits, out float currentBombProb)
    {
        if (useDynamicDifficulty && GameManager.Instance != null && scoreThreshold > 0)
        {
            // Get current score from GameManager
            int currentScore = GameManager.Instance.score;

            // Calculate difficulty progress (0.0 to 1.0)
            float difficultyProgress = Mathf.Clamp01((float)currentScore / scoreThreshold);

            // Interpolate fruit counts
            minFruits = Mathf.RoundToInt(Mathf.Lerp(baseMinFruits, maxMinFruits, difficultyProgress));
            maxFruits = Mathf.RoundToInt(Mathf.Lerp(baseMaxFruits, maxMaxFruits, difficultyProgress));

            // Interpolate bomb probability
            currentBombProb = Mathf.Lerp(baseBombProbability, maxBombProbability, difficultyProgress);
        }
        else
        {
            // Use static values
            minFruits = minFruitsPerGroup;
            maxFruits = maxFruitsPerGroup;
            currentBombProb = bombProbability;
        }
    }

    // Get current bomb probability for single spawning
    private float GetCurrentBombProbability()
    {
        if (useDynamicDifficulty && GameManager.Instance != null && scoreThreshold > 0)
        {
            int currentScore = GameManager.Instance.score;
            float difficultyProgress = Mathf.Clamp01((float)currentScore / scoreThreshold);
            return Mathf.Lerp(baseBombProbability, maxBombProbability, difficultyProgress);
        }
        return bombProbability;
    }

    // Public method to get current difficulty info (for debugging or UI)
    public string GetDifficultyInfo()
    {
        int minFruits, maxFruits;
        float currentBombProb;
        GetDynamicSpawnValues(out minFruits, out maxFruits, out currentBombProb);

        int currentScore = GameManager.Instance != null ? GameManager.Instance.score : 0;
        float progress = useDynamicDifficulty ? Mathf.Clamp01((float)currentScore / scoreThreshold) : 0f;

        return $"Score: {currentScore} | Difficulty: {progress:P0} | Fruits: {minFruits}-{maxFruits} | Bomb Chance: {currentBombProb:P0}";
    }

    public Vector3 GetRandAngVel()
    {
        float x = Random.Range(-3f, 3f);
        float y = Random.Range(-3f, 3f);
        float z = Random.Range(-3f, 3f);

        return new Vector3(x, y, z);
    }

    public Vector3 GetRandSpawnPos()
    {
        float x = Random.Range(-1.5f, 1.5f);
        float y = transform.position.y;
        float z = Random.Range(-1.77f, -1.3f);

        return new Vector3(x, y, z);
    }

    public Quaternion GetRandSpawnRot()
    {
        float x = Random.Range(0f, 360f);
        float y = Random.Range(0f, 360f);
        float z = Random.Range(0f, 360f);

        return Quaternion.Euler(x, y, z);
    }


    // Update is called once per frame
    void Update()
    {
        /*if (OVRInput.GetDown(OVRInput.Button.One))
        {
            SpawnFruit();
        }*/

    }
}
