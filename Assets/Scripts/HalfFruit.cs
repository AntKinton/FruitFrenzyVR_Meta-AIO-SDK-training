using UnityEngine;
using EzySlice;

[RequireComponent(typeof(Rigidbody), typeof(MeshFilter), typeof(MeshCollider))]
public class HalfFruitRuntime : MonoBehaviour
{
    public float gravityScale = 1f;
    private Rigidbody rb;
    
    // --- CONTROL DE FILETEADO ---
    public bool hasBeenSliced = false;
    public int sliceGeneration = 1; 
    
    // 1. VOLVEMOS A PONER ESTO EN 1 POR DEFECTO (Para los peces normales)
    public int maxSliceGenerations = 1; 
    
    public bool isWhaleBonusPiece = false;
    private bool hasUnfrozen = false;
    // -----------------------------------

    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("HalfFruit");
        rb = GetComponent<Rigidbody>();
        
        // 2. SEGURO DE VIDA EXTRA: Destruir el trozo a los 4 segundos 
        // por si se queda rebotando o no toca el suelo, así limpiamos la memoria.
        Destroy(gameObject, 4f);
    }

    public void Init(Mesh slicedMesh, Vector3 inheritedVelocity,
                     Vector3 separationDir, float separationForce,
                     Material[] materials)
    {
        GetComponent<MeshFilter>().sharedMesh = slicedMesh;

        var col = GetComponent<MeshCollider>();
        col.sharedMesh  = slicedMesh;
        col.convex      = true;

        GetComponent<MeshRenderer>().materials = materials;

        rb.linearVelocity = inheritedVelocity;
        rb.AddForce(separationDir * separationForce, ForceMode.Impulse);
        
        // Si hay fuerza de separación (pez normal), gira. Si es 0 (ballena), no gira nada.
        if (separationForce > 0f)
        {
            rb.angularVelocity = new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), Random.Range(-3f, 3f));
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void Slice(UnityEngine.Plane slicePlane)
    {
        // 1. El muro de seguridad
        if (!isWhaleBonusPiece || hasBeenSliced || sliceGeneration >= maxSliceGenerations) return;
        hasBeenSliced = true;

        // Recuperamos el material de la carne original de los materiales actuales
        Material[] currentMats = GetComponent<MeshRenderer>().sharedMaterials;
        Material fleshMat = currentMats.Length > 1 ? currentMats[1] : currentMats[0];

        // 2. ¡LA MAGIA DE EZYSLICE! Hace todo el cálculo matemático perfecto
        SlicedHull result = gameObject.Slice(transform.position, slicePlane.normal, fleshMat);

        if (result != null)
        {
            // Creamos los GameObjects con las mallas ya generadas
            GameObject top = result.CreateUpperHull(gameObject, fleshMat);
            GameObject bottom = result.CreateLowerHull(gameObject, fleshMat);

            // Los convertimos en nuestros "HalfFruits"
            SetupNextHalf(top, slicePlane.normal, currentMats);
            SetupNextHalf(bottom, -slicePlane.normal, currentMats);

            // --- PUNTOS Y COMBO ---
            if (GameManager.Instance != null)
            {
                GameManager.Instance.score += 5; 
                GameManager.Instance.comboCount += 2; // +2 por ser ballena
                GameManager.Instance.inComboWindow = true;
                GameManager.Instance.comboWindowTime = 0.5f;
                AudioManager.instance.PlaySoundByName("FruitSlice", true);
            }
        }

        Destroy(gameObject);
    }

    void SetupNextHalf(GameObject go, Vector3 separationDir, Material[] mats)
    {
        go.layer = LayerMask.NameToLayer("HalfFruit");
        
        // Replicamos el transform
        go.transform.position = transform.position;
        go.transform.rotation = transform.rotation;
        go.transform.localScale = transform.localScale;

        // Añadimos físicas
        var mc = go.AddComponent<MeshCollider>();
        mc.convex = true;
        var rb2 = go.AddComponent<Rigidbody>();
        rb2.useGravity = false;

        // Añadimos nuestro script para que la herencia continúe
        var half = go.AddComponent<HalfFruitRuntime>();
        half.sliceGeneration = this.sliceGeneration + 1;
        half.isWhaleBonusPiece = true;
        half.maxSliceGenerations = this.maxSliceGenerations;
        half.gravityScale = this.gravityScale;
        
        // Inicializamos con fuerza nula para que no salgan volando
        half.Init(go.GetComponent<MeshFilter>().sharedMesh, Vector3.zero, separationDir, 0f, mats);
    }

    // ¡CRÍTICO PARA EL RENDIMIENTO! Limpia la memoria RAM al destruirse
    private void OnDestroy()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            Destroy(mf.sharedMesh); // Borra la malla visual de la RAM
        }

        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc != null && mc.sharedMesh != null)
        {
            Destroy(mc.sharedMesh); // Borra la malla de físicas de la RAM
        }
    }

    void FixedUpdate()
    {
        // 1. MODO MATRIX: Si es la ballena y el tiempo del combo sigue activo
        if (isWhaleBonusPiece && GameManager.Instance != null && GameManager.Instance.isWhaleBonusPhase)
        {
            // isKinematic hace que la pieza sea sorda y ciega a la gravedad y a los choques. ¡Estatua total!
            if (!rb.isKinematic) rb.isKinematic = true; 
        }
        else
        {
            // 2. EL ESTALLIDO ANIME: Si el tiempo se acabó y estaba congelada
            if (isWhaleBonusPiece && !hasUnfrozen)
            {
                hasUnfrozen = true;
                rb.isKinematic = false; // Le devolvemos las físicas
                rb.useGravity = true;
                
                // Calculamos una dirección aleatoria y la empujamos para que el puzzle estalle
                Vector3 randomDir = Random.onUnitSphere;
                rb.AddForce(randomDir * 3.5f, ForceMode.Impulse);
                rb.angularVelocity = new Vector3(Random.Range(-6f, 6f), Random.Range(-6f, 6f), Random.Range(-6f, 6f));
            }

            // 3. Física normal para los trozos que están cayendo
            if (!rb.isKinematic)
            {
                rb.AddForce(Physics.gravity * gravityScale * 0.5f, ForceMode.Acceleration);
            }
        }
    }
}
