using System;

namespace DwarfClone.World.Tile
{
    [Serializable]
    public class TileData
    {
        public TileType type;
        public string displayName;
        public float hardness;
        public string dropItemId;
        public int dropItemCount;
        public bool isSolid;
        public bool isWalkable;
        public bool isDiggable;
        public bool connectsZUp;
        public bool connectsZDown;
        public string spriteName;

        public TileData(
            TileType type,
            string displayName,
            float hardness,
            string dropItemId,
            int dropItemCount,
            bool isSolid,
            bool isWalkable,
            bool isDiggable,
            bool connectsZUp = false,
            bool connectsZDown = false,
            string spriteName = null)
        {
            this.type = type;
            this.displayName = displayName;
            this.hardness = hardness;
            this.dropItemId = dropItemId;
            this.dropItemCount = dropItemCount;
            this.isSolid = isSolid;
            this.isWalkable = isWalkable;
            this.isDiggable = isDiggable;
            this.connectsZUp = connectsZUp;
            this.connectsZDown = connectsZDown;
            this.spriteName = spriteName ?? type.ToString();
        }
    }
}
