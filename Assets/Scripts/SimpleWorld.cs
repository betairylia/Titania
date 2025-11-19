using System;
using Titania;
using Titania.WorldGenUtils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Voxelis;

namespace Titania
{
    public class SimpleWorld : VoxelEntity
    {
        [BurstCompile]
        public struct FillWorldSectorJob : IJob
        {
            public int3 sectorPos;
            public SectorHandle sector;

            public void Execute()
            {
                for (int x = 0; x < Sector.SIZE_IN_BRICKS * Sector.SIZE_IN_BLOCKS; x++)
                {
                    for (int z = 0; z < Sector.SIZE_IN_BRICKS * Sector.SIZE_IN_BLOCKS; z++)
                    {
                        int wx = x + sectorPos.x * Sector.SECTOR_SIZE_IN_BLOCKS;
                        int wz = z + sectorPos.z * Sector.SECTOR_SIZE_IN_BLOCKS;

                        // float3 grad1, grad2;
                        // float3 gradd1, gradd2, gradd3;
                        //
                        // float heightMap = -math.abs(noise.snoise(new float3(wx / 1024.0f, 0.0f, wz / 1024.0f), out grad1)) +
                        //                   -math.abs(noise.snoise(new float3(wx / 512.0f, 0.0f, wz / 512.0f), out grad2)) * 0.5f;
                        //
                        // float detailMap = noise.snoise(new float3(wx / 64.0f, 0, wz / 64.0f), out gradd1) * 1.0f +
                        //                   noise.snoise(new float3(wx / 32.0f, 0, wz / 32.0f), out gradd2) * 0.5f +
                        //                   noise.snoise(new float3(wx / 16.0f, 0, wz / 16.0f), out gradd3) * 0.25f;
                        //
                        // float erosion = math.length(grad1) * 0.0f + 
                        //                 math.length(grad2) * 0.0f + 
                        //                 math.length(gradd1) * 0.25f +
                        //                 math.length(gradd2) * 0.125f +
                        //                 math.length(gradd3) * 0.0625f;
                        //
                        // float height = 128.0f + heightMap * 128.0f + detailMap * 8.0f - erosion * 5.0f;
                        
                        // float heightMap = noise.snoise(new float2(wx / 1024.0f, wz / 1024.0f)) +
                        //                   -math.abs(noise.snoise(new float2(wx / 512.0f, wz / 512.0f))) * 0.5f +
                        //                   noise.snoise(new float2(wx / 64.0f, wz / 64.0f)) * 0.0625f +
                        //                   noise.snoise(new float2(wx / 32.0f, wz / 32.0f)) * 0.03125f +
                        //                   noise.snoise(new float2(wx / 16.0f, wz / 16.0f)) * 0.015625f;

                        float2 wp = new float2(wx, wz);
                        float heightMap = Noise.Simplex2D(wp, 1024.0f, octaves: 1) +
                                          Noise.Simplex2D(wp, 512.0f, Noise.SimplexModifiers.Ridge,
                                              octaves: 2) * 0.5f +
                                          Noise.Simplex2D(wp, 64.0f) * 0.0625f;
                        heightMap /= (1.0f + 0.5f + 0.0625f);
                        heightMap = HMUtils.Steepness(heightMap, 0.5f);
                        
                        float height = 128.0f + heightMap * 110.0f;
                        
                        for (int y = 0; y < Sector.SIZE_IN_BRICKS * Sector.SIZE_IN_BLOCKS; y++)
                        {
                            int wy = y + sectorPos.y * Sector.SECTOR_SIZE_IN_BLOCKS;

                            // var n = Unity.Mathematics.noise.snoise(
                            //     new float3(wx / 32.0f, wy / 32.0f, wz / 32.0f));
                            // var n = Unity.Mathematics.noise.snoise(
                            //             new float3(wx / 72.0f, wy / 72.0f, wz / 72.0f)) +
                            //         noise.snoise(
                            //             new float3(wx / 24.0f, wy / 24.0f, wz / 24.0f)) * 0.5f;
                            // n = n - (wy - 48) / 16.0f;
                            // float n = (float)(600 + 200 * math.sin(wx / 100.0) + 100 * math.cos(wz / 150.0)) - wy;
                            float n = (height - wy);
                            
                            // Don't make invisible blocks for now
                            // if (n is > 0 and < 16)
                            if (n > 0)
                            {
                                sector.SetBlock(x, y, z, IDBlocks.STONE);
                            }
                            // else if (wy < 63)
                            // {
                            //     sector.SetBlock(x, y, z, IDBlocks.WATER);
                            // }
                        }
                    }
                }
            }
        }
        
        public Vector3Int numSectors;

        private void Start()
        {
            Initialize();
        }

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            Dispose();
            throw new NotImplementedException();

            NativeList<JobHandle> fillWorldJobs = new NativeList<JobHandle>(Allocator.Temp);

            for (int i = 0; i < numSectors.x; i++)
            {
                for (int j = 0; j < numSectors.z; j++)
                {
                    for (int k = 0; k < numSectors.y; k++)
                    {
                        var secPos = new int3(i, k, j);
                        if (!Sectors.ContainsKey(secPos))
                        {
                            var sec = Sector.New(Allocator.Persistent, 128);
                            // sectors.Add(secPos, sec);
                        }

                        // var job = new FillWorldSectorJob()
                        // {
                        //     sectorPos = secPos,
                        //     sector = sectors[secPos]
                        // };
                        //
                        // fillWorldJobs.Add(job.Schedule());
                    }
                }
            }

            JobHandle.CompleteAll(fillWorldJobs);
            fillWorldJobs.Dispose();
            Debug.Log("Done!");

            int totalBricks = 0;
            foreach (var kvp in Sectors)
            {
                // totalBricks += sectors[kvp.Key].NonEmptyBrickCount;
            }

            Debug.Log($"Total: {totalBricks} Bricks ({totalBricks * 2 / 1024} MiB)");
        }
    }
}