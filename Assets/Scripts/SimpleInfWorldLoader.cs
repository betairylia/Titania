using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Voxelis;

namespace Titania
{
    public class SimpleInfWorldLoader : InfiniteLoader
    {
        private HashSet<(JobHandle, int3, SectorHandle)> allSectorsRemain = new();
        
        public override void LoadSector(int3 secPos)
        {
            if (entity.Sectors.ContainsKey(secPos))
            {
                return;
            }
            
            if (secPos.y < 0)
            {
                return;
            }
            
            SectorHandle sec = SectorHandle.AllocEmpty(1, Allocator.Persistent);

            var job = new SimpleWorld.FillWorldSectorJob()
            {
                sectorPos = secPos,
                sector = sec
            };

            allSectorsRemain.Add((job.Schedule(), secPos, sec));
            // entity.SetBlock(sectorPos * 128 + 64 * Vector3Int.one,
            //     new Block(sectorPos.x % 32, sectorPos.y % 32, sectorPos.z % 32, false));
        }

        public override void Tick()
        {
            base.Tick();

            var copy = allSectorsRemain.ToList();
            foreach (var tuple in copy)
            {
                if (tuple.Item1.IsCompleted)
                {
                    tuple.Item1.Complete();
                    MarkSectorLoaded(tuple.Item2, tuple.Item3);
                    allSectorsRemain.Remove(tuple);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (var tuple in allSectorsRemain)
            {
                tuple.Item3.Dispose(Allocator.Persistent);
            }
        }
        private void OnGUI()
        {
            // Create a rectangle that starts 100px from the left of the screen
            Rect areaRect = new Rect(110, 0, 250, Screen.height);
    
            // Begin the GUI area with the specified rectangle
            GUILayout.BeginArea(areaRect);
            
            GUILayout.BeginVertical(); 

            GUILayout.Box($"Render bounds: {sectorLoadBounds}\n" +
                          $"Render radius: {sectorLoadRadiusInBlocks}\n" +
                          $"Render unload: {sectorUnloadRadiusInBlocks}\n" +
                          $"Sectors generating: {allSectorsRemain.Count}");
            
            GUILayout.EndVertical();
            
            GUILayout.EndArea();
        }
    }
}