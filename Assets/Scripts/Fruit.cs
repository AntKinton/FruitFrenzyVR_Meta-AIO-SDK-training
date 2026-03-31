using Oculus.Interaction;
using Unity.VisualScripting;
using UnityEngine;
using EzySlice;

[RequireComponent (typeof(Rigidbody))]
public class Fruit : MonoBehaviour
{
    public Rigidbody rb;
    public Vector3 velocity;
    private float apexTimer = 0f;
    bool isAtApex = false;
    public float apexThreshold = 1f;
    [SerializeField] public bool isBomba = false; // bomboclatt

    [Header("Dynamic Slicing")]
    public Material crossSectionMaterial; // material para la tapa interior
    public float separationForce = 1.5f;

    [HideInInspector] public UnityEngine.Plane slicePlane;
    // Fruit particles
    public ParticleSystem juiceParticleEffect;
    public ParticleSystem sparkParticleEffect;
    public GameObject explosionEffect;
    public GameObject spawnSmokeEffect;

    public GameObject halfFruitPrefab;

    public float gravityScale = 0.5f;
    public float maxFallSpeed = -2.5f;

    public float upwardGravityReduction = 0.8f;

    [Tooltip("Duration to hover at apex")]
    public float apexHoverTime = 6f;

    [Tooltip("How much to reduce gravity at apex (0 = no gravity, 1 = full gravity)")]
    [Range(0f, 1f)]
    public float apexGravityMultiplier = 0.15f;

    [Tooltip("Air resistance while moving upward/downward")]
    public float upwardDrag = 0.1f;
    public float downwardDrag = 0.05f;
    public bool hasBeenSliced = false;
    public bool isDead = false;
    public bool isWhaleBonus = false;

