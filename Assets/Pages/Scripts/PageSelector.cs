using System.Collections.Generic;
using UnityEngine;

public class PageSelector : MonoBehaviour
{
    public List<Material> textures;
    public List<GameObject> pages1;
    public List<GameObject> pages2;
    public List<GameObject> pages3;
    public List<GameObject> pages4;
    public List<GameObject> pages5;
    public List<GameObject> pages6;
    public List<GameObject> pages7;
    public List<GameObject> pages8;
    public List<GameObject> pages9;
    public List<GameObject> pages10;

    void Awake() {
        List<int> lista = new();
        for (int i = 0; i < 10; i++)
            lista.Add(i);

        for (int i = 0; i < lista.Count; i++)
        {
            int randIndex = Random.Range(i, lista.Count);
            (lista[randIndex], lista[i]) = (lista[i], lista[randIndex]);
        }
        List<int> selectedAreas = lista.GetRange(0, 7);

        List<List<GameObject>> areas = new()
        {
            pages1,
            pages2,
            pages3,
            pages4,
            pages5,
            pages6,
            pages7,
            pages8,
            pages9,
            pages10
        };

        foreach (int area in selectedAreas)
        {
            int page = Random.Range(0, areas[area].Count);
            areas[area][page].SetActive(true);
        }

        foreach (int area in lista)
        {
            for (int page=areas[area].Count-1; page>=0; page--)
            {
                if (!areas[area][page].activeSelf)
                    Destroy(areas[area][page], 0.1f);
            }
        }
        enabled = false;
    }
}
