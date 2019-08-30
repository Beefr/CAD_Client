using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class MyMesh
    {
        public Vector3[] uniqueVert; // vertices
        public int[] triangles; // triangles
        // always oriented correctly

        public GameObject obj;

        /// <summary>
        /// create a new game object
        /// </summary>
        public MyMesh()
        {
            obj = new GameObject();
        }
        /// <summary>
         /// create a new game object
         /// </summary>
        public MyMesh(GameObject go)
        {
            obj = go;
        }

        /// <summary>
        /// create a new game object
        /// </summary>
        public MyMesh(int[] triangles, List<OpencascadePart.Pnt> vertices)
        {
            uniqueVert = ComputeVertices(vertices);
            
            obj = new GameObject();
            obj.AddComponent<MeshRenderer>();
            MeshRenderer mr = MeshRenderer;
            obj.GetComponent<MeshRenderer>().material.color = Color.white;

            try { mr.material = new Material(Shader.Find("Lines/Colored Blended")); }
            catch (Exception e) { System.IO.File.WriteAllText(@"ShaderProblem.txt", e.ToString());  }


            obj.AddComponent<MeshFilter>();
            MeshFilter mf = MeshFilter;
            Mesh mesh = new Mesh();
            mf.mesh = mesh;
            
            mf.mesh.vertices = uniqueVert;
            mf.mesh.triangles = triangles;
            
            // unity fonctions that compute normals
            mf.mesh.Optimize();
            mf.mesh.RecalculateNormals();
        }

        /// <summary>
        /// the mesh renderer
        /// </summary>
        public MeshRenderer MeshRenderer
        {
            get
            {
                return obj.GetComponent<MeshRenderer>();
            }
        }

        /// <summary>
        /// the mesh filter
        /// </summary>
        public MeshFilter MeshFilter
        {
            get
            {
                return obj.GetComponent<MeshFilter>();
            }
        }

        /// <summary>
        /// the Mesh of the meshfilter
        /// </summary>
        public Mesh Mesh
        {
            get
            {
                return MeshFilter.mesh;
            }
        }

        /// <summary>
        /// convert our List<Pnt> into a List<Vector3>
        /// </summary>
        /// <param name="uniqueVertices"></param>
        private Vector3[] ComputeVertices(List<OpencascadePart.Pnt> uniqueVertices)
        {
            Vector3[] uniqueVert = new Vector3[uniqueVertices.Count];
            int count = 0;
            
            foreach (OpencascadePart.Pnt pt in uniqueVertices)
            {
                uniqueVert[count] = new Vector3((float)pt.x, (float)pt.y, (float)pt.z);
                count++;
            }

            return uniqueVert;

        }

    }
}
