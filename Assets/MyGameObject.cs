using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    /// <summary>
    /// your game object with some more properties
    /// </summary>
    [DataContract]
    class MyGameObject
    {
        [DataMember]
        private GameObject gameObject { get; set; }

        [DataMember]
        private int ID { get; set; }

        [DataMember]
        private Vector3 position { get; set; }

        [DataMember]
        private Vector3 scale { get; set; }

        public MyGameObject() { }

        public MyGameObject(GameObject go) {
            this.gameObject = go;
            this.position = go.transform.position;
            this.scale = go.transform.localScale;
            this.ID = go.GetComponent<AdditionnalProperties>().ID;
        }
    }
}
