
using System.Drawing;
using UnityEngine;

namespace Assets
{


    public class MyCamera : MonoBehaviour
    {
        

        [SerializeField]
        private float rotateHorizontal;


        [SerializeField]
        private float rotateVertical;
        
        private float angleV = 0;
        private float maxAngleV = 5f;
        private float minAngleV = 0;


        //private Camera cam;
        //private Camera cameraVR;
        private GameObject playerController;
        private Light[] lums;

        private int freezeV = 0;
        private int freezeH = 0;

        void Start()
        {

            //cam = GameObject.Find("Main Camera").GetComponent<Camera>();
            playerController = GameObject.Find("OVRPlayerController");
            //cameraVR = playerController.transform.Find("OVRCameraRig").Find("TrackingSpace").Find("CenterEyeAnchor").GetComponent<Camera>();
            lums = Light.GetLights(LightType.Directional, 0);
        }

        
        public void Rotation()
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
        }//*/


        void Update()
        {
            // only enabled in non-vr mode or may cause u to "feel bad mister stark"
            if (!lums[0].GetComponent<NCClient>().IsVREnabled())
            {
                freezeH++;
                freezeV++;

                if (Input.GetMouseButtonDown(0))
                {

                }


                if (Input.GetMouseButton(0))
                {
                    if (freezeH > 25)
                    {
                        rotateHorizontal = rotateHorizontal + Input.GetAxis("Mouse X");
                        //Debug.Log(rotateHorizontal);
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



            if (Input.GetKeyDown(KeyCode.Z))
                playerController.transform.Translate(new Vector3(0f, 0.0f, 1f)); // we can't stop rendering the body so we move it with us

            if (Input.GetKeyDown(KeyCode.D))
                playerController.transform.Translate(new Vector3(1f, 0.0f, 0f));

            if (Input.GetKeyDown(KeyCode.Q))
                playerController.transform.Translate(new Vector3(-1f, 0.0f, 0f));

            if (Input.GetKeyDown(KeyCode.S))
                playerController.transform.Translate(new Vector3(0f, 0.0f, -1f));


               

            }//*/
        }
    }
}
