using UnityEngine;
using Thesis;
using System.Linq;
namespace Thesis
{
    public class CityMapController : MonoBehaviour
    {

        private Block block;

        private bool _drawGizmos = true;
        private bool _drawFirstBlock = true;
        private bool _drawBlocks = true;
        private bool _drawSideWalk = true;
        private bool _drawFirstLot = true;
        private bool _drawLots = true;

        void Start()
        {
            block = new Block();
            block.Bisect();
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.B))
            {
                AddBuildings();
                CityMapManager.Instance.DrawRoads();
                CityMapManager.Instance.DrawSidewalks();
                CityMapManager.Instance.DrawLots();
            }

            if (Input.GetKeyUp(KeyCode.R))
            {
                BuildingManager.Instance.DestroyBuildings();
                block = new Block();
                block.Bisect();
            }

            if (Input.GetKeyUp(KeyCode.G))
                _drawGizmos = !_drawGizmos;

            if (Input.GetKeyUp(KeyCode.Alpha1))
                _drawFirstBlock = !_drawFirstBlock;

            if (Input.GetKeyUp(KeyCode.Alpha2))
                _drawBlocks = !_drawBlocks;

            if (Input.GetKeyUp(KeyCode.Alpha3))
                _drawSideWalk = !_drawSideWalk;

            if (Input.GetKeyUp(KeyCode.Alpha4))
                _drawFirstLot = !_drawFirstLot;

            if (Input.GetKeyUp(KeyCode.Alpha5))
                _drawLots = !_drawLots;

            if (Input.GetKeyUp(KeyCode.Alpha0))
            {
                _drawFirstBlock = false;
                _drawBlocks = false;
                _drawSideWalk = false;
                _drawFirstLot = false;
                _drawLots = false;
            }
        }

        private void AddBuildings()
        {
            BuildingManager.Instance.DestroyBuildings();
            foreach (Block b in CityMapManager.Instance.blocks)
                foreach (BuildingLot l in b.finalLots)
                    BuildingManager.Instance.Build(l);
        }

        void OnPostRender()
        {
            if (_drawGizmos)
            {
                if (_drawFirstBlock)
                {
                    MaterialManager.Instance.Get("line_block").SetPass(0);
                    GL.Begin(GL.LINES);
                    foreach (Edge e in block.edges)
                    {
                        GL.Vertex(e.start);
                        GL.Vertex(e.end);
                    }
                    GL.End();
                }

                if (_drawBlocks)
                {
                    MaterialManager.Instance.Get("line_block").SetPass(0);
                    GL.Begin(GL.LINES);
                    foreach (Block b in CityMapManager.Instance.blocks)
                        foreach (Edge e in b.edges)
                        {
                            GL.Vertex(e.start);
                            GL.Vertex(e.end);
                        }
                    GL.End();
                }

                if (_drawSideWalk)
                {
                    MaterialManager.Instance.Get("line_sidewalk").SetPass(0);
                    GL.Begin(GL.LINES);
                    foreach (Sidewalk s in CityMapManager.Instance.sidewalks)
                        foreach (Edge e in s.edges)
                        {
                            GL.Vertex(e.start);
                            GL.Vertex(e.end);
                        }
                    GL.End();
                }

                if (_drawFirstLot)
                {
                    MaterialManager.Instance.Get("line_lot").SetPass(0);
                    GL.Begin(GL.LINES);
                    foreach (Block b in CityMapManager.Instance.blocks)
                        foreach (Edge e in b.initialLot.edges)
                        {
                            GL.Vertex(e.start);
                            GL.Vertex(e.end);
                        }
                    GL.End();
                }

                if (_drawLots)
                {
                    MaterialManager.Instance.Get("line_lot").SetPass(0);
                    GL.Begin(GL.LINES);
                    foreach (Block b in CityMapManager.Instance.blocks)
                        foreach (BuildingLot l in b.finalLots)
                            foreach (Edge e in l.edges)
                            {
                                GL.Vertex(e.start);
                                GL.Vertex(e.end);
                            }
                    GL.End();
                }
            }
        }
    }
}