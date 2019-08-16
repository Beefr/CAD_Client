using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using System;

namespace Assets
{
    /// <summary>
    /// to use VR, attached to ovrplayercontroller
    /// </summary>
    public class VRHandler : MonoBehaviour
    {
        public bool IsVREnabled { get; private set; } = true;

        public GameObject canvaPerso = null; // for the canva attached to u

        // some variables to de/activate things
        private int timingBetweenEachSecondaryIndexTrigger = 0;
        private int turnUI = 0;
        public float rotationSpeed = 0.5f;
        public int UIRotationSpeed = 1;

        /// <summary>
        /// to find the personnal canva
        /// </summary>
        void Start()
        { 
            if (canvaPerso==null)
            {
                try
                {
                    canvaPerso = FindGameObjectInChildWithTag(GameObject.Find("OVRPlayerController"), "SelectableUI");
                }
                catch (Exception) { /*no personal canva*/ }
            }
        }



        /// <summary>
        /// enables vr
        /// </summary>
        public void EnableVR()
        {
            if (XRSettings.loadedDeviceName!="")
            {
                IsVREnabled = true;
                StartCoroutine(LoadDevice(XRSettings.loadedDeviceName, true));
            } else { /* can't enable vr*/ }
        }

        /// <summary>
        /// disables vr
        /// </summary>
        public void DisableVR()
        {
            IsVREnabled = false;
            StartCoroutine(LoadDevice("", false));
        }

        /// <summary>
        /// toggle the vr
        /// </summary>
        public void ToggleVR()
        {
            if (IsVREnabled == false) { EnableVR(); }
            else { DisableVR(); }
        }

        /// <summary>
        /// load the device connected
        /// </summary>
        /// <param name="newDevice"></param>
        /// <param name="enable"></param>
        /// <returns></returns>
        IEnumerator LoadDevice(string newDevice, bool enable)
        {
            XRSettings.LoadDeviceByName(newDevice);
            yield return null;
            XRSettings.enabled = enable;
        } // https://stackoverflow.com/questions/36702228/enable-disable-vr-from-code

      

        
        /// <summary>
        /// checks if keys are pressed or not
        /// it allows to enable/disable VR 
        /// to create elements by asking the server to
        /// to rotate the UI around you
        /// to rotate the camera
        /// </summary>
        void Update()
        {

            // _______________________ ENABLING VR _____________________________________
            if ((Input.GetKeyDown(KeyCode.P) || OVRInput.Get(OVRInput.Button.Two) || OVRInput.Get(OVRInput.RawButton.B)))
            {
                //ToggleVR();
            }
            
            // if vr is enabled
            if (IsVREnabled)
            {
                timingBetweenEachSecondaryIndexTrigger++;
                // it sends a notification to the server, with the key pressed, and it sends back a notifications requesting the creation of an object
                if ( (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > 0 || OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger)>0) && timingBetweenEachSecondaryIndexTrigger > 25)
                {
                    GameObject.Find("Main").GetComponent<Client>().SendKeyDownIndication("SecondaryIndexTrigger");

                    timingBetweenEachSecondaryIndexTrigger = 0;
                }

                // MOVING THE UI AROUND U
                turnUI++;
                if ((OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger)>0 || OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger) >0) && turnUI > 25)
                {
                    canvaPerso.transform.RotateAround(GameObject.Find("OVRPlayerController").transform.position, GameObject.Find("OVRPlayerController").transform.up, -UIRotationSpeed);

                    turnUI = 0;
                }
                if ((OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0 || OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger)>0) && turnUI > 25)
                {
                    canvaPerso.transform.RotateAround(GameObject.Find("OVRPlayerController").transform.position, GameObject.Find("OVRPlayerController").transform.up, UIRotationSpeed);

                    turnUI = 0;
                }



                // _________________ ROTATION of the camera (because if YOU rotate, then the camera rotates also __________________________________________________________
                if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft) || GoingLeft(OVRInput.Get(OVRInput.RawAxis2D.RThumbstick)) || Input.GetKeyDown(KeyCode.A))
                {
                    GameObject.Find("OVRPlayerController").transform.Rotate(-GameObject.Find("OVRPlayerController").transform.up * rotationSpeed, Space.Self);
                }
                if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight) || GoingRight(OVRInput.Get(OVRInput.RawAxis2D.RThumbstick)) || Input.GetKeyDown(KeyCode.E))
                {
                    GameObject.Find("OVRPlayerController").transform.Rotate(GameObject.Find("OVRPlayerController").transform.up * rotationSpeed, Space.Self);
                }
                
                

            }

        }

        /// <summary>
        /// function to get the first child with this tag
        /// </summary>
        /// <param name="parent">parent of the child</param>
        /// <param name="tag">the tag</param>
        /// <returns>the child you want</returns>
        public static GameObject FindGameObjectInChildWithTag(GameObject parent, string tag)
        {
            Transform parentTransform = parent.transform;

            for (int i = 0; i < parentTransform.childCount; i++)
            {
                if (parentTransform.GetChild(i).gameObject.tag == tag)
                {
                    return parentTransform.GetChild(i).gameObject;
                }

            }

            return null;
        }

        /// <summary>
        /// checks if the vector is >0
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private bool GoingRight(Vector2 v)
        {

            if (v.x > 0)
                return true;

            return false;
        }//*/

        /// <summary>
        /// checks if the vector <0
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private bool GoingLeft(Vector2 v)
        {

            if (v.x < 0)
                return true;

            return false;
        }








    }
}
