using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Voxelis;
using Voxelis.Simulation;

namespace Dynamics
{
    public class TitaniaCore : MonoBehaviour
    {
        private VoxelisXCoreWorld world;
        private VoxelisXRenderer renderer;
        private VoxelRayCast rayCaster;

        [SerializeField] private bool doTick = false;
        
        void Start()
        {
            world = VoxelisXCoreWorld.instance;
            renderer = FindFirstObjectByType<VoxelisXRenderer>();
            rayCaster = FindFirstObjectByType<VoxelRayCast>();

            Tick();
        }
        
        // TODO: FIXME: Implement proper input handling and change ticking to FixedUpdate
        // public void FixedUpdate()
        public void Update()
        {
            if (doTick) Tick();
        }

        public void Tick()
        {
            // Handle player inputs
            rayCaster.Tick();
            
            // Tick voxel entities
            foreach (var entity in world.AllEntities)
            {
                // TODO: Better ways?
                // entity.SendMessage("Tick");
            }

            // Update renderer
            renderer.Tick();
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