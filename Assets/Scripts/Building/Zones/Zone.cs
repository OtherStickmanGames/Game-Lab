using System;
using System.Collections.Generic;
using UnityEngine;

namespace DwarfClone.Building.Zones
{
    [Serializable]
    public class Zone
    {
        public string zoneId;
        public string zoneName;
        public ZoneType zoneType;
        public HashSet<Vector3Int> tiles = new HashSet<Vector3Int>();
        public Color color;

        public Zone(string id, string name, ZoneType type, Color col)
        {
            this.zoneId = id;
            this.zoneName = name;
            this.zoneType = type;
            this.color = col;
        }

        public void AddTile(Vector3Int tile)
        {
            tiles.Add(tile);
        }

        public void RemoveTile(Vector3Int tile)
        {
            tiles.Remove(tile);
        }

        public bool Contains(Vector3Int tile)
        {
            return tiles.Contains(tile);
        }
    }
}
