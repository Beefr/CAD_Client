using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    /// <summary>
    /// plays a music for fun purpose
    /// </summary>
    public class Musique:MonoBehaviour
    {
        public AudioSource stingSource;

        void Start()
        {
            AudioSource stingSource = GetComponent<AudioSource>();
            stingSource.Play();
        }
    }
}
