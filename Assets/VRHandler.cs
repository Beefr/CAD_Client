﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    public class VRHandler : MonoBehaviour
    {

        private int alreadyCaptured = 0;

        private GameObject playerController;
        //private Component forwardDirection;
        //private Rigidbody rigidBody;
        //private OVRCameraRig camera;
        
        private float rotationSpeed = 0.5f;

        private Transform indexGauche;
        private Transform hand;
        private LineRenderer lineGauche;
        private RaycastHit hit2;

        private Light[] lums; // to get the client

        private TMP_Text txtPerso;

        private int toggleVR = 0;
        private int turnUI = 0;
        

        void Start()
        {
            lums = Light.GetLights(LightType.Directional, 0);


            playerController = GameObject.Find("OVRPlayerController");

            hand = playerController.transform.Find("OVRCameraRig").Find("TrackingSpace").Find("LeftHandAnchor");
            indexGauche = hand.Find("IndexGauche");
            
            
            lineGauche = indexGauche.GetComponent<LineRenderer>();
            lineGauche.startWidth = 0.01f;
            lineGauche.endWidth = 0.01f;
            lineGauche.startColor = Color.red;
            lineGauche.endColor = Color.red;
            //lineGauche.SetPosition(0, indexGauche.transform.position);// + new Vector3(0.05f, 0f, 0f));
            

            //System.IO.File.WriteAllText(@"visible.txt", lineGauche.isVisible.ToString());
            //*/

            txtPerso = GameObject.Find("CanvasPerso").GetComponent<TMP_Text>();
            //txtPerso.color
        }

        void Update()
        {

            // _______________________ ENABLING VR _____________________________________
            toggleVR++;
            if ((Input.GetKeyDown(KeyCode.P) || OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.RawButton.B)) && toggleVR > 25)
            {
                //Debug.Log(lums[0].GetComponent<NCClient>().IsVREnabled());
                if (!lums[0].GetComponent<NCClient>().IsVREnabled())
                {
                    lums[0].GetComponent<NCClient>().EnableVR();
                }
                else
                {
                    lums[0].GetComponent<NCClient>().DisableVR();
                }

                toggleVR = 0;
            }

            //Debug.Log(lums[0].GetComponent<NCClient>().IsVREnabled());
            if (lums[0].GetComponent<NCClient>().IsVREnabled())
            {

               alreadyCaptured++;
                // it sends a notification to the server, with the key pressed, and it sends back a notifications requesting the creation of an object
                if ( (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0 || OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger)>0) && alreadyCaptured > 25)
                {
                    //GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    //Camera.main.GetComponent<NCClient>().SendKeyDownIndication("PrimaryIndexTrigger");

                    lums[0].GetComponent<NCClient>().SendKeyDownIndication("SecondaryIndexTrigger");
                    // ATTENTION S IL Y A PLUSIEURS LUMIERES

                    alreadyCaptured = 0;
                }

                // MOVING THE UI AROUND U
                turnUI++;
                if ((OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger)>0 || OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger) >0) && turnUI > 25)
                {
                    TMP_Text txtPerso = GameObject.Find("CanvasPerso").GetComponent<TMP_Text>();
                    txtPerso.transform.RotateAround(playerController.transform.position, -Vector3.up, 30);

                    turnUI = 0;
                }
                if ((OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0 || OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger)>0) && turnUI > 25)
                {
                    TMP_Text txtPerso = GameObject.Find("CanvasPerso").GetComponent<TMP_Text>();
                    txtPerso.transform.RotateAround(playerController.transform.position, Vector3.up, 30);

                    turnUI = 0;
                }



                // _________________ ROTATION __________________________________________________________
                if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft) || GoingLeft(OVRInput.Get(OVRInput.RawAxis2D.RThumbstick)) || Input.GetKeyDown(KeyCode.A))
                {
                    playerController.transform.Rotate(-playerController.transform.up * rotationSpeed, Space.Self);
                    //rigidBody.transform.Rotate(-rigidBody.transform.up * rotationSpeed);
                }
                if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight) || GoingRight(OVRInput.Get(OVRInput.RawAxis2D.RThumbstick)) || Input.GetKeyDown(KeyCode.E))
                {
                    playerController.transform.Rotate(playerController.transform.up * rotationSpeed, Space.Self);
                    //rigidBody.transform.Rotate(rigidBody.transform.up * rotationSpeed);
                }
                //Debug.Log(playerController.transform.forward);




               


                

            }

        }

        private bool GoingRight(Vector2 v)
        {

            if (v.x > 0)
                return true;

            return false;
        }

        private bool GoingLeft(Vector2 v)
        {

            if (v.x < 0)
                return true;

            return false;
        }








    }
}
