using Assets;
using Elements;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet.DPSBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;
using TMPro;
using UnitsNet;
using UnitsNet.Serialization.JsonNet;
using UnityEngine;
using static OVRInput;
using UnityEngine.XR;
using System.Collections;
using UnityEngine.EventSystems;
using System.Xml;

public class NCClient : MonoBehaviour
{
    private static int position = 1;
    private TCPConnection serverConnection;
    private ConnectionInfo connectionInfo;
    private int port = 10000;
    private string ipAddress = "127.0.0.1";
    private Communication message;
    private CommunicateUpdate messageUpdate;
    private SendReceiveOptions customSendReceiveOptions;
    private bool firstConnexion = true;
    private bool lookingForServer = true;
    private int connectionTry = 0;

    // to avoid crashes at runtime (https://docs.unity3d.com/ScriptReference/GameObject.CreatePrimitive.html)
    private MeshFilter mf;
    private MeshRenderer mr;
    private BoxCollider bc;
    private SphereCollider sc;
    private List<Length> ll=new List<Length>(); //https://forum.unity.com/threads/json-net-for-unity.200336/page-35
    
    private List<BaseElement> constructionQueue = new List<BaseElement>();
    private JsonSerializerSettings _jsonSerializerSettings;
    private bool found = false;
    private DataContractSerializer dcs;

    private int currentID = 0;



    private bool VR_enabled = true;

    private GameObject playerController;

    private bool clientOff = false;
    private string deviceName ="Oculus";


    // Start is called before the first frame update
    void Start()
    {
        if (XRSettings.loadedDeviceName!="")
            deviceName = XRSettings.loadedDeviceName;
        System.IO.File.WriteAllText(@"deviceName.txt", deviceName);

        

        playerController = GameObject.Find("OVRPlayerController");

        PrepareTheDeserializer();

        /*
        object[] obj = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (object o in obj)
        {
            GameObject g = (GameObject)o;
            try
            {
                g.GetComponent<Renderer>().material.color = new Color(9f, 0f, 0f, 1f); // RED INDICATES THAT THE COLOUR IS THE ONLY THING THAT WORKED
            } catch (Exception) { }
        }//*/

        // handler for messages
        try
        {
            customSendReceiveOptions = new SendReceiveOptions<ProtobufSerializer>();
            
            //Create a connectionInfo object that specifies the target server
            //This line assumes the server is on the local machine - 127.0.0.1
            //This line also assumes that the server is listening on port 10000 (check this is the case).
            connectionInfo = new ConnectionInfo(ipAddress, port);

            //Get a connection with the specified connectionInfo
            serverConnection = TCPConnection.GetConnection(connectionInfo, customSendReceiveOptions);
            serverConnection.AppendShutdownHandler(ClientDisconnected);
            serverConnection.AppendIncomingPacketHandler<Communication>("Communication", HandleIncomingChatMessage, customSendReceiveOptions);
            
            // get the deplacement instructions // use MyCamera.cs instead
            //serverConnection.AppendIncomingPacketHandler<string>("Deplacement", HandleIncomingDeplacement);

            // get the objects 
            serverConnection.AppendIncomingPacketHandler<CommunicateElement>("Element", HandleIncomingObject, customSendReceiveOptions);

            // get an update on an object
            serverConnection.AppendIncomingPacketHandler<CommunicateUpdate>("Update", HandleIncomingObjectUpdate, customSendReceiveOptions);
        }
        catch (Exception)
        {
            //We can decide what to do here if the synchronous send and receive timed out after the specified 1000ms
        }

        // for testing purpose
        Elements.Sphere sphere = new Sphere(1, "Sphere", "Element", 1, 1, true, new Length(1, UnitsNet.Units.LengthUnit.Meter), new Length(1, UnitsNet.Units.LengthUnit.Meter));
        CreateObject(sphere);


        // set the id of all objects that were introduced previously in the scene
        object[] obj = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (object o in obj)
        {
            GameObject g = (GameObject)o;
            try
            {
                int ID = g.GetComponent<AdditionnalProperties>().ID;
                if (ID >= currentID)
                    currentID = ID + 1;
            }
            catch (Exception)
            {
                g.AddComponent<GetSelected>();
                g.AddComponent<AdditionnalProperties>();
                g.GetComponent<AdditionnalProperties>().ID = currentID;
                currentID++;
            }
        }//*/

    }

    public void EnableVR()
    {
        VR_enabled = true;
        StartCoroutine(LoadDevice(deviceName, true));
    }

