﻿
using System.Drawing;
using UnityEngine;

namespace Assets
{
    public class MyCamera : MonoBehaviour
    {
        [SerializeField]
        private float rotateHorizontal; // angle horizontal
        
        [SerializeField]
        private float rotateVertical; // angle vertical
        
        private float angleV = 0; // vertical angle
        public float maxAngleV = 5f; // maximum height
        public float minAngleV = 0; // min height
        
        public GameObject playerController;

        // timers
        private int freezeV = 0; 
        private int freezeH = 0;

        /// <summary>
        /// initialize everything
        /// </summary>
        void Start()
        {
            if (playerController==null)
                playerController = GameObject.Find("OVRPlayerController");
        }

        /// <summary>
        /// rotate the camera vertically or horizontally (not both on the same time)
        /// </summary>
        private void Rotation()
        {
            if (Mathf.Abs(rotateHorizontal) > Mathf.Abs(rotateVertical) ) 
            {
                if(Mathf.Abs(rotateHorizontal) < 1)// <1 is here to prevent the camera of doing a 180° loop for no fking reason, dont remove it
                    playerController.transform.RotateAround(playerController.transform.position, Vector3.up, rotateHorizontal * 1000.3f * Time.deltaTime);
            }
            else 
            {
                if (Mathf.Abs(rotateVertical) < 1)
                {
                    if (rotateVertical + angleV > maxAngleV)
                        rotateVertical = maxAngleV - angleV;
                    if (rotateVertical + angleV < minAngleV)
                        rotateVertical = minAngleV - angleV;

                    angleV = angleV + rotateVertical;

                    playerController.transform.RotateAround(playerController.transform.position, playerController.transform.right, -rotateVertical * 1000.3f * Time.deltaTime);
                }
            }
            rotateHorizontal = 0;
            rotateVertical = 0;
        }

        /// <summary>
        /// to rotate the camera in non-vr mode
        /// </summary>
        void Update()
        {
            // only enabled in non-vr mode 
            if (!GameObject.Find("VRHandler").GetComponent<VRHandler>().IsVREnabled)
            {
                freezeH++;
                freezeV++;
                
                // if u press the button
                if (Input.GetMouseButton(0))
                {
                    if (freezeH > 25)
                    {
                        rotateHorizontal = rotateHorizontal + Input.GetAxis("Mouse X");
                        if (rotateHorizontal> rotateVertical && Mathf.Abs(rotateHorizontal) >0.2)
                            freezeV = 0;
                    }

                    if (freezeV > 25)
                    {
                        rotateVertical = rotateVertical + Input.GetAxis("Mouse Y");
                        if (rotateHorizontal < rotateVertical && Mathf.Abs(rotateVertical) > 0.2)
                            freezeH = 0;
                    }

                    Rotation();
                }


                // move 
                if (Input.GetKeyDown(KeyCode.Z))
                    playerController.transform.Translate(new Vector3(0f, 0.0f, 1f)); 

                if (Input.GetKeyDown(KeyCode.D))
                    playerController.transform.Translate(new Vector3(1f, 0.0f, 0f));

                if (Input.GetKeyDown(KeyCode.Q))
                    playerController.transform.Translate(new Vector3(-1f, 0.0f, 0f));

                if (Input.GetKeyDown(KeyCode.S))
                    playerController.transform.Translate(new Vector3(0f, 0.0f, -1f));


               

            }
        }
    }
}
