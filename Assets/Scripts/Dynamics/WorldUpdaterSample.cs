using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Voxelis;
using Titania;

[RequireComponent(typeof(VoxelEntity))]
public class WorldUpdaterSample : MonoBehaviour
{
    private VoxelEntity entity;
    
    NativeArray<Vector3Int> consts_offsetsPowder;

    NativeArray<Vector3Int> consts_offsetsLiquid;
    
    [BurstCompile]
    struct UpdateJob : IJob
    {
        public SectorHandle sector;
        public int tick;

        [ReadOnly] public NativeArray<Vector3Int> offsetsPowder;
        [ReadOnly] public NativeArray<Vector3Int> offsetsLiquid;
        
        public void Execute()
        {
            Sector cloned = Sector.CloneNoRecord(sector, Allocator.Temp);
            var rng = new Unity.Mathematics.Random((uint)tick);
            
            for (int x = 1; x < 127; x++)
            {
                for (int z = 1; z < 127; z++)
                {
                    for (int y = 1; y < 127; y++)
                    {
                        var b = cloned.GetBlock(x, y, z);

                        if (b == TemporaryBlocks.LIGHT)
                        {
                            sector.SetBlock(x, y, z, new Block((ushort)(b.id + 1)));
                        }
                        
                        int r = rng.NextInt(4);
                        // int r = 0;
                        
                        if (b == TemporaryBlocks.SAND)
                        // if (b != Block.Empty)
                        {
                            for (int offi = 0; offi < offsetsPowder.Length; offi++)
                            {
                                int offi_shuffule = offi;
                                if (offi > 0)
                                {
                                    offi_shuffule = 1 + ((offi_shuffule-1) / 4) * 4 + (((offi_shuffule-1) + r) & 3);
                                }

                                Vector3Int offd = offsetsPowder[offi_shuffule];
                                Block bo = sector.GetBlock(x + offd.x, y + offd.y, z + offd.z);
                                if (bo.isEmpty)
                                {
                                    sector.SetBlock(x, y, z, bo);
                                    sector.SetBlock(x + offd.x, y + offd.y, z + offd.z, b);
                                    break;
                                }
                            }
                        }
                        else if (b == TemporaryBlocks.WATER)
                        {
                            for (int offi = 0; offi < offsetsLiquid.Length; offi++)
                            {
                                int offi_shuffule = offi;
                                if (offi > 0)
                                {
                                    offi_shuffule = 1 + ((offi_shuffule-1) / 4) * 4 + (((offi_shuffule-1) + r) % 4);
                                }
                                
                                Vector3Int offd = offsetsLiquid[offi_shuffule];
                                // Block boc = cloned.GetBlock(x + offd.x, y + offd.y, z + offd.z);
                                Block bo = sector.GetBlock(x + offd.x, y + offd.y, z + offd.z);
                                if (bo.isEmpty)
                                {
                                    sector.SetBlock(x, y, z, bo);
                                    sector.SetBlock(x + offd.x, y + offd.y, z + offd.z, b);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            
            cloned.Dispose(Allocator.Temp);
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        entity = GetComponent<VoxelEntity>();

        consts_offsetsPowder = new NativeArray<Vector3Int>(new Vector3Int[]
        {
            new Vector3Int(0, -1, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(0, -1, 1),
            new Vector3Int(0, -1, -1),
        }, Allocator.Persistent);

        // consts_offsetsLiquid = new NativeArray<Vector3Int>(new Vector3Int[]
        // {
        //     new Vector3Int(0, -1, 0),
        //     new Vector3Int(-1, -1, 0),
        // }, Allocator.Persistent);
        
        consts_offsetsLiquid = new NativeArray<Vector3Int>(new Vector3Int[]
        {
            new Vector3Int(0, -1, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(0, -1, 1),
            new Vector3Int(0, -1, -1),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 0, -1),
        }, Allocator.Persistent);
    }

    [SerializeField]
    private bool doTick = true;

    private NativeList<JobHandle> jobs;

    [ContextMenu("Tick")]
    public void Tick()
    {
        if(!doTick) return;
        
        jobs = new NativeList<JobHandle>(Allocator.Persistent);
        
        foreach (var kvp in entity.Sectors)
        {
            int p = Time.frameCount;

            jobs.Add(new UpdateJob()
            {
                tick = p,
                sector = entity.Sectors[kvp.Key],
                offsetsLiquid = consts_offsetsLiquid,
                offsetsPowder = consts_offsetsPowder
            }.Schedule());
        }
        
        JobHandle.CompleteAll(jobs);
        jobs.Dispose();
    }

    private void OnDestroy()
    {
        consts_offsetsLiquid.Dispose();
        consts_offsetsPowder.Dispose();
    }
}
