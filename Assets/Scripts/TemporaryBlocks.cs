using Voxelis;
using Voxelis.Simulation;

namespace Titania
{
    public static class TemporaryBlocks
    {
        // Solids
        public static readonly Block STONE = new Block(5, 5, 5, false);
        
        // Powders
        public static readonly Block SAND = new Block() { data = 4249616384U };
        
        // Liquids
        public static readonly Block WATER = new Block() { data = 225312768U };
        public static readonly Block WATERSMALL = new Block() { data = 225312769U };
        public static readonly Block LIGHT = new Block() { data = 4294836224U };
    }

    public static class IDBlocks
    {
        // Solids
        public static readonly Block STONE = new Block(0x1000);
        
        // Liquids
        public static readonly Block WATER = new Block(0x0001);
    }
}