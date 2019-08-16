using OpencascadePart.Elements;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Assets
{
    public delegate void MyConstructionHandler(object source, MyEventArgs e);
    
    /// <summary>
    /// tries building elements in the construction queue when calling TryBuilding()
    /// it also stores all the existing elements
    /// </summary>
    public sealed class Constructor
    {

        public event MyConstructionHandler InQueue;

        private int positionIncrementation = 5;
        private int position = 0;
        private int currentID = 0;
        private List<BasicElement> constructionList = new List<BasicElement>();
        private List<MyMesh> constructedElements = new List<MyMesh>();

        private static readonly Lazy<Constructor> _lazy = new Lazy<Constructor>(() => new Constructor());
        public static Constructor Instance { get { return _lazy.Value; } }
        private Constructor()
        {
            // Now lets test the event contained in the above class.
            this.InQueue += new MyConstructionHandler(ElementInQueue);
        }
        
        /// <summary>
        /// if there are elements in the construction queue, then it tries constructing them
        /// </summary>
        public void TryBuilding()
        {
            if (constructionList.Count != 0)
            {
                lock(constructionList)
                {
                    InQueue(this, new MyEventArgs(position, currentID, positionIncrementation, constructionList.GetRange(0, constructionList.Count)));
                    this.position += constructionList.Count*positionIncrementation;
                    this.currentID += constructionList.Count;
                    constructionList.Clear();
                }
            }
        }

        // getters and setters _________________

        public int Position() { return position; }
        public int CurrentID() { return currentID; }
        public void SetCurrentID(int id) { this.currentID = id; }
        public void SetPosition(int pos) { this.position = pos; }
        public void IncrementID() { this.currentID++; }
        public void IncrementPosition(int value) { this.position += value; }
        public void SetPositionIncrementation(int value) { this.positionIncrementation = value; }

        //_____________________________________

        /// <summary>
        /// foreach element in the construction queue, it constructs it
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        static void ElementInQueue(object source, MyEventArgs e)
        {
            Debug.Log("Elements in queue, starting building...");
            int position = e.GetPosition();
            int currentID = e.GetCurrentID();
            foreach(BasicElement elem in e.GetConstructionQueue())
            {
                CreateObject(elem, position, currentID);
                position+= e.GetPositionIncrementation();
                currentID++;
            }
        }

        /// <summary>
        /// create an object with the given element
        /// </summary>
        /// <param name="c"></param>
        private static void CreateObject(BasicElement basicElement, int position, int currentID)
        {
            basicElement.Build();

            MyMesh mesh = new MyMesh(basicElement.GetMesh().triangles, basicElement.GetMesh().vertices);

            mesh.obj.transform.localScale = new Vector3(1, 1, 1);
            mesh.obj.AddComponent<BoxCollider>();
            mesh.obj.transform.position = new Vector3(1.0f, 1.0f, -1.0f - position);
            mesh.obj.AddComponent<GetSelected>();
            mesh.obj.AddComponent<AdditionnalProperties>();
            mesh.obj.GetComponent<AdditionnalProperties>().ID = currentID + 1;
        }

        /// <summary>
        /// adds a new element in the construction queue
        /// </summary>
        /// <param name="elem"></param>
        public void AddNewElement(BasicElement elem)
        {
            constructionList.Add(elem);
        }
        /// <summary>
        /// adds a list of new elements in the construction queue
        /// </summary>
        /// <param name="elem"></param>
        public void AddNewElement(List<BasicElement> elem)
        {
            constructionList.AddRange(elem);
        }

        /// <summary>
        /// adds an existing element in the constructed list
        /// </summary>
        /// <param name="mesh"></param>
        public void AddExistingElement(MyMesh mesh)
        {
            constructedElements.Add(mesh);
        }
        /// <summary>
        /// adds a list of existing elements in the constructed list
        /// </summary>
        /// <param name="meshes"></param>
        public void AddExistingElement(List<MyMesh> meshes)
        {
            constructedElements.AddRange(meshes);
        }


        /// <summary>
        /// adds an existing element in the constructed list
        /// </summary>
        /// <param name="mesh"></param>
        public void AddExistingElement(GameObject go)
        {
            MyMesh mesh = new MyMesh(go);

            //mesh.obj.AddComponent<BoxCollider>();
            mesh.obj.AddComponent<GetSelected>();
            mesh.obj.AddComponent<AdditionnalProperties>();
            AdditionnalProperties prop = mesh.obj.GetComponent<AdditionnalProperties>();
            prop.ID = currentID + 1;
            currentID++;
            constructedElements.Add(mesh);
        }

        /// <summary>
        /// adds a list of existing elements in the constructed list
        /// </summary>
        /// <param name="mesh"></param>
        public void AddExistingElement(List<GameObject> go)
        {
            foreach(GameObject g in go)
            {
                MyMesh mesh = new MyMesh(g);

                //mesh.obj.AddComponent<BoxCollider>();
                mesh.obj.AddComponent<GetSelected>();
                mesh.obj.AddComponent<AdditionnalProperties>();
                AdditionnalProperties prop = mesh.obj.GetComponent<AdditionnalProperties>();
                prop.ID = currentID + 1;
                constructedElements.Add(mesh);
            }
        }

        public List<MyMesh> GetConstructedElements()
        {
            return constructedElements;
        }

    }

  

    public class MyEventArgs : EventArgs
    {
        private int position;
        private int currentID;
        private int positionIncrementation;
        private List<BasicElement> ConstructionQueue;

        public MyEventArgs(int position, int currentID, int positionIncrementation, List<BasicElement> ConstructionQueue)
        {
            this.position = position;
            this.currentID = currentID;
            this.positionIncrementation = positionIncrementation;
            this.ConstructionQueue = ConstructionQueue;
        }

        public int GetPosition() { return position; }
        public int GetCurrentID() { return currentID; }
        public int GetPositionIncrementation() { return positionIncrementation; }
        public List<BasicElement> GetConstructionQueue() { return ConstructionQueue; }
    }
} // source: https://stackoverflow.com/questions/803242/understanding-events-and-event-handlers-in-c-sharp