using UnityEngine;

public class MirageDetector : MonoBehaviour
{
    [Header("Configuração de Filtro")]
    public LayerMask layerAlvo; // Selecione a Layer do objeto parado no Inspetor

    private MeshCollider meuMeshCollider;
    private bool estavaDentro = false;
    private Collider colliderAtual = null;

    public JumpscareMgmt flashlightPyramid;

    void Start()
    {
        meuMeshCollider = GetComponent<MeshCollider>();

        // Força a reconstrução do MeshCollider caso a malha tenha sido alterada
        if (meuMeshCollider != null) {
            Mesh mesh = meuMeshCollider.sharedMesh;
            meuMeshCollider.sharedMesh = null;
            meuMeshCollider.sharedMesh = mesh;
        }
    }

    void Update()
    {
        if (meuMeshCollider == null || meuMeshCollider.sharedMesh == null)
            return;

        // Usa os bounds atuais do collider
        Bounds bounds = meuMeshCollider.bounds;

        // 1. Faz uma checagem rápida ao redor da pirâmide para achar o objeto parado
        Collider[] colisoresProximos = Physics.OverlapBox(
            bounds.center,
            bounds.extents,
            transform.rotation, 
            layerAlvo
        );

        bool estaColidindoMeshExato = false;
        Collider outroAtual = null;

        // 2. Se achou o objeto por perto, testa a geometria exata da pirâmide contra ele
        foreach (Collider outroCollider in colisoresProximos)
        {
            Vector3 direcao;
            float distancia;

            // Esta função calcula se a geometria real dos dois meshes está se intersectando
            bool encostouDeVerdade = Physics.ComputePenetration(
                meuMeshCollider, transform.position, transform.rotation,
                outroCollider, outroCollider.transform.position, outroCollider.transform.rotation,
                out direcao, out distancia
            );

            if (encostouDeVerdade) {
                estaColidindoMeshExato = true;
                outroAtual = outroCollider;
                break;
            }
        }

        // 3. Gerencia o Trigger Enter de forma precisa
        if (estaColidindoMeshExato && !estavaDentro) {
            colliderAtual = outroAtual;
            OnCustomEnter(colliderAtual);
        }
        else if (!estaColidindoMeshExato && estavaDentro) {
            OnCustomExit(colliderAtual);
            colliderAtual = null;
        }

        estavaDentro = estaColidindoMeshExato;
    }

    void OnCustomEnter (Collider other)
    {
        if (other != null && other.gameObject.CompareTag("SlenderMirage")) {
            flashlightPyramid.MakeJumpscare(other.GetComponent<SlenderMirage>());
        }
    }

    void OnCustomExit (Collider other) {}
}