    public void DisableVR()
    {
        VR_enabled = false;
        StartCoroutine(LoadDevice("", false));
    }

    IEnumerator LoadDevice(string newDevice, bool enable)
    {
        XRSettings.LoadDeviceByName(newDevice);
        yield return null;
        XRSettings.enabled = enable;
    } // https://stackoverflow.com/questions/36702228/enable-disable-vr-from-code


    public bool IsVREnabled() { return VR_enabled; }


    /// <summary>
    /// Performs whatever functions we might so desire when we receive an incoming ChatMessage
    /// </summary>
    /// <param name="header">The PacketHeader corresponding with the received object</param>
    /// <param name="connection">The Connection from which this object was received</param>
    /// <param name="incomingMessage">The incoming ChatMessage we are after</param>
    private void HandleIncomingChatMessage(PacketHeader header, Connection connection, Communication incomingMessage)
    {
        if (incomingMessage.SecretKey == 1234 && NetworkComms.NetworkIdentifier != connection.ConnectionInfo.NetworkIdentifier)
        {
            message = new Communication(NetworkComms.NetworkIdentifier, position, 1234);
            serverConnection.SendObject("Communication", message);
        }//*/
    }


    /// <summary>
    /// Performs whatever functions we might so desire when we receive an incoming ChatMessage
    /// </summary>
    /// <param name="header">The PacketHeader corresponding with the received object</param>
    /// <param name="connection">The Connection from which this object was received</param>
    /// <param name="incomingMessage">The incoming CommunicateElement we are after</param>
    private void HandleIncomingObject(PacketHeader header, Connection connection, CommunicateElement incomingMessage)
    {
        if (incomingMessage.SecretKey == 1234 && NetworkComms.NetworkIdentifier != connection.ConnectionInfo.NetworkIdentifier)
        {
            try
            {
                //System.IO.File.WriteAllText(@"json.txt", incomingMessage.Message);
                
                constructionQueue.Add(JsonConvert.DeserializeObject<BaseElement>(incomingMessage.Message, _jsonSerializerSettings));
                
            }
            catch (Exception e) { System.IO.File.WriteAllText(@"Debug.txt", e.ToString()); }
        }
    //https://stackoverflow.com/questions/4535840/deserialize-json-object-into-dynamic-object-using-json-net
    }

    /// <summary>
    /// Performs whatever functions we might so desire when we receive an incoming ChatMessage
    /// </summary>
    /// <param name="header">The PacketHeader corresponding with the received object</param>
    /// <param name="connection">The Connection from which this object was received</param>
    /// <param name="incomingMessage">The incoming CommunicateElement we are after</param>
    private void HandleIncomingObjectUpdate(PacketHeader header, Connection connection, CommunicateUpdate incomingMessage)
    {

        if (incomingMessage.ID != -1)
        {
            if (incomingMessage.SecretKey == 1234 && NetworkComms.NetworkIdentifier != connection.ConnectionInfo.NetworkIdentifier)
            {
                object[] obj = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
                foreach (object o in obj)
                {
                    
                    GameObject g = (GameObject)o;
                    //System.IO.File.WriteAllText(@"Readme.txt", g.GetComponent<AdditionnalProperties>().ID + "," + incomingMessage.ID);
                    if (g.GetComponent("AdditionnalProperties") != null)
                    {
                        if (g.GetComponent<AdditionnalProperties>().ID == incomingMessage.ID)
                        {
                            string content = incomingMessage.Message;

                            UpdateCanvas(content);

                            // deserialize the object
                            //constructionQueue.Add(JsonConvert.DeserializeObject<BaseElement>(content, _jsonSerializerSettings));
                            string position = GetBetween(content, "<Position", "Position>");
                            string spax = GetBetween(position, "<a:x>", "</a:x>");
                            if (!Int32.TryParse(spax, out int pax))
                            {
                                // dont change value
                            }

                            string spay = GetBetween(position, "<a:y>", "</a:y>");
                            if (!Int32.TryParse(spay, out int pay))
                            {
                                // dont change value
                            }

                            string spaz = GetBetween(position, "<a:z>", "</a:z>");
                            if (!Int32.TryParse(spaz, out int paz))
                            {
                                // dont change value
                            }
                            //_____________________
                            string scale = GetBetween(content, "<Scale", "Scale>");
                            string ssax = GetBetween(scale, "<a:x>", "</a:x>");
                            if (!Int32.TryParse(ssax, out int sax))
                            {
                                // dont change value
                            }

                            string ssay = GetBetween(scale, "<a:y>", "</a:y>");
                            if (!Int32.TryParse(ssay, out int say))
                            {
                                // dont change value
                            }

                            string ssaz = GetBetween(scale, "<a:z>", "</a:z>");
                            if (!Int32.TryParse(ssaz, out int saz))
                            {
                                // dont change value
                            }


                            // and update properties
                            //Destroy(g);
                            g.transform.position = new Vector3(pax, pay, paz);
                            g.transform.localScale = new Vector3(sax, say, saz);






                            break; // get out of the loop
                        }
                    }
                }//*/
            }
        }
    }

