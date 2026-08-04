using UnityEngine;

public class MirageDetector : MonoBehaviour
{
    [Header("Configuração de Filtro")]
    public LayerMask layerAlvo; // Selecione a Layer do objeto parado no Inspetor

    private MeshCollider meuMeshCollider;
    private bool estavaDentro = false;

    public JumpscareMgmt flashlightPyramid;

    void Start()
    {
        meuMeshCollider = GetComponent<MeshCollider>();
    }

    void Update()
    {
        if (meuMeshCollider == null) return;

        // 1. Faz uma checagem rápida ao redor da pirâmide para achar o objeto parado
        Collider[] colisoresProximos = Physics.OverlapBox(
            transform.position, 
            meuMeshCollider.bounds.extents, 
            transform.rotation, 
            layerAlvo
        );

        bool estaColidindoMeshExato = false;
        Collider other = new Collider();

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

            if (encostouDeVerdade)
            {
                estaColidindoMeshExato = true;
                other = outroCollider;
                break;
            }
        }

        // 3. Gerencia o Trigger Enter de forma precisa
        if (estaColidindoMeshExato && !estavaDentro)
        {
            OnCustomEnter(other);
        }
        else if (!estaColidindoMeshExato && estavaDentro)
        {
            OnCustomExit(other);
        }

        estavaDentro = estaColidindoMeshExato;
    }

    void OnCustomEnter (Collider other)
    {
        if (other.gameObject.CompareTag("SlenderMirage")) {
            flashlightPyramid.MakeJumpscare(other.GetComponent<SlenderMirage>());
        }
    }

    void OnCustomExit (Collider other) {}
}
