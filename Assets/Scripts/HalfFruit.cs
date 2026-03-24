using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(MeshFilter), typeof(MeshCollider))]
public class HalfFruitRuntime : MonoBehaviour
{
    public float gravityScale = 1f;
    private Rigidbody rb;

    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("HalfFruit");
        rb = GetComponent<Rigidbody>();
    }

    // Llamado desde Fruit.Slice() justo después de instanciar
    public void Init(Mesh slicedMesh, Vector3 inheritedVelocity,
                     Vector3 separationDir, float separationForce,
                     Material[] materials)
    {
        // Asignar mesh cortado
        GetComponent<MeshFilter>().sharedMesh = slicedMesh;

        // MeshCollider convex para que Unity pueda calcular física
        var col = GetComponent<MeshCollider>();
        col.sharedMesh  = slicedMesh;
        col.convex      = true;

        // Materiales: [0] = skin del fruto, [1] = cross-section (tapa interior)
        GetComponent<MeshRenderer>().materials = materials;

        // Física
        rb.linearVelocity = inheritedVelocity;
        rb.AddForce(separationDir * separationForce, ForceMode.Impulse);
        rb.angularVelocity = new Vector3(
            Random.Range(-3f, 3f),
            Random.Range(-3f, 3f),
            Random.Range(-3f, 3f));
    }

    void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * gravityScale * 0.5f, ForceMode.Acceleration);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Plane"))
            Destroy(gameObject);
    }
}