    public static string GetBetween(string strSource, string strStart, string strEnd)
    {
        int Start, End;
        if (strSource.Contains(strStart) && strSource.Contains(strEnd))
        {
            Start = strSource.IndexOf(strStart, 0) + strStart.Length;
            End = strSource.IndexOf(strEnd, Start);
            return strSource.Substring(Start, End - Start);
        }
        else
        {
            return "";
        }
    } // credit https://stackoverflow.com/questions/10709821/find-text-in-string-with-c-sharp



    /// <summary>
    /// create an object with the given "theoric" element
    /// </summary>
    /// <param name="c"></param>
    private void CreateObject(BaseElement c)
    {

        GameObject go=null;
        var values = PrimitiveType.GetValues(typeof(PrimitiveType));
        foreach (PrimitiveType pt in values) {
            if (pt.ToString() == c.Designation)
            {
                go = GameObject.CreatePrimitive(pt);
                break;
            }
        }
        //PrimitiveType.
        // Capsule Cube Cylinder Plane Quad Sphere
        if (go != null)
        {
            go.transform.position = new Vector3(-1.0f, -1.0f, -1.0f - position);
            go.AddComponent<GetSelected>();
            go.AddComponent<AdditionnalProperties>();
            go.GetComponent<AdditionnalProperties>().ID = currentID + 1;
            currentID++;

            //var size = go.transform.localScale;
            //size = new Vector3((float)c.Diameter.Value / 2, (float)c.Diameter.Value / 2, (float)c.Height.Value);
            //go.transform.localScale = size;

            position+=2;
        }
    }

 

    static void ClientDisconnected(Connection connection)
    {
        /*GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = new Vector3(1.0f + position, 0, 0);
        position++;*/

        object[] obj = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        foreach (object o in obj)
        {
            GameObject g = (GameObject)o;
            try
            {
                g.GetComponent<Renderer>().material.color = new Color(9f, 0f, 0f, 1f); // RED INDICATES THAT THE CLIENT DISCONNECTED
            }
            catch (Exception) { }
        }//*/
    }

    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            clientOff = !clientOff;

        

        while (constructionQueue.Count != 0)
        {
            CreateObject(constructionQueue[0]);// constructionQueue.Count-1]);
            constructionQueue.RemoveAt(0);// constructionQueue.Count-1);
        }

