using UnityEngine;

namespace Assets
{
    /// <summary>
    /// plays a music for fun purpose
    /// </summary>
    public class Musique:MonoBehaviour
    {
        public AudioSource singSource;

        void Start()
        {
            singSource = GetComponent<AudioSource>();
            singSource.Play();
        }
    }
}
