using System;
using UnityEngine;

namespace Assets
{
    /// <summary>
    /// to give to your objects in order to be able to select them 
    /// </summary>
    class GetSelected : MonoBehaviour
    {
        private GameObject myObject; // the object
        private LineRenderer line; // the line that you see from your right index
        
        private Transform index; // your index
        
        
        /// <summary>
        /// initializing everything
        /// </summary>
        void Start()
        {
            index = GameObject.Find("OVRPlayerController").transform.Find("OVRCameraRig").Find("TrackingSpace").Find("RightHandAnchor").Find("Index");
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
            if (!GameObject.Find("VRHandler").GetComponent<VRHandler>().IsVREnabled)
            {

                line.enabled = false;

                // Bit shift the index of the layer (8) to get a bit mask
                int layerMask = 1 << 1;
                // This would cast rays only against colliders in layer 8. EDIT: no, we raycast everything now
                // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
                layerMask = ~layerMask;

                // the ray direction 
                var ray = GameObject.Find("OVRPlayerController").transform.Find("OVRCameraRig").Find("TrackingSpace").Find("CenterEyeAnchor").GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
                // drawing the ray in debug mode
                Debug.DrawRay(ray.origin, ray.direction * 100f/*hit.distance*/, Color.yellow);

                // if we press the mouse
                if (Input.GetMouseButton(0))
                {
                    RaycastHit hit;
                    // Does the ray intersect any objects excluding the player layer
                    if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
                    {
                        // if we hit the object on which we attached this script
                        if (hit.collider.gameObject == myObject && myObject.tag != "Unselectable") // we musn't get selected
                        {
                            try
                            {
                                // RED INDICATES THAT THE OBJECT HAS BEEN SELECTED
                                myObject.GetComponent<Renderer>().material.color = Color.red;

                                // messaging the server with the object's caracteristics
                                GameObject.Find("Main").GetComponent<Client>().SendObjectCaracteristics(myObject);
                            }
                            catch (Exception) { }
                        }
                    }

                }

            }
            else // ______________________ IF VR IS ENABLED ______________________________________
            {
                line.enabled = true; // enabling the line renderer
                // we need a hand otherwise you can easily understand that there will be a problem with the raycast
                if (GameObject.Find("OVRPlayerController").transform.Find("OVRCameraRig").Find("TrackingSpace").Find("RightHandAnchor") != null)
                {
                    // drawing the line (origin)
                    line.SetPosition(0, index.transform.position);
                    RaycastHit hit2;
                    // Does the ray intersect any objects excluding the player layer
                    if (Physics.Raycast(index.transform.position, index.transform.forward, out hit2))
                    {
                        // if we hit the object on which we attached this script
                        if (hit2.collider.gameObject == myObject)
                        {
                            // why would we select the ground ??
                            if (myObject.tag != "Sol" && myObject.tag!="Unselectable") 
                            {
                                try
                                {
                                    // RED INDICATES THAT THE OBJECT HAS BEEN SELECTED
                                    myObject.GetComponent<Renderer>().material.color = Color.red; 
                                }catch (Exception e) {  /* no renderer attached to object */ }

                                // if we press a button of the oculus
                                if ((OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.RawButton.A)))
                                {
                                    // messaging the server with the object's caracteristics
                                    GameObject.Find("Main").GetComponent<Client>().SendObjectCaracteristics(myObject);

                                    // _________________ MODIFY CARACS IN VR ______________________________
                                    Manager manager = GameObject.Find("Manager").GetComponent<Manager>();
                                    manager.SetObj(myObject);
                                    manager.TryDesigning(); // only the UI should be modified
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

       

        


    }
}
