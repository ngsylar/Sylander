using System.Collections.Generic;
using UnityEngine;

public class PresenceDetector : MonoBehaviour
{
    public HouseBuilding house;

    [SerializeField] private bool isPredator;

    public bool present { get; set; }
    public bool entered { get; set; }

    void Start()
    {
        present = false;
        entered = false;
    }

    public bool IsOutside(Collider collider)
    {
        uint mask = house.GetMaskByCollider(collider);
        bool leaving = (mask & house.outside) > 0u;
        if (!leaving) return false;
        bool entering = (mask & house.inside) > 0u;
        return leaving && !entering;
    }

    public bool IsInside(Collider collider)
    {
        if (entered) return true;
        uint mask = house.GetMaskByCollider(collider);
        return (mask & house.inside) > 0u;
    }

    void OnTriggerEnter(Collider collider)
    {
        // present = true;
        // if (IsInside(collider))
        //     entered = true;
    }

    public List<int> GetRandomExplorationPath(int startNode)
    {
        List<int> path = new();
        bool[] visited = new bool[32];

        void DFS(int current) // funcao local recursiva para executar a DFS
        {
            visited[current] = true;
            path.Add(current); // chegou ao no

            // encontra todos os vizinhos ainda nao visitados
            List<int> unvisitedNeighbors = new();
            uint mask = house.adjs[current];
            
            for (int i = 0; i < 32; i++)
                if ((mask & (1u << (31 - i))) != 0) // bitwise a partir do bit mais significativo
                    if (!visited[i]) unvisitedNeighbors.Add(i);

            // embaralha a lista de vizinhos para garantir escolha aleatoria
            int n = unvisitedNeighbors.Count;
            while (n > 1) {
                n--;
                int k = Random.Range(0, n+1);
                (unvisitedNeighbors[n], unvisitedNeighbors[k])
                    = (unvisitedNeighbors[k], unvisitedNeighbors[n]);
            }
            // visita os vizinhos na ordem aleatoria
            foreach (int next in unvisitedNeighbors) {
                // verifica novamente se foi visitado caso tenha havido ciclos
                // explorados por outras ramificacoes desde que listamos os vizinhos
                if (!visited[next]) {
                    DFS(next);
                    // adiciona o no atual de novo ao caminho, 
                    // representando o backtrack da volta no grafo
                    path.Add(current);
                }
            }
        }
        // validacao basica do no inicial
        if (startNode >= 0 && startNode <= 31)
            DFS(startNode);

        return path;
    }
}