        if (!clientOff) // if we want the client to do something
        {
            if (serverConnection != null && serverConnection.ConnectionAlive())
            {

                if (firstConnexion)
                {
                    message = new Communication(NetworkComms.NetworkIdentifier, position, 1234);
                    serverConnection.SendObject("InitialClientConnect", message);
                    firstConnexion = false;
                }


                object[] obj = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
                foreach (object o in obj)
                {
                    GameObject g = (GameObject)o;
                    try
                    {
                        g.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 1f);
                        //g.GetComponent<Renderer>().material.color = new Color(0f, 9f, 0f, 1f); // GREEN INDICATES THAT THERE IS A CONNEXION
                    }
                    catch (Exception) { }
                }//*/
            }
            else
            {

                if (lookingForServer) // the server is probably turned off at that moment, so he looks for the server but cannot achieve his mission, even though he accepted it
                {
                    /*
                    object[] obj = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
                    foreach (object o in obj)
                    {
                        GameObject g = (GameObject)o;
                        try
                        {
                            g.GetComponent<Renderer>().material.color = new Color(0f, 0f, 9f, 1f); // BLUE INDICATES THAT IT DIDNT CONNECT BUT THE SCRIPT UPDATE IS WORKING (AT LEAST)
                        }
                        catch (Exception) { }
                    }//*/
                    lookingForServer = false;
                }

                connectionTry++;
                if (connectionTry > 50)
                {
                    try
                    {
                        serverConnection = TCPConnection.GetConnection(connectionInfo, customSendReceiveOptions);
                    }
                    catch (Exception) { serverConnection = null; }
                    connectionTry = 0;
                }

            }
        }

    }

    public void SendObjectCaracteristics(GameObject obj)
    {
        try
        {
            // serialize the object
            int ID = obj.GetComponent<AdditionnalProperties>().ID;
            MyGameObject myObj = new MyGameObject(obj);
            DataContractSerializer DCS = new DataContractSerializer(myObj.GetType());
            MemoryStream streamer = new MemoryStream();
            DCS.WriteObject(streamer, myObj);
            streamer.Seek(0, SeekOrigin.Begin);
            string content = XElement.Parse(Encoding.ASCII.GetString(streamer.GetBuffer()).Replace("\0", "")).ToString();

            string monString = "Name: " + GetBetween(content, "<a:name>", "</a:name>") + "\n\n";
            monString = monString + "<ID \n    " + ID.ToString() + "\nID> \n\n";
            monString = monString + "<Position " + GetBetween(content, "<position xmlns:a=\"http://schemas.datacontract.org/2004/07/UnityEngine\">", "</position>") + "Position> \n\n";
            monString = monString + "<Scale " + GetBetween(content, "<scale xmlns:a=\"http://schemas.datacontract.org/2004/07/UnityEngine\">", "</scale>") + "Scale> \n\n";

            

            //UpdateCanvas(monString);

            System.IO.File.WriteAllText(@"Content.txt", content);

            messageUpdate = new CommunicateUpdate(NetworkComms.NetworkIdentifier, monString, 1234, ID);
            if (!clientOff)
                serverConnection.SendObject("Update", messageUpdate);
            

        }
        catch  (Exception e) { Debug.Log(e);  }

    }

    public void SendKeyDownIndication(string key)
    {
        try
        {
            serverConnection.SendObject(key, 1);
        } catch (Exception e) { Debug.Log("No server connected"); }
    }

    void OnApplicationQuit()
    {
        NetworkComms.Shutdown();
    }

    private void HandleIncomingDeplacement(PacketHeader header, Connection connection, string incomingMessage)
    {
        
        switch (incomingMessage)
        {
            case "top":
                //Camera.current.transform.Translate(new Vector3(0f, 0.0f, 1f));
                playerController.transform.Translate(new Vector3(0f, 0.0f, 1f)); // we can't stop rendering the body so we move it with us
                break;
            case "right":
                playerController.transform.Translate(new Vector3(1f, 0.0f, 0f));
                //Camera.current.transform.Translate(new Vector3(1f, 0.0f, 0f));
                break;
            case "left":
                playerController.transform.Translate(new Vector3(-1f, 0.0f, 0f));
                //Camera.current.transform.Translate(new Vector3(-1f, 0.0f, 0f));
                break;
            case "bot":
                playerController.transform.Translate(new Vector3(0f, 0.0f, -1f));
                //Camera.current.transform.Translate(new Vector3(0f, 0.0f, -1f));
                break;
            default:
                break;

        }


    }//*/

    private void UpdateCanvas(string monString)
    {

        TMP_Text txt = GameObject.Find("Canvas").GetComponent<TMP_Text>();
        if (txt != null)
            txt.text = monString;

        TMP_Text txtPerso = GameObject.Find("CanvasPerso").GetComponent<TMP_Text>();
        if (txtPerso != null)
            txtPerso.text = monString;
    }


    private void PrepareTheDeserializer() {

        Assembly[] myAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        //string[] lines = new string[myAssemblies.Length];
        //int compteur = 0;
        for (int i = 0; i < myAssemblies.Length; i++)
        {
            if (myAssemblies[i].GetName().Name == "Elements")
            {
                //lines[compteur] = myAssemblies[i].GetName().Name;
                //compteur++;
                Type[] types = myAssemblies[i].GetTypes();
                _jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Binder = new Assets.Binder(types), Formatting = Newtonsoft.Json.Formatting.Indented };
                found = true;
                break;
            }
        }
        //System.IO.File.WriteAllLines(@"WriteText.txt", lines);
        if (!found)
        {// may provoke crashes if selected
            _jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented };
            System.IO.File.WriteAllText(@"Readme.txt", "Wrong settings selected, Elements.dll has not been found");
        }
        _jsonSerializerSettings.Converters.Add(new UnitsNetJsonConverter());

    }
}
