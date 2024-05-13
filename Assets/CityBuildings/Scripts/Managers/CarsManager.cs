﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEngine;

namespace CityBuildings.Managers
{
    using Creators;
    using Utilities;
    using Structs;

    using Random = UnityEngine.Random;

    [DisallowMultipleComponent]
    [AddComponentMenu("City Buildings/Managers/Cars Manager")]
    public class CarsManager : MonoBehaviour
    {
        public const string PropHeight = "_Height";
        public const string PropSize = "_Size";
        public const string PropPower = "_Power";
        public const string PropForwardColor = "_ForwardColor";
        public const string PropBackColor = "_BackColor";
        public const string PropGeomData = "_GeomData";

        public bool Validity { get; set; } = false;

        [SerializeField]
        private int num = 50;
        [SerializeField, Range(0f, 1f)]
        private float straightRate = 0.75f;
        [SerializeField]
        private float speed = 1f;
        [SerializeField]
        private float offset = 1f;
        [SerializeField]
        private float size = 1f;
        [SerializeField]
        private float height = 0f;
        [SerializeField]
        private float power = 1f;
        [SerializeField]
        private Color forward = Color.white;
        [SerializeField]
        private Color back = Color.red;
        [SerializeField]
        private Material material = null;

        private ComputeBuffer geomBuffer = null;

        private Car[] cars = new Car[0];
        private SimpleCar[] simpleCars = new SimpleCar[0];
        private SkyscraperManager skyscraper = null;
        

        public void Initialize(SkyscraperManager skyscraper)
        {
            this.skyscraper = skyscraper;

            var roads = skyscraper.CityArea.Roads;
            var ids = roads.Keys;

            this.cars = new Car[this.num];
            this.simpleCars = new SimpleCar[this.num];

            for(var i = 0; i < this.num; i++)
            {
                var id = ids.ElementAt(Random.Range(0, ids.Count));

                this.cars[i] = new Car(roads[id], this.offset);
                this.cars[i].Update(this.skyscraper.CityArea, this.speed, this.straightRate);

                this.simpleCars[i] = this.cars[i];
            }

            this.geomBuffer = new ComputeBuffer(this.cars.Length, Marshal.SizeOf(typeof(SimpleCar)), ComputeBufferType.Default);
            this.geomBuffer.SetData(this.simpleCars.ToArray());
        }

        private void Update()
        {
            if(this.geomBuffer == null || this.Validity == false)
            {
                return;
            }

            for(var i = 0; i < this.cars.Length; i++)
            {
                var car = this.cars[i];
                car.Update(this.skyscraper.CityArea, this.speed, this.straightRate);

                this.cars[i] = car;
                this.simpleCars[i] = car;
            }
            
            this.geomBuffer.SetData(this.simpleCars.ToArray());
        }

        private void OnRenderObject()
        {
            if(this.geomBuffer == null)
            {
                return;
            }

            this.material.SetPass(0);

            this.material.SetFloat(PropHeight, this.height);
            this.material.SetFloat(PropSize, this.size);
            this.material.SetFloat(PropPower, this.power);
            this.material.SetColor(PropForwardColor, this.forward);
            this.material.SetColor(PropBackColor, this.back);
            this.material.SetBuffer(PropGeomData, this.geomBuffer);

            Graphics.DrawProceduralNow(MeshTopology.Points, 1, this.geomBuffer.count);
        }

        private void OnDestroy()
        {
            this.geomBuffer?.Release();
        }
    }
}
