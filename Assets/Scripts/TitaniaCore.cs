using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Voxelis;
using Voxelis.Rendering.Meshing;
using Voxelis.Simulation;
using Vector3 = System.Numerics.Vector3;

namespace Dynamics
{
    public class TitaniaCore : MonoBehaviour
    {
        private VoxelisXWorld world;
        // private VoxelisXRenderer renderer;
        // private VoxelMeshRendererComponent meshingRenderer;
        // private VoxelRayCast rayCaster;

        [SerializeField] private bool doTick = false;
        
        void Start()
        {
            world = (VoxelisXWorld)(VoxelisXWorld.instance);
            // renderer = FindFirstObjectByType<VoxelisXRenderer>();
            // meshingRenderer = FindFirstObjectByType<VoxelMeshRendererComponent>();
            // rayCaster = FindFirstObjectByType<VoxelRayCast>();
            
            world.automataStage.RegisterHook(new WireWorld());
        }
        
        // TODO: FIXME: Implement proper input handling and change ticking to FixedUpdate
        // public void FixedUpdate()
        public void Update()
        {
        }

        private List<VoxelCollisionSolver.ContactPoint> contacts = new();
        
        private void FixedUpdate()
        {
            var nativeBuf = new NativeList<VoxelCollisionSolver.ContactPoint>(Allocator.TempJob);
            
            // Collision test
            for (int i = 0; i < world.AllEntities.Count; i++)
            {
                var entity = world.AllEntities[i]; 
                for (int j = i+1; j < world.AllEntities.Count; j++)
                {
                    var otherEntity = world.AllEntities[j];
                    // entity.TestCollision(otherEntity, contacts, nativeBuf);
                }
            }

            nativeBuf.Dispose();
        }
    }
}