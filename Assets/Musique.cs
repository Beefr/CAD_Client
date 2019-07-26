using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    public class Musique:MonoBehaviour
    {
        public AudioSource stingSource;
        //public AudioClip[] stings;

        void Start()
        {
            /*stings.Append(Resources.Load("Assets/Musique/Jump.mp3"));
            int randClip = UnityEngine.Random.Range(0, stings.Length);
            stingSource.clip = stings[randClip];//*/
            AudioSource stingSource = GetComponent<AudioSource>();
            //stingSource.clip = (AudioClip)Resources.Load("Assets/Musique/Jump.mp3");
            stingSource.Play();
        }
    }
}