    [HideInInspector] public GameObject originalPrefab;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("WholeFruit"), LayerMask.NameToLayer("Bamboo"), true);
    }


    public void ResetFruit()
    {
        hasBeenSliced = false;
        isDead = false;
        isWhaleBonus = false;
        isAtApex = false;
        apexTimer = 0f;

       // Limpiamos fuerzas anteriores para que no salga volando a lo loco
        rb.isKinematic = true;  // Forzamos a Unity a borrar la caché de físicas
        rb.isKinematic = false; // Lo volvemos a despertar
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        gameObject.layer = LayerMask.NameToLayer("WholeFruit");

        if (isBomba && sparkParticleEffect != null) sparkParticleEffect.Play();

        // El humo lo instanciamos y destruimos porque es un efecto rápido
        if (spawnSmokeEffect != null)
        {
            var smoke = Instantiate(spawnSmokeEffect, transform.position, Quaternion.identity);
            smoke.gameObject.SetActive(true);
            smoke.GetComponent<ParticleSystem>().Play();
            smoke.GetComponentInChildren<ParticleSystem>().Play();
            Destroy(smoke, 2f);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        velocity = rb.linearVelocity;
        if (GameManager.Instance.State == GameState.Lose) { isDead = true; }    
        if (velocity.y > 0) // Moving upward
        {
            ApplyUpwardPhysics();
        }
        else if (Mathf.Abs(velocity.y) < apexThreshold) // Near apex
        {
            ApplyApexPhysics();
        }
        else // Falling
        {
            ApplyFallingPhysics();
        }

        var clampedVelocity = rb.linearVelocity;
        clampedVelocity.y = Mathf.Max(clampedVelocity.y, maxFallSpeed);
        rb.linearVelocity = clampedVelocity;
    }

    private void ApplyUpwardPhysics()
    {
        isAtApex = false;
        apexTimer = 0f;

        // reduced gravity when moving up for floaty feel
        float reducedGravity = gravityScale * upwardGravityReduction;
        //Debug.Log($"upward gravity: {Physics.gravity * reducedGravity}");
        rb.AddForce(Physics.gravity * reducedGravity, ForceMode.Acceleration);

        // add upward air resistance
        Vector3 dragForce = -velocity.normalized * upwardDrag * velocity.sqrMagnitude;
        rb.AddForce(dragForce, ForceMode.Force);
    }

    private void ApplyApexPhysics()
    {
        if (!isAtApex)
        {
            isAtApex = true;
            apexTimer = 0f;
        }

        apexTimer += Time.fixedDeltaTime;

        if (apexTimer < apexHoverTime)
        {
            //Debug.Log($"apex gravity: {Physics.gravity * gravityScale * apexGravityMultiplier}");
            // reduced gravity at apex
            rb.AddForce(Physics.gravity * gravityScale * apexGravityMultiplier, ForceMode.Acceleration);

            // slight upward force to really suspend the fruit
            rb.AddForce(Vector3.up * 0.1f, ForceMode.Force);
        }
        else
        {
            //Debug.Log("applying falling physics from apex");
            // transition back to normal falling
            ApplyFallingPhysics();
        }
    }

    private void ApplyFallingPhysics()
    {
        isAtApex = false;

        // normal gravity when falling
        rb.AddForce(Physics.gravity * gravityScale * 0.3f, ForceMode.Acceleration);

        // light air resistance when falling
        Vector3 dragForce = -velocity.normalized * downwardDrag * velocity.sqrMagnitude;
        rb.AddForce(dragForce, ForceMode.Force);
    }

    public void Slice()
    {
        if (hasBeenSliced) return; // Seguro extra para evitar dobles cortes en el mismo frame
        hasBeenSliced = true;

        if (isBomba)
        {
            var explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            explosion.gameObject.SetActive(true);
            explosion.GetComponent<ParticleSystem>().Play();
            explosion.GetComponentInChildren<ParticleSystem>().Play(); 
            GameManager.Instance.State = GameState.Lose;
            AudioManager.instance.PlaySoundByName("BombExplode", false); 
            FruitSpawner.Instance.ReturnFruit(gameObject, originalPrefab); // Usando la piscina
            return;
        }
        else 
        {
            // --- GESTIÓN DEL COMBO ---
            if (GameManager.Instance != null && !isWhaleBonus)
            {
                GameManager.Instance.RegisterFishSliced();
            }

            if (GameManager.Instance.inComboWindow)
            {
                GameManager.Instance.comboCount++;
                GameManager.Instance.comboWindowTime = 0.5f; 
            }
            else
            {
                GameManager.Instance.comboCount++;
                GameManager.Instance.inComboWindow = true; 
                GameManager.Instance.comboWindowTime = 0.5f; 
            }

            AudioManager.instance.PlaySoundByName("FruitSlice", true);

            // --- EFECTO DE SANGRE (Con clonación para el Object Pool) ---
            if (juiceParticleEffect != null)
            {
                ParticleSystem juiceClone = Instantiate(juiceParticleEffect, transform.position, Quaternion.LookRotation(slicePlane.normal));
                juiceClone.transform.SetParent(null);
                juiceClone.gameObject.SetActive(true);
                juiceClone.Play();
                Destroy(juiceClone.gameObject, 2f);
            }

            // --- ¡LA MAGIA DE EZYSLICE! ---
            // Cortamos directamente el GameObject usando el material de la carne para el interior
            SlicedHull result = gameObject.Slice(transform.position, slicePlane.normal, crossSectionMaterial);

            if (result == null) return;
            
            // EzySlice nos crea directamente los dos GameObjects con sus mallas
            GameObject top = result.CreateUpperHull(gameObject, crossSectionMaterial);
            GameObject bottom = result.CreateLowerHull(gameObject, crossSectionMaterial);

            // Los configuramos
            SetupSlicedHalf(top, slicePlane.normal);
            SetupSlicedHalf(bottom, -slicePlane.normal);

            // Devolvemos el pez original a la piscina al siguiente frame
            StartCoroutine(ReturnNextFrame());
        }  
    }

    private System.Collections.IEnumerator ReturnNextFrame()
    {
        yield return null; 
        FruitSpawner.Instance.ReturnFruit(gameObject, originalPrefab);
    }

    // --- NUEVA VERSIÓN DE SPAWN HALF FRUIT ---
    void SetupSlicedHalf(GameObject go, Vector3 separationDir)
    {
        go.layer = LayerMask.NameToLayer("HalfFruit");

        // Igualamos posiciones
        go.transform.position = transform.position;
        go.transform.rotation = transform.rotation;
        go.transform.localScale = transform.localScale;

        // Físicas
        var mc = go.AddComponent<MeshCollider>();
        mc.convex = true;
        var rb2 = go.AddComponent<Rigidbody>();
        rb2.useGravity = false; // La gravedad la maneja HalfFruitRuntime

        var half = go.AddComponent<HalfFruitRuntime>();

        // Extraemos la malla y los materiales que EzySlice acaba de crear
        Mesh slicedMesh = go.GetComponent<MeshFilter>().sharedMesh;
        Material[] mats = go.GetComponent<MeshRenderer>().sharedMaterials;

        float currentSeparationForce = this.isWhaleBonus ? 0f : separationForce;
        Vector3 inheritedVel = this.isWhaleBonus ? Vector3.zero : rb.linearVelocity;
        
        // Inicializamos nuestro script de físicas
        half.Init(slicedMesh, inheritedVel, separationDir, currentSeparationForce, mats);

        // Herencia de poderes si es la ballena
        if (this.isWhaleBonus)
        {
            half.isWhaleBonusPiece = true;
            half.maxSliceGenerations = 6; 
            half.gravityScale = 0.03f; 
            
            // 1. Ampliamos un poco la grieta (de 2cm a 3cm)
            go.transform.position += separationDir * 0.03f; 
            
            // 2. ¡CONGELACIÓN INSTANTÁNEA! Evita que las físicas arruinen la grieta
            rb2.isKinematic = true; 
        }
    }

    public Vector3 GetRandAngVel()
    {
        float x = Random.Range(-2f, 2f);
        float y = Random.Range(-2f, 2f);
        float z = Random.Range(-2f, 2f);

        return new Vector3(x, y, z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log($"{gameObject.name} collision with {collision.gameObject.name} | This layer: {gameObject.layer}, Other layer: {collision.gameObject.layer}");
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Plane"))
        {
            if (rb.linearVelocity.y > 0.1f) return;

            if (!isBomba && !isWhaleBonus && GameManager.Instance.State == GameState.Play && !isDead)
            {
                GameManager.Instance.AddFail();
                AudioManager.instance.PlaySoundByName("Fail", false); // play fail sound
            }
            FruitSpawner.Instance.ReturnFruit(gameObject, originalPrefab);
        }
    }
    private void LateUpdate()
    {
        // nothing for now idk
    }
}
