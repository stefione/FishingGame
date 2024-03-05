using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
namespace PixPlays.Fishing.Fish
{
    public class ChainController : MonoBehaviour
    {
        [SerializeField] Transform _Ancor;
        [SerializeField] MeshFilter _meshFilter;

        [SerializeField] float _Distance;
        [SerializeField] float Height;
        [SerializeField] int HorizontalSubdivision;
        [SerializeField] int VerticalSubdivision;

        private NativeArray<Vector3> _verticies;
        private NativeArray<Vector3> _points;
        private Mesh mesh;
        private int flip = 1;

        private void Start()
        {
            Initialize();
            _verticies = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);
        }

        private void Initialize()
        {
            _points = new NativeArray<Vector3>(HorizontalSubdivision, Allocator.Persistent);
            for (int i = 0; i < HorizontalSubdivision; i++)
            {
                _points[i] = Vector3.zero;
            }

            if (mesh == null)
            {
                mesh = new Mesh();
            }
            List<Vector3> verts = new();
            List<Vector2> uvs = new();

            float verticalDelta = Height / VerticalSubdivision;
            float horizontalDelta = 1f / (HorizontalSubdivision - 1);
            float verticalOneDelta = 1f / (VerticalSubdivision - 1);
            for (int i = 0; i < _points.Length; i++)
            {
                Vector3 dir = Vector3.up;
                if (i + 1 < _points.Length - 1)
                {
                    dir = Quaternion.Euler(0, 0, 90) * (_points[i + 1] - _points[i]).normalized;
                }
                else
                {
                    dir = Quaternion.Euler(0, 0, 90) * (_points[i] - _points[i - 1]).normalized;
                }
                Vector3 p = _points[i] - dir * Height / 2f;

                for (int j = 0; j < VerticalSubdivision; j++)
                {
                    verts.Add(p + dir * verticalDelta * j);
                    uvs.Add(new Vector2(1 - horizontalDelta * i, j * verticalOneDelta));
                }
            }
            List<int> tris = new List<int>();
            for (int i = 0; i < verts.Count - VerticalSubdivision; i += VerticalSubdivision)
            {
                for (int j = 0; j < VerticalSubdivision - 1; j++)
                {
                    int p = i + j;
                    tris.Add(p);
                    tris.Add(p + 1);
                    tris.Add(p + VerticalSubdivision);

                    tris.Add(p + 1);
                    tris.Add(p + VerticalSubdivision + 1);
                    tris.Add(p + VerticalSubdivision);
                }
            }


            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.UploadMeshData(false);
            _meshFilter.mesh = mesh;
        }

        private void FixedUpdate()
        {
            if (_Ancor.hasChanged)
            {
                Vector3 ancor = _Ancor.position;
                float distanceDelta = _Distance / HorizontalSubdivision;
                for (int i = 0; i < _points.Length; i++)
                {
                    Vector3 direction = (_points[i] - ancor);
                    if (direction.magnitude > distanceDelta)
                    {
                        _points[i] = ancor + direction.normalized * distanceDelta;
                    }
                    ancor = _points[i];
                }

                UpdateMesh();
                _Ancor.hasChanged = false;
            }
        }

        private void UpdateMesh()
        {
            //Vector3 ancorDir = (_points[0] - _points[1]).normalized;
            //if (flip * Vector3.Dot(Vector3.right, ancorDir) > 0)
            //{
            //    flip *= -1;
            //}
            float verticalDelta = Height / VerticalSubdivision;
            UpdateVerticiesJob job = new UpdateVerticiesJob()
            {
                Points= _points,
                Verts = _verticies,
                VerticalDelta = verticalDelta,
                Height=Height,
                Horizontal=HorizontalSubdivision,
                Vertical=VerticalSubdivision,
            };
            var jobHandle = job.Schedule(_verticies.Length, HorizontalSubdivision*VerticalSubdivision);
            jobHandle.Complete();
            mesh.SetVertices(job.Verts);
            jobHandle = default;
        }

        private void OnDestroy()
        {
            _points.Dispose();
            _verticies.Dispose();
        }


        [BurstCompile]
        public struct UpdateVerticiesJob : IJobParallelFor
        {
            [NativeDisableParallelForRestriction] public NativeArray<Vector3> Verts;
            [NativeDisableParallelForRestriction] public NativeArray<Vector3> Points;
            public float VerticalDelta;
            public float Height;
            public int Horizontal;
            public int Vertical;
            public void Execute(int i)
            {
                int x = i / Vertical;
                int y = i % Vertical;
                Vector3 dir = Vector3.up;
                if (x < Horizontal-1)
                {
                    dir = Quaternion.Euler(0, 0, 90) * (Points[x+1] - Points[x]).normalized;
                }
                else
                {
                    dir = Quaternion.Euler(0, 0, 90) * (Points[x] - Points[x-1]).normalized;
                }
                Vector3 p = Points[x] - dir * Height / 2f;
                Verts[i] = (p + dir * VerticalDelta * y);
            }
        }
    }
}
