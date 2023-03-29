using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace DungeonGenerator
{
    public enum TileType
    {
        Wall,
        Floor,
        Element,
    }

    public class Tile
    {
        public  TileType TileType { get; set; }
        public  Element? Element  { get; set; }

        private bool _passable;
        public bool Passable
        {
            get => Element?.Passable ?? _passable;
            set
            {
                if (Element == null)
                    _passable = value;
            }
        }
        //public  bool     Passable => Element?.Passable ?? _passable;

        public Tile(TileType type)
        {
            SetTileType(type);
        }

        public void SetElement(Element e)
        {
            Element = e;
        }

        public void SetTileType(TileType type)
        {
            TileType = type;
            switch (type)
            {
                case TileType.Wall:
                    Passable = false;
                    break;
                case TileType.Floor:
                    Passable = true;
                    break;
                case TileType.Element:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }


}
