using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.DPSBase;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpencascadePart.Elements;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using UnitsNet;
using UnitsNet.Serialization.JsonNet;
using UnitsNet.Units;
using UnityEngine;

namespace Assets
{
    /// <summary>
    /// client for the communication
    /// it creates a connector that connects the client to the server
    /// it creates a constructor that constructs what is added to the construction queue
    /// it sends messages
    /// it receives messages
    /// it also calls the manager when needed to update objects and UI
    /// </summary>
    public class Client : MonoBehaviour
    {
        

        // a construction queue
        private Constructor constructor;

        // the communication
        public int port = 10000;
        public string ipAddress = "127.0.0.1";
        private Connector connector;
        

        // for deserializing 
        private SendReceiveOptions customSendReceiveOptions = new SendReceiveOptions<ProtobufSerializer>();
        private JsonSerializerSettings _jsonSerializerSettingsOCC;

        /// <summary>
        /// called at start to initialize things
        /// </summary>
        void Start()
        {
            // singleton of constructor, we simply need to add new elements to it
            constructor = Constructor.Instance;

            // you absolutely need it to deserialize your objects correctly, make sure you got Elements.dll
            PrepareSerializerSettings();

            // connect to the server
            connector = Connector.Instance; // singleton of Connector
            connector.SetParameters(new ConnectionInfo(ipAddress, port), customSendReceiveOptions); 
            GetConnectedToServer();



#if UNITY_EDITOR
            ForTestingPurpose();
#endif

            // give the already existing objects some properties and functions
            InitializeAllObjects();
        }
        

        /// <summary>
        /// for testing purpose: it creates an element at the beginning
        /// </summary>
        private void ForTestingPurpose()
        {
            // for testing purpose
            //__________ server's part_________________
            //Elements.Caps c = new Elements.Caps(1, "Caps", "Element", 1, 1, "Caps", new Length(0.2, LengthUnit.Meter), new Length(0.5, LengthUnit.Meter), new Length(0.4999, LengthUnit.Meter), new Length(0.5, LengthUnit.Meter), new Length(1, LengthUnit.Meter), new Length(0, LengthUnit.Meter), new Pressure(0, PressureUnit.Bar), "Caps", false);
            //Elements.PipeRectangular c = new Elements.PipeRectangular(1, "PipeRectangular", "Element", 1, 1, "PipeRectangular", "Shape", new Length(0.1, LengthUnit.Meter), new Length(1, LengthUnit.Meter), new Length(0.5, LengthUnit.Meter), new Length(0.5, LengthUnit.Meter));
            Elements.ElbowCylindrical c = new Elements.ElbowCylindrical(1, "ElbowCylindrical", "Element", 1, 1, new Length(0.1, LengthUnit.Meter), new Length(1, LengthUnit.Meter), new Angle(90, AngleUnit.Degree), new Length(0, LengthUnit.Meter), false, new Length(0, LengthUnit.Meter), new Length(5, LengthUnit.Meter), new Length(0, LengthUnit.Meter), new Angle(0, AngleUnit.Degree), new Angle(0, AngleUnit.Degree));
            //Elements.Cone c = new Elements.Cone(1, "Tube", "Element", 1, 1, new Length(0.1, LengthUnit.Meter), new Length(2, LengthUnit.Meter), new Length(1, LengthUnit.Meter), new Length(2, LengthUnit.Meter), new Angle(0, AngleUnit.Degree), new Length(0, LengthUnit.Meter), new Length(0, LengthUnit.Meter), new Length(0, LengthUnit.Meter), new Length(0, LengthUnit.Meter));
            //Elements.Tube c = new Elements.Tube(1, "Tube", "Element", 1, 1, "tube", new Length(1, LengthUnit.Meter), new Length(0.1, LengthUnit.Meter), new Length(2, LengthUnit.Meter));
            JsonSerializerSettings _jsonSerializerSettingsOCCServer = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented };
            _jsonSerializerSettingsOCCServer.Converters.Add(new UnitsNetJsonConverter());
            string json = JsonConvert.SerializeObject(c, typeof(Elements.BaseElement), _jsonSerializerSettingsOCCServer);
            CommunicateElement messageObject = new CommunicateElement(NetworkComms.NetworkIdentifier, json, c.Designation);
            //__________ client's part_________________
            constructor.AddNewElement(JsonConvert.DeserializeObject<BasicElement>(messageObject.Message, _jsonSerializerSettingsOCC));//*/
            constructor.IncrementPosition(5);
        }

