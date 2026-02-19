using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Voxelis;
using Voxelis.Tick;

namespace Dynamics
{
    [BurstCompile]
    public struct WireWorldJob : IJobParallelForDefer
    {
        public NativeArray<VoxelisXWorld.BrickInfo> brickInfos;
        
        public void Execute(int index)
        {
            // This whole thing doesn't seem vectorized
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
                        // Unity.Burst.CompilerServices.Loop.ExpectVectorized();
                        int3 blockPos = brickInfo.BrickOrigin + new int3(x, y, z);
                        // bool solid = readerHelper.GetBlock(blockPos).id > 0;
                        // if (solid) continue;

                        byte solid = 0;
                        foreach (var d in NeighborhoodSettings.Directions)
                        {
                            solid += (byte)((readerHelper.GetBlock(blockPos + d).id > 0) ? 1 : 0);
                            // if (solid) break;
                        }

                        workingSector.SetBlock(
                            blockPos.x, blockPos.y, blockPos.z,
                            (solid == 4) ? new Block(15, 15, 15, false) : default
                        );
                    }
                }
            }
            
            // // TODO: Implement WireWorld
            // uint x = b.data;
            // x += 0x9e3779b9;  // Weyl sequence (golden ratio)
            // x ^= x >> 16;
            // x *= 0x85ebca6b;  // multiplication for avalanche
            // x ^= x >> 13;
            // x *= 0xc2b2ae35;
            // x ^= x >> 16;

            // b.data = x;
        }
    }
    
    public class WireWorld : ITickHook<VoxelisXWorld.AutomataStageInputs>
    {
        public bool Execute(VoxelisXWorld.AutomataStageInputs inputs, JobHandle stageStart, JobHandle chained, out JobHandle handle)
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