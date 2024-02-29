using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace PixPlays.Fishing.States
{
    public abstract class EntityState
    {
        public bool Active { get; private set; }
        protected async void UpdateLoop()
        {
            while (Active && Application.isPlaying)
            {
                Update(Time.deltaTime);
                await Task.Yield();
            }
        }
        protected abstract void Update(float deltaTime);
        protected abstract void OnDeactivate();
        protected abstract void OnActivate();
        public void Activate()
        {
            if (!Active)
            {
                OnActivate();
                Active = true;
                UpdateLoop();
            }
        }
        public void Deactivate()
        {
            if (Active)
            {
                OnDeactivate();
                Active = false;
            }
        }
    }
}