        /// <summary>
        /// connects you if u aren't already and builds if there is anything in the construction queue
        /// </summary>
        void Update()
        {
            // verifying if there are elements in queue and build them in that case
            constructor.TryBuilding();
            
            // do things only if there is no connection
            GetConnectedToServer();
            // checks each tick, so it is lagging if it is not connected, 
            // you can fix this by adding a timer between each checking if you really wish to build without connection
            
        }

        /// <summary>
        /// if there is no connection then tries to connect, and, if it works, adds the handlers
        /// </summary>
        private void GetConnectedToServer()
        {
            //Create a connectionInfo object that specifies the target server
            //This line assumes the server is on the local machine - 127.0.0.1
            //This line also assumes that the server is listening on port 10000 (check this is the case).
            //connectionInfo = new ConnectionInfo(ipAddress, port);
            
            if (connector.ConnectionAlive()==false)
            {
                connector.TryConnecting();

                if (connector.ConnectionAlive())
                {
                    File.WriteAllText(@"ConnectionStatus.txt", "alive");

                    // calls ClientDisconnected if it disconnects
                    connector.GetServerConnection().AppendShutdownHandler(ClientDisconnected);

                    // if u receive a message of type communication with the communication tag then it calls handleincomingCommunication(), and it does that with the customsendreceiveoptions that you need
                    connector.GetServerConnection().AppendIncomingPacketHandler<Communication>("Communication", HandleIncomingCommunication, customSendReceiveOptions);

                    // get the objects sent by the server and add it to the construction queue
                    connector.GetServerConnection().AppendIncomingPacketHandler<CommunicateElement>("Element", HandleIncomingObject, customSendReceiveOptions);

                    // get an update on an object
                    connector.GetServerConnection().AppendIncomingPacketHandler<CommunicateUpdate>("Update", HandleIncomingObjectUpdate, customSendReceiveOptions);
                }
                else { File.WriteAllText(@"ConnectionStatus.txt", "offline"); }
            }
            

        }


        /// <summary>
        /// give the already existing objects some properties and functions
        /// </summary>
        private void InitializeAllObjects()
        {
            // set the id of all objects that were introduced previously in the scene
            object[] obj = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
            foreach (object o in obj)
            {
                GameObject g = (GameObject)o;
                constructor.AddExistingElement(g);
                // dont create objects by yourself, now you must give them to the constructor which will give ID and so on
            }
        }



        /// <summary>
        /// do whatever you want if it disconnected
        /// </summary>
        /// <param name="connection"></param>
        void ClientDisconnected(Connection connection)
        {
            Manager manager = GameObject.Find("Manager").GetComponent<Manager>();
            manager.SetText("You got disconnected");
        }



        /// <summary>
        /// initializes _jsonSerializerSettingsOCC that will allow us to deserialize our json(Elements) into a BasicElement (OpencascadePart.dll)
        /// </summary>
        private void PrepareSerializerSettings()
        {

            _jsonSerializerSettingsOCC = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Binder = new AdvancedBinder(),
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            _jsonSerializerSettingsOCC.Converters.Add(new UnitsNetJsonConverter());

        }

        /// <summary>
        /// for example purpose
        /// </summary>
        /// <param name="header">The PacketHeader corresponding with the received object</param>
        /// <param name="connection">The Connection from which this object was received</param>
        /// <param name="incomingMessage">The incoming communication we are after</param>
        private void HandleIncomingCommunication(PacketHeader header, Connection connection, Communication incomingMessage)
        {
            if (NetworkComms.NetworkIdentifier != connection.ConnectionInfo.NetworkIdentifier)
            {
                Communication message = new Communication(NetworkComms.NetworkIdentifier, constructor.Position());
                connector.GetServerConnection().SendObject("Communication", message); // sends back a communication message
                                                                       // this is more for example purpose than anything else
            }
        }


