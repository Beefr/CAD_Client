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
    class GetSelected : MonoBehaviour
    {
        private GameObject myObject;
        private int layerMask;
        private RaycastHit hit;
        private LineRenderer line;
        
        
        private Transform hand;
        private Transform index;
        private RaycastHit hit2;
        
        private Camera cameraVR;
        private GameObject playerController;


        private int clicked = 0;

        private UiDesigner designer;

        private Light[] lums; // to get the client

        void Start()
        {
            try
            {
                File.Delete(@"Content.txt");
            } catch (Exception e) { /* y a pas le fichier */ }

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

       

        void Update()
        {
            if (!lums[0].GetComponent<NCClient>().IsVREnabled())
            {

                line.enabled = false;
                // Bit shift the index of the layer (8) to get a bit mask
                layerMask = 1 << 1;

                // This would cast rays only against colliders in layer 8.
                // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
                layerMask = ~layerMask;
                

                var ray = cameraVR.ScreenPointToRay(Input.mousePosition);


                Debug.DrawRay(ray.origin, ray.direction * 100f/*hit.distance*/, Color.yellow);
                if (Input.GetMouseButton(0))
                {

                    // Does the ray intersect any objects excluding the player layer
                    if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
                    {
                        if (hit.collider.gameObject == myObject)
                        {
                            try
                            {

                                myObject.GetComponent<Renderer>().material.color = Color.red; // RED INDICATES THAT THE OBJECT HAS BEEN SELECTED

                                //Debug.Log("Did Hit"+ myObject.name);
                                lums[0].GetComponent<NCClient>().SendObjectCaracteristics(myObject);
                                //Debug.Log("Did Hit");
                            }
                            catch (Exception) { }
                            //Debug.Log("Did Hit");
                        }
                    }

                }//*/

            }
            else // ______________________ IF VR IS ENABLED ______________________________________
            {
                clicked++;
                line.enabled = true;
                if (hand != null)
                {
                    line.SetPosition(0, index.transform.position);// + new Vector3(0.05f, 0f, 0f));

                    if (Physics.Raycast(index.transform.position, index.transform.forward, out hit2))
                    {

                        //Debug.DrawRay(hand.transform.position, hand.transform.forward * hit2.distance, Color.yellow);
                        if (hit2.collider.gameObject == myObject)
                        {
                            if (myObject.tag != "Sol") // le sol on le selectionne pas
                            {

                                //line.SetPosition(1, hand.transform.forward * hit2.distance);
                                try
                                {
                                    myObject.GetComponent<Renderer>().material.color = Color.red; // RED INDICATES THAT THE OBJECT HAS BEEN SELECTED
                                }catch (Exception e) {  /* no renderer attached to object */ }


                                if ((OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.RawButton.A)) && clicked > 50)
                                {
                                    //Debug.Log("Did Hit"+ myObject.name);
                                    if (OVRInput.Get(OVRInput.Button.One))
                                    {
                                        lums[0].GetComponent<NCClient>().SendObjectCaracteristics(myObject);
                                    }

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
                            myObject.GetComponent<Renderer>().material.color = Color.white;
                        }
                        catch (Exception e) { /* no renderer attached//*/ }
                    }

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
                //int ID = obj.GetComponent<AdditionnalProperties>().ID;
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
