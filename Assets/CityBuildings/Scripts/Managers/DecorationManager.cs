﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using UnityEngine;

namespace CityBuildings.Managers
{
    using Structs;
    using Creators;
    using Random = UnityEngine.Random;

    [DisallowMultipleComponent]
    [AddComponentMenu("City Buildings/Managers/Decoration Manager")]
    public class DecorationManager : MonoBehaviour
    {
        [Serializable]
        private struct Times
        {
            [SerializeField]
            public float time;
        }

        public const string PropData = "_Data";
        public const string PropTriangleSize = "_Radius";
        public const string PropTimesBuffer = "_Times";
        public const string PropNightColor = "_NightColor";
        public const string PropNoonColor = "_NoonColor";

        [SerializeField]
        private Material material = null;
        [SerializeField]
        private float radius = 0.5f;
        [SerializeField]
        private AnimationCurve curve = new AnimationCurve();
        [SerializeField]
        private float speed = 0.1f;
        [SerializeField]
        private Color nightColor = Color.black;
        [SerializeField]
        private Color noonColor = new Color(0.76f, 0.78f, 0.79f, 1f);
        
        private ComputeBuffer buffer = null;
        private ComputeBuffer timesBuffer = null;
        private float[] timers = new float[0];
        private Times[] times = new Times[0];
        

        public void Initialize(SkyscraperManager skyscraper)
        {
            var decos = skyscraper.Builder.Decos;

            this.buffer?.Release();
            this.timesBuffer?.Release();

            if(decos.Count <= 0)
            {
                this.timers = new float[0];

                return;
            }

            this.buffer = new ComputeBuffer(decos.Count, Marshal.SizeOf(typeof(DecorationData)), ComputeBufferType.Default);
            this.buffer.SetData(decos.ToArray());

            this.timesBuffer = new ComputeBuffer(decos.Count, Marshal.SizeOf(typeof(Times)), ComputeBufferType.Default);
            this.timers = new float[this.buffer.count];
            this.times = new Times[this.buffer.count];

            for(var i = 0; i < this.times.Length; i++)
            {
                this.timers[i] = Random.value;
                this.times[i].time = this.curve.Evaluate(this.timers[i]);
            }

            this.timesBuffer.SetData(this.times);

            this.material.SetColor(PropNightColor, this.nightColor);
            this.material.SetColor(PropNoonColor, this.noonColor);
        }
        
        private void Update()
        {
            if(this.timers.Length <= 0 || MainController.Instance.IsNight == false)
            {
                return;
            }

            for(var i = 0; i < this.times.Length; i++)
            {
                var t = this.timers[i];
                this.timers[i] += Time.deltaTime * this.speed;

                if(t < 0f && this.timers[i] >= 0f)
                {
                    this.timers[i] = 0f;
                }
                else if(t > 0f && this.timers[i] >= 1f)
                {
                    this.timers[i] = -1f;
                }

                this.times[i].time = this.curve.Evaluate(Mathf.Abs(this.timers[i]));
            }

            this.timesBuffer.SetData(this.times);
        }

        private void OnRenderObject()
        {
            if(this.timers.Length <= 0)
            {
                return;
            }

            this.material.SetPass(0);
            this.material.SetBuffer(PropData, this.buffer);
            this.material.SetBuffer(PropTimesBuffer, this.timesBuffer);
            this.material.SetFloat(PropTriangleSize, this.radius);

            Graphics.DrawProceduralNow(MeshTopology.Points, 4, this.buffer.count);
        }

        private void OnDestroy()
        {
            this.buffer?.Release();
            this.timesBuffer?.Release();
        }
    }
}
