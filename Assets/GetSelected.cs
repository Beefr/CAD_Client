using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    /// <summary>
    /// to give to your objects in order to be able to select them 
    /// </summary>
    class GetSelected : MonoBehaviour
    {
        private GameObject myObject; // the object
        private int layerMask; // the mask for the selection
        private RaycastHit hit; // the ray for mouse selection
        private LineRenderer line; // the line that you see from your right index
        
        
        private Transform hand; // your hand
        private Transform index; // your index
        private RaycastHit hit2;// the ray 2 for vr selection

        private Camera cameraVR; // the camera
        private GameObject playerController; // you


        private int clicked = 0; // a sort of timer

        private UiDesigner designer; // the ui designer

        private Light[] lums; // to get the client


        /// <summary>
        /// initializing everything
        /// </summary>
        void Start()
        {
            try
            {
                File.Delete(@"Content.txt");
            } catch (Exception e) { /* file not existing */ }

            designer = GameObject.Find("UiDesigner").GetComponent<UiDesigner>();

            playerController = GameObject.Find("OVRPlayerController");
            cameraVR = playerController.transform.Find("OVRCameraRig").Find("TrackingSpace").Find("CenterEyeAnchor").GetComponent<Camera>();

            lums = Light.GetLights(LightType.Directional, 0);
            
            hand = playerController.transform.Find("OVRCameraRig").Find("TrackingSpace").Find("RightHandAnchor");
            index = hand.Find("Index");
            if (line == null)
            {
                myObject = this.gameObject;

                line = index.GetComponent<LineRenderer>();
                line.startWidth = 0.01f;
                line.endWidth = 0.01f;
                line.startColor = Color.red;
                line.endColor = Color.red;
                line.SetPosition(0, index.transform.position);
                
               
            }
            
        }

       
        /// <summary>
        /// every tick it updates the raycast
        /// </summary>
        void Update()
        {
            // in case vr is not enabled
            if (!lums[0].GetComponent<NCClient>().IsVREnabled())
            {

                line.enabled = false;

                // Bit shift the index of the layer (8) to get a bit mask
                layerMask = 1 << 1;
                // This would cast rays only against colliders in layer 8. EDIT: no, we raycast everything now
                // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
                layerMask = ~layerMask;

                // the ray direction 
                var ray = cameraVR.ScreenPointToRay(Input.mousePosition);
                // drawing the ray in debug mode
                Debug.DrawRay(ray.origin, ray.direction * 100f/*hit.distance*/, Color.yellow);

                // if we press the mouse
                if (Input.GetMouseButton(0))
                {
                    // Does the ray intersect any objects excluding the player layer
                    if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
                    {
                        // if we hit the object on which we attached this script
                        if (hit.collider.gameObject == myObject)
                        {
                            try
                            {
                                // RED INDICATES THAT THE OBJECT HAS BEEN SELECTED
                                myObject.GetComponent<Renderer>().material.color = Color.red; 

                                // messaging the server with the object's caracteristics
                                lums[0].GetComponent<NCClient>().SendObjectCaracteristics(myObject);
                            }
                            catch (Exception) { }
                        }
                    }

                }

            }
            else // ______________________ IF VR IS ENABLED ______________________________________
            {
                clicked++; // updating the timer
                line.enabled = true; // enabling the line renderer
                // we need a hand otherwise you can easily understand that there will be a problem with the raycast
                if (hand != null)
                {
                    // drawing the line (origin)
                    line.SetPosition(0, index.transform.position);

                    // Does the ray intersect any objects excluding the player layer
                    if (Physics.Raycast(index.transform.position, index.transform.forward, out hit2))
                    {
                        // if we hit the object on which we attached this script
                        if (hit2.collider.gameObject == myObject)
                        {
                            // why would we select the ground ??
                            if (myObject.tag != "Sol") 
                            {
                                try
                                {
                                    // RED INDICATES THAT THE OBJECT HAS BEEN SELECTED
                                    myObject.GetComponent<Renderer>().material.color = Color.red; 
                                }catch (Exception e) {  /* no renderer attached to object */ }

                                // if we press a button of the oculus
                                if ((OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.RawButton.A)) && clicked > 50)
                                {
                                    // messaging the server with the object's caracteristics
                                    lums[0].GetComponent<NCClient>().SendObjectCaracteristics(myObject);

                                    // _________________ MODIFY CARACS IN VR ______________________________
                                    designer.setObj(myObject);
                                    convertObjectToString(myObject);
                                    while (IsFileLocked(new FileInfo(@"Content.txt")))
                                    {
                                        Thread.Sleep(20);
                                    }
                                    designer.convertStringToCanvas();
                                    clicked = 0;
                                }
                            }
                        } 
                    }
                    else
                    {
                        try
                        {
                            // resetting the color to white as it is not selected anymore
                            myObject.GetComponent<Renderer>().material.color = Color.white;
                        }
                        catch (Exception e) { /* no renderer attached//*/ }
                    }
                    // drawing the line (end)
                    line.SetPosition(1, index.transform.forward * 100f);
                }
            }
        }

        /// <summary>
        /// check if file is accessible 
        /// </summary>
        /// <param name="file">the FileInfo of the txt file</param>
        /// <returns></returns>
        private bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            //file is not locked
            return false;
        } // https://stackoverflow.com/questions/876473/is-there-a-way-to-check-if-a-file-is-in-use/937558#937558

        /// <summary>
        /// take an object and convert his properties into a txt file named Content.txt
        /// </summary>
        /// <param name="obj"> the object that is gonna be converted</param>
        private void convertObjectToString(GameObject obj)
        {
            try
            {
                // serialize the object
                MyGameObject myObj = new MyGameObject(obj);
                DataContractSerializer DCS = new DataContractSerializer(myObj.GetType());
                MemoryStream streamer = new MemoryStream();
                DCS.WriteObject(streamer, myObj);
                streamer.Seek(0, SeekOrigin.Begin);
                string content = XElement.Parse(Encoding.ASCII.GetString(streamer.GetBuffer()).Replace("\0", "")).ToString();
                
                System.IO.File.WriteAllText(@"Content.txt", content);
            }
            catch (Exception e) { Debug.Log(e); }
        }














    }
}
