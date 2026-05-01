using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Voxelis;
using Voxelis.Tick;

namespace Dynamics
{
    public static class WireWorldStates
    {
        public const uint Empty = 0;
        public const uint Conductor = 1;
        public const uint ElectronHead = 2;
        public const uint ElectronTail = 3;

        public static readonly Block EmptyBlock = default;
        public static readonly Block ConductorBlock = new Block(Conductor);
        public static readonly Block ElectronHeadBlock = new Block(ElectronHead);
        public static readonly Block ElectronTailBlock = new Block(ElectronTail);
    }

    /// <summary>
    /// Legacy placeholder automata extracted from WireWorld.
    /// Kept for experimentation and regression checks.
    /// </summary>
    [BurstCompile]
    public struct LegacyNeighborhoodSolidCountJob : IJobParallelForDefer
    {
        public NativeArray<VoxelisXWorld.BrickInfo> brickInfos;

        public void Execute(int index)
        {
            VoxelisXWorld.BrickInfo brickInfo = brickInfos[index];
            SectorHandle workingSector = brickInfo.Sector;

            SectorNeighborhoodReaderHelper readerHelper =
                new SectorNeighborhoodReaderHelper(
                    brickInfo.Sector,
                    brickInfo.Neighbors
                );

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int z = 0; z < 8; z++)
                    {
                        int3 blockPos = brickInfo.BrickOrigin + new int3(x, y, z);

                        byte solid = 0;
                        foreach (var d in NeighborhoodSettings.Directions)
                        {
                            solid += (byte)((readerHelper.GetBlock(blockPos + d).id > 0) ? 1 : 0);
                        }

                        workingSector.SetBlock(
                            blockPos.x,
                            blockPos.y,
                            blockPos.z,
                            (solid == 4) ? new Block(15, 15, 15, false) : default
                        );
                    }
                }
            }
        }
    }

    [BurstCompile]
    public struct WireWorldJob : IJobParallelForDefer
    {
        public NativeArray<VoxelisXWorld.BrickInfo> brickInfos;

        public void Execute(int index)
        {
            VoxelisXWorld.BrickInfo brickInfo = brickInfos[index];
            SectorHandle workingSector = brickInfo.Sector;

            SectorNeighborhoodReaderHelper readerHelper =
                new SectorNeighborhoodReaderHelper(
                    brickInfo.Sector,
                    brickInfo.Neighbors
                );

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int z = 0; z < 8; z++)
                    {
                        int3 blockPos = brickInfo.BrickOrigin + new int3(x, y, z);
                        Block currentBlock = readerHelper.GetBlock(blockPos);

                        workingSector.SetBlock(blockPos.x, blockPos.y, blockPos.z, NextState(currentBlock.id, blockPos, readerHelper));
                    }
                }
            }
        }

        private static Block NextState(uint currentId, int3 blockPos, SectorNeighborhoodReaderHelper readerHelper)
        {
            if (currentId == WireWorldStates.Empty)
            {
                return WireWorldStates.EmptyBlock;
            }

            if (currentId == WireWorldStates.ElectronHead)
            {
                return WireWorldStates.ElectronTailBlock;
            }

            if (currentId == WireWorldStates.ElectronTail)
            {
                return WireWorldStates.ConductorBlock;
            }

            if (currentId != WireWorldStates.Conductor)
            {
                return new Block(currentId);
            }

            int headNeighbors = 0;
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dz = -1; dz <= 1; dz++)
                    {
                        if (dx == 0 && dy == 0 && dz == 0)
                        {
                            continue;
                        }

                        int3 neighborPos = blockPos + new int3(dx, dy, dz);
                        if (readerHelper.GetBlock(neighborPos).id == WireWorldStates.ElectronHead)
                        {
                            headNeighbors++;
                            if (headNeighbors > 2)
                            {
                                return WireWorldStates.ConductorBlock;
                            }
                        }
                    }
                }
            }

            return (headNeighbors == 1 || headNeighbors == 2)
                ? WireWorldStates.ElectronHeadBlock
                : WireWorldStates.ConductorBlock;
        }
    }

    public class WireWorld : ITickHook<VoxelisXWorld.AutomataStageInputs>
    {
        public bool Execute(VoxelisXWorld.AutomataStageInputs inputs, JobHandle stageStart, JobHandle chained,
            out JobHandle handle)
        {
            var job = new WireWorldJob
            {
                brickInfos = inputs.BricksRequiredUpdate.AsDeferredJobArray(),
            };
            handle = job.Schedule(inputs.BricksRequiredUpdate, 32, chained);

            return true;
        }
    }
}
