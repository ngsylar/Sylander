using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class HouseBuilding : MonoBehaviour
{
    [Header("You do\'nt want to mess with Nodes")]
    public List<Collider> nodes;

    private const int MAX_INDEXES = 32;
    private const int MAX_LEVELS = 32;
    
    public uint[] adjs { get; private set; }

    public Collider front { get; private set; }
    public Collider entrance { get; private set; }
    public Collider exit { get; private set; }
    public Collider back { get; private set; }

    public HousePlace outside { get; private set; }
    public HousePlace inside { get; private set; }

    private uint[,] _lut;

    private void SetupConstants()
    {
        adjs = new uint[] {
        //    oi   o e     d      d     e
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
        //                    0         1         2         3
        //                    01234567890123456789012345678901
        outside = new(this, 0b10000100000000000000000000000000u);
        inside  = new(this, 0b01111011111111111111111111111111u);

        front = nodes[0];
        entrance = nodes[1];
        exit = nodes[4];
        back = nodes[5];
    }

    private void MakeLookUpTable()
    {
        _lut = new uint[MAX_INDEXES, MAX_LEVELS];

        for (int i = 0; i < MAX_INDEXES; i++) {
            // level 0: apenas o proprio bit
            uint resultMask = 1u << (31 - i);
            _lut[i, 0] = resultMask;
            
            uint borderMask = resultMask;

            // calcula os niveis de 1 ata o limite
            for (int lvl = 1; lvl < MAX_LEVELS; lvl++) {
                uint newAdjacents = 0u;
                uint tempMask = borderMask;

                // processa a borda atual
                while (tempMask != 0) {
                    uint singleBit = tempMask & (~tempMask + 1);
                    int bitIndex = 31 - math.tzcnt(singleBit);
                    newAdjacents |= adjs[bitIndex];
                    tempMask &= (tempMask - 1);
                }
                // atualiza a borda e o resultado final do nivel
                borderMask = newAdjacents & ~resultMask;
                resultMask |= borderMask;
                _lut[i, lvl] = resultMask; // salva na tabela

                // OTIMIZACAO DE PREENCHIMENTO:
                // se a borda atingiu as extremidades do mapa ou ilha isolada,
                // os niveis seguintes terao a mesma mascara final, preenche o resto e para
                if (borderMask == 0) {
                    for (int remainingLvl = lvl + 1; remainingLvl < MAX_LEVELS; remainingLvl++)
                        _lut[i, remainingLvl] = resultMask;
                    break;
                }
            }
        }
    }

    void Awake()
    {
        SetupConstants();
        MakeLookUpTable();
    }

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

    public bool IsTheSame (int index, Collider collider)
    {
        return nodes[index] == collider;
    }

    public bool IsIncluded (int index, uint mask)
    {
        return (IndexToMask(index) & mask) > 0u;
    }

    public bool IsIncluded (Collider collider, uint mask)
    {
        uint colliderMask = GetMaskByCollider(collider);
        return (mask & colliderMask) > 0u;
    }

    public bool IsAdjacent (Collider a, Collider b)
    {
        int aIndex = nodes.IndexOf(a);
        uint bMask = GetMaskByCollider(b);
        return (adjs[aIndex] & bMask) > 0u;
    }

    public uint GetAdjacentsByLevel(int index, int level)
    {
        if (level < 0) return 0u;
        if (level >= MAX_LEVELS) level = MAX_LEVELS - 1;
        return _lut[index, level];
    }

    public uint GetAdjacentsByLevel(Collider collider, int level)
    {
        if (level < 0) return 0u;
        if (level >= MAX_LEVELS) level = MAX_LEVELS - 1;
        int index = nodes.IndexOf(collider);
        return _lut[index, level];
    }

    public record HousePlace {
        private HouseBuilding house;
        public uint mask { get; private set; }
        
        public HousePlace(HouseBuilding house, uint mask)
        {
            this.house = house;
            this.mask = mask;
        }

        public bool Includes (int index)
        {
            return (house.IndexToMask(index) & mask) > 0u;
        }

        public bool Includes (Collider collider)
        {
            uint colliderMask = house.GetMaskByCollider(collider);
            return (mask & colliderMask) > 0u;
        }
    }
}
