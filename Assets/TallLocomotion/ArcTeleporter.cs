using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://developer.oculus.com/blog/teleport-curves-with-the-gear-vr-controller/
public class ArcTeleporter : MonoBehaviour {
	public enum UpDirection { World, TargetNormal};

	[Tooltip("Raycaster used for teleportation")]
	public ArcRaycaster arcRaycaster;
    [Tooltip("What object to teleport")]
    public Transform objectToMove;
    [Tooltip("The origin of the raycast")]
    public Transform originRaycast;

    [Tooltip("Height of object being teleported. How far off the ground the object should land.")]
	public float height = 1.29f;
	[Tooltip("When teleporting, should object be aligned with the world or destination")]
	public UpDirection teleportedUpAxis = UpDirection.World;

	// Used to buffer trigger
	protected bool lastTriggerState = false;

	void Awake() {
		if (arcRaycaster == null) {
			arcRaycaster = GetComponent<ArcRaycaster> ();
		}
		if (arcRaycaster == null) {
			Debug.LogError ("ArcTeleporter's Arc Ray Caster is not set");
        }
        if (objectToMove == null)
        {
            Debug.LogError("ArcTeleporter's target object is not set");
        }
        if (originRaycast == null)
        {
            Debug.LogError("No source set for raycast");
        }
    }

	void Update () {
		/*if (!HasController)
        {
            Debug.Log("salut");
            return; 
		}//*/
        
		bool currentTriggerState = OVRInput.Get (OVRInput.Button.PrimaryIndexTrigger) // oculus 
            || OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger)>0 // oculus ...?
            || OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger) > 0;// oculkus rift s


		// If the trigger was released this frame
		if (lastTriggerState && !currentTriggerState) {
			Vector3 forward = originRaycast.forward;
			Vector3 up = Vector3.up;

			// If there is a valid raycast
			if (arcRaycaster!= null && arcRaycaster.MakingContact) {
				if (objectToMove != null) {
					if (teleportedUpAxis == UpDirection.TargetNormal) {
						up = arcRaycaster.Normal;
					}
					objectToMove.position = arcRaycaster.HitPoint + up * height;
				}
			}

			if (OVRInput.Get (OVRInput.Touch.PrimaryTouchpad) || OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger)) {
				forward = TouchpadDirection;
			}

			objectToMove.rotation = Quaternion.LookRotation (forward, up);
		}

		lastTriggerState = currentTriggerState;
	}


    /// <summary>
    /// the controllers in use
    /// </summary>
	OVRInput.Controller Controller {
		get {
            /*OVRInput.Controller controllers = OVRInput.GetConnectedControllers ();
			if ((controllers & OVRInput.Controller.LTrackedRemote) == OVRInput.Controller.LTrackedRemote) {
				return OVRInput.Controller.LTrackedRemote;
			}
			if ((controllers & OVRInput.Controller.RTrackedRemote) == OVRInput.Controller.RTrackedRemote) {
				return OVRInput.Controller.RTrackedRemote;
			}
            return OVRInput.Controller.None;//*/


            try
            {
                return OVRInput.Controller.LTrackedRemote; //OVRInput.GetActiveController ();
            }
            catch (Exception e) { return OVRInput.Controller.None; }
        }
	}

    /// <summary>
    /// return true if there is at least one controller
    /// </summary>
	bool HasController {
		get {
			OVRInput.Controller controllers = OVRInput.GetConnectedControllers ();
			if ((controllers & OVRInput.Controller.LTrackedRemote) == OVRInput.Controller.LTrackedRemote) {
				return true;
			}/*
			if ((controllers & OVRInput.Controller.RTrackedRemote) == OVRInput.Controller.RTrackedRemote) {
				return true;
			}//*/
			return false;
		}
	}

    /// <summary>
    ///  returns a 4x4 matrix in world coordinates corresponding to the coordinates of the controller
    /// </summary>
	Matrix4x4 ControllerToWorldMatrix {
		get {
			if (!HasController) {
				return Matrix4x4.identity;
			}

			Matrix4x4 localToWorld = arcRaycaster.trackingSpace.localToWorldMatrix;

			Quaternion orientation = OVRInput.GetLocalControllerRotation(Controller);
            //Vector3 position = OVRInput.GetLocalControllerPosition (Controller);
            Vector3 position = GameObject.Find("OVRPlayerController").transform.Find("OVRCameraRig").Find("TrackingSpace").Find("LeftHandAnchor").Find("IndexGauche").transform.position;

            Matrix4x4 local = Matrix4x4.TRS (position, orientation, Vector3.one);

			Matrix4x4 world = local * localToWorld;

			return world;
		}
	}


    /// <summary>
    /// direction of the left index to cast the ray
    /// </summary>
	Vector3 TouchpadDirection {
		get {
            /*
            Vector2 touch = OVRInput.Get(OVRInput.Axis2D.PrimaryTouchpad);
            Vector3 forward = new Vector3 (touch.x, 0.0f, touch.y).normalized;
            //*/
            Vector3 forward = GameObject.Find("OVRPlayerController").transform.Find("OVRCameraRig").Find("TrackingSpace").Find("LeftHandAnchor").Find("IndexGauche").transform.forward;

            forward = ControllerToWorldMatrix.MultiplyVector (forward);
			forward = Vector3.ProjectOnPlane (forward, Vector3.up);
			return forward.normalized;
		}
	}
}
