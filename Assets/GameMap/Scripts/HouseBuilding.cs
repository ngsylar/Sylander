using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class HouseBuilding : MonoBehaviour
{
    [Header("You do\'nt want to mess with Nodes")]
    public List<Collider> nodes;

    public Collider frente;
    public Collider entrada;
    public Collider saida;
    public Collider tras;
    
    // readonly List<uint> places = new();
    public readonly uint[] adjs = new uint[]
    {//   oi   o e     d      d     e
    //    fescsae012345012345d012345012345
        0b01000000000000000000000000000000u, // 00 frente
        0b10100000000000000000000000000000u, // 01 entrada
        0b01010010000000000001000000000000u, // 02 salao
        0b00101000000000000000000000000000u, // 03 corredor
        0b00010100000000000000000000000000u, // 04 saida
        0b00001000000000000000000000000000u, // 05 atras
        0b00100001000000000000000000000000u, // 06 esquerda
        0b00000010100001000000000000000000u, // 07 ee0
        0b00000001010000000000000000000000u, // 08 ee1
        0b00000000101000000000000000000000u, // 09 ee2
        0b00000000010100000000000000000000u, // 10 ee3
        0b00000000001010000000000000000000u, // 11 ee4
        0b00000000000100000000000000000000u, // 12 ee5
        0b00000001000000100000000000000000u, // 13 ed0
        0b00000000000001010000000000000000u, // 14 ed1
        0b00000000000000101000000000000000u, // 15 ed2
        0b00000000000000010100000000000000u, // 16 ed3
        0b00000000000000001010000000000000u, // 17 ed4
        0b00000000000000000100000000000000u, // 18 ed5
        0b00100000000000000000100000000000u, // 19 direita
        0b00000000000000000001010000100000u, // 20 dd0
        0b00000000000000000000101000000000u, // 21 dd1
        0b00000000000000000000010100000000u, // 22 dd2
        0b00000000000000000000001010000000u, // 23 dd3
        0b00000000000000000000000101000000u, // 24 dd4
        0b00000000000000000000000010000000u, // 25 dd5
        0b00000000000000000000100000010000u, // 26 de0
        0b00000000000000000000000000101000u, // 27 de1
        0b00000000000000000000000000010100u, // 28 de2
        0b00000000000000000000000000001010u, // 29 de3
        0b00000000000000000000000000000101u, // 30 de4
        0b00000000000000000000000000000010u  // 31 de5
    };
    //                                    0         1         2         3
    //                                    01234567890123456789012345678901
    public readonly uint outside      = 0b10000100000000000000000000000000u;
    public readonly uint inside       = 0b01111011111111111111111111111111u;

    // public readonly uint insideLeft   = 0b00000011111111111110000000000000u;
    // public readonly uint insideRight  = 0b00000000000000000001111111111111u;
    // public readonly uint hallway      = 0b01111000000000000000000000000000u;
    // public readonly uint wichWay      = 0b00000001111111111110111111111111u;
    // public readonly uint hidden       = 0b00000000001110001110000111000111u;

    // void Awake()
    // {
    //     int i = 0;
    //     uint tmp = 0b10000000000000000000000000000000u;
    //     while (i < 32) places.Add(tmp >> i++);

    //     #if UNITY_EDITOR
    //     foreach (uint place in places)
    //         Debug.Log(System.Convert.ToString(place, 2).PadLeft(32, '0'));
    //     #endif
    // }

    void Start()
    {
        enabled = false;
    }

    public int MaskToIndex (uint mask)
    {
        return math.lzcnt(mask);
    }

    public uint IndexToMask (int index)
    {
        uint mask = 1u << (31 - index);
        return mask;
    }

    public int GetIndexByCollider (Collider collider)
    {
        return nodes.IndexOf(collider);
    }

    public uint GetMaskByCollider (Collider collider)
    {
        int index = nodes.IndexOf(collider);
        uint mask = 1u << (31 - index);
        return mask;
    }

    public Collider GetColliderByIndex (int index)
    {
        return nodes[index];
    }

    public Collider GetColliderByMask (uint mask)
    {
        int index = math.lzcnt(mask);
        return nodes[index];
    }
}
