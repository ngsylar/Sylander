using System.Collections.Generic;
using UnityEngine;

public class PresenceHunter : MonoBehaviour
{
    public HouseBuilding house;
    [SerializeField, ReadOnly] private Collider currentPlacement;

    private bool inside = false;
    private bool inFront = false;
    private bool behind = false;

    public bool IsInside
    {
        get => inside;
    }

    public bool IsOutside
    {
        get => !inside && (behind || inFront);
    }

    public bool IsPresent
    {
        get => inside || behind || inFront;
    }

    public int CurrentPlaceIndex
    {
        get => house.GetIndexByCollider(currentPlacement);
    }

    public Collider CurrentPlacement
    {
        get => currentPlacement;
    }

    void OnTriggerEnter(Collider collider)
    {
        if (house.inside.Includes(collider))
            inside = true;
        else if (collider == house.back)
            behind = true;
        else if (collider == house.front)
            inFront = true;
    }

    void OnTriggerStay(Collider collider)
    {
        if (inside) {
            if (house.inside.Includes(collider))
                currentPlacement = collider;
        }
        else if (behind) currentPlacement = house.back;
        else if (inFront) currentPlacement = house.front;
    }

    void OnTriggerExit(Collider collider)
    {
        if ((collider == house.entrance || collider == house.exit)
            && (transform.position.x >= house.entrance.bounds.max.x
            || transform.position.x <= house.exit.bounds.min.x)) {
            inside = false;
        }
        else if (collider == house.back) behind = false;
        else if (collider == house.front) inFront = false;
        if (!inside && !behind && !inFront)
            currentPlacement = null;
    }

    // precisa teleportar para funcionar corretamente
    public void ForceExit()
    {
        inside = false;
    }

    // sylar: depois explorar outros algoritmos para as demais paginas
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