        /// <summary>
        /// add an object to the construction queue
        /// </summary>
        /// <param name="header">The PacketHeader corresponding with the received object</param>
        /// <param name="connection">The Connection from which this object was received</param>
        /// <param name="incomingMessage">The incoming CommunicateElement we are after</param>
        private void HandleIncomingObject(PacketHeader header, Connection connection, CommunicateElement incomingMessage)
        {
            if (NetworkComms.NetworkIdentifier != connection.ConnectionInfo.NetworkIdentifier) // checks if it is not us messaging ourself
            {
                try
                {

                    // adds an object to the construction queue, this is the tricky part
                    // it converts your BaseElement object into a cylinder or whatever you want
                    //ConstructionQueue.Add(JsonConvert.DeserializeObject<BasicElement>(incomingMessage.Message, _jsonSerializerSettingsOCC));
                    constructor.AddNewElement(JsonConvert.DeserializeObject<BasicElement>(incomingMessage.Message, _jsonSerializerSettingsOCC));
                    
                }
                catch (Exception e) { System.IO.File.WriteAllText(@"Debug.txt", e.ToString()); }
            }
            //https://stackoverflow.com/questions/4535840/deserialize-json-object-into-dynamic-object-using-json-net
        }

        /// <summary>
        /// get a message that asks us to update our object and UI
        /// </summary>
        /// <param name="header">The PacketHeader corresponding with the received object</param>
        /// <param name="connection">The Connection from which this object was received</param>
        /// <param name="incomingMessage">The incoming CommunicateElement we are after</param>
        private void HandleIncomingObjectUpdate(PacketHeader header, Connection connection, CommunicateUpdate incomingMessage)
        {
            // verifies it is an object 
            if (incomingMessage.ID != -1)
            {
                if (NetworkComms.NetworkIdentifier != connection.ConnectionInfo.NetworkIdentifier)
                {
                    foreach(MyMesh mesh in constructor.GetConstructedElements())
                    {
                        if (mesh.obj.GetComponent("AdditionnalProperties") != null)
                        {
                            if (mesh.obj.GetComponent<AdditionnalProperties>().ID == incomingMessage.ID)
                            {
                                string content = incomingMessage.Message;

                                Manager manager = GameObject.Find("Manager").GetComponent<Manager>();
                                manager.SetObj(mesh.obj);
                                manager.TryUpdating(content);
                                manager.TryDesigning(content);

                                break; // get out of the loop

                            }
                        }
                    }
                }
            }
        }


        

        /// <summary>
        /// sends the caracteristics of your object to the server
        /// </summary>
        /// <param name="obj"></param>
        public void SendObjectCaracteristics(GameObject obj)
        {
            try
            {
                if (connector.GetServerConnection() == null)
                {
                }
                else if (connector.GetServerConnection().ConnectionAlive()) // is it connected ? 
                {

                    // serialize the object
                    int ID = obj.GetComponent<AdditionnalProperties>().ID;
                    MyGameObject myObj = new MyGameObject(obj);
                    DataContractSerializer DCS = new DataContractSerializer(myObj.GetType());
                    MemoryStream streamer = new MemoryStream();
                    DCS.WriteObject(streamer, myObj);
                    streamer.Seek(0, SeekOrigin.Begin);
                    string content = XElement.Parse(Encoding.ASCII.GetString(streamer.GetBuffer()).Replace("\0", "")).ToString();

                    string monString = "Name: " + StringHelper.GetBetween(content, "<a:name>", "</a:name>") + "\n\n";
                    monString = monString + "<ID \n    " + ID.ToString() + "\nID> \n\n";
                    monString = monString + "<Position " + StringHelper.GetBetween(content, "<position xmlns:a=\"http://schemas.datacontract.org/2004/07/UnityEngine\">", "</position>") + "Position> \n\n";
                    monString = monString + "<Scale " + StringHelper.GetBetween(content, "<scale xmlns:a=\"http://schemas.datacontract.org/2004/07/UnityEngine\">", "</scale>") + "Scale> \n\n";
                    
                    CommunicateUpdate messageUpdate = new CommunicateUpdate(NetworkComms.NetworkIdentifier, monString, ID);

                    connector.GetServerConnection().SendObject("Update", messageUpdate);
                }


            }
            catch (Exception e) { System.IO.File.WriteAllText(@"Content.txt", e.ToString()); }
        }

        /// <summary>
        /// if you press a key u can tell the server you did
        /// </summary>
        /// <param name="key"></param>
        public void SendKeyDownIndication(string key)
        {
            try
            {
                connector.GetServerConnection().SendObject(key, 1);
            }
            catch (Exception e) { Debug.Log("No server connected"); }
        }


        

        ~Client() { System.IO.File.WriteAllText(@"ConnectionStatus.txt", "offline"); }
    }
}
