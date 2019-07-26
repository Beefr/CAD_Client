using Elements;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet.DPSBase;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using UnitsNet;
using UnityEngine;
using UnitsNet.Serialization.JsonNet;

namespace Assets
{
    class NCServer : MonoBehaviour
    {
        private int position = -1;
        private bool serverEnabled = false;
        //private String IPServer="127.0.0.1";
        private int PortServer = 10000;
        private ConnectionInfo serverConnectionInfo = null;
        private Communication message;
        private CommunicateElement messageObject;
        private SendReceiveOptions customSendReceiveOptions;
        private List<IPEndPoint> connectedClients = new List<IPEndPoint>();
        private bool connected = false;
        private int compteur = 0;

        public void Start() {
            customSendReceiveOptions = new SendReceiveOptions<ProtobufSerializer>();
            serverConnectionInfo = new ConnectionInfo(new IPEndPoint(IPAddress.Any, PortServer));
            //connection = TCPConnection.GetConnection(serverConnectionInfo);

            //Start listening for incoming TCP connections
            if (!serverEnabled)
                EnableServer_Toggle();


            //Configure NetworkComms .Net to handle and incoming packet of type 'ChatMessage'
            //e.g. If we receive a packet of type 'ChatMessage' execute the method 'HandleIncomingChatMessage'
            NetworkComms.AppendGlobalIncomingPacketHandler<Communication>("Communication", HandleIncomingChatMessage, customSendReceiveOptions);
            NetworkComms.AppendGlobalConnectionCloseHandler(ClientDisconnected);

            NetworkComms.AppendGlobalConnectionEstablishHandler(ClientConnected);


            /*NetworkComms.AppendGlobalIncomingPacketHandler<CommunicateObject>("Caracteristics", (header, connection, message) =>
            {
                Console.WriteLine("Object name: " + message.Message.name);
            });//*/

            NetworkComms.AppendGlobalIncomingPacketHandler<string>("Caracteristics", (header, connection, message) =>
            {
                Console.WriteLine("Object name: " + message);
            });

            NetworkComms.AppendGlobalIncomingPacketHandler<Communication>("InitialClientConnect", (header, connection, message) =>
            {
                //Debug.Log("a client has been registered");
                if (!connected)
                {
                    lock (connectedClients)
                    {
                        //If a client sends a InitialClientConnect packet we add them to the connectedClients list
                        connectedClients.Add((IPEndPoint)connection.ConnectionInfo.RemoteEndPoint);
                        position = message.Message;

                        ICollection<IPEndPoint> withoutDuplicates = new HashSet<IPEndPoint>(connectedClients);
                        connectedClients = new List<IPEndPoint>(withoutDuplicates);
                        connected = true;
                    }
                }
            });

        }

        public void Update() {
            if (compteur % 100 == 0 && compteur >400)
            {
                Debug.Log("Update");
                SendMyObject();
            }
            compteur++;
        }

        public NCServer()
        {
            /*
            customSendReceiveOptions = new SendReceiveOptions<ProtobufSerializer>();
            serverConnectionInfo = new ConnectionInfo(new IPEndPoint(IPAddress.Any, PortServer));
            //connection = TCPConnection.GetConnection(serverConnectionInfo);

            //Start listening for incoming TCP connections
            if (!serverEnabled)
                EnableServer_Toggle();


            //Configure NetworkComms .Net to handle and incoming packet of type 'ChatMessage'
            //e.g. If we receive a packet of type 'ChatMessage' execute the method 'HandleIncomingChatMessage'
            NetworkComms.AppendGlobalIncomingPacketHandler<Communication>("Communication", HandleIncomingChatMessage, customSendReceiveOptions);
            NetworkComms.AppendGlobalConnectionCloseHandler(ClientDisconnected);

            NetworkComms.AppendGlobalConnectionEstablishHandler(ClientConnected);


            //NetworkComms.AppendGlobalIncomingPacketHandler<CommunicateObject>("Caracteristics", (header, connection, message) =>
            //{
            //    Console.WriteLine("Object name: " + message.Message.name);
            //});

            NetworkComms.AppendGlobalIncomingPacketHandler<string>("Caracteristics", (header, connection, message) =>
            {
                Console.WriteLine("Object name: " + message);
            });

            NetworkComms.AppendGlobalIncomingPacketHandler<Communication>("InitialClientConnect", (header, connection, message) =>
            {
                if (!connected)
                {
                    lock (connectedClients)
                    {
                        //If a client sends a InitialClientConnect packet we add them to the connectedClients list
                        connectedClients.Add((IPEndPoint)connection.ConnectionInfo.RemoteEndPoint);
                        position = message.Message;

                        ICollection<IPEndPoint> withoutDuplicates = new HashSet<IPEndPoint>(connectedClients);
                        connectedClients = new List<IPEndPoint>(withoutDuplicates);
                        connected = true;
                    }
                }
            });
            */




        }


        public void ClientConnected(Connection connection)
        {
            Console.WriteLine(" ");
            Console.WriteLine("Connection established: " + connection.ConnectionInfo.LocalEndPoint);

        }


        public void ClientDisconnected(Connection connection)
        {
            Console.WriteLine(" ");
            Console.WriteLine("Connection is over: " + connection.ConnectionInfo.LocalEndPoint);

        }




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
                position = incomingMessage.Message;
                Console.WriteLine("Message received from: " + connection.ConnectionInfo.LocalEndPoint + ", position updated: " + incomingMessage.Message + " " + incomingMessage.SecretKey);
                //Console.WriteLine("Message received from: " + incomingMessage.SourceIdentifier + ", position updated: " + incomingMessage.Message + " " + incomingMessage.SecretKey);


            }
        }

        public void Right(int val)
        {
            foreach (System.Net.IPEndPoint localEndPoint in connectedClients)
            {
                //Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);
                try
                {
                    for (int i = 0; i < val; i++)
                    {
                        //NetworkComms.SendObject("Communication", localEndPoint.Address.ToString(), localEndPoint.Port, message);
                        TCPConnection.GetConnection(new ConnectionInfo(localEndPoint), customSendReceiveOptions).SendObject("Deplacement", "right");
                        //Communication customObject2 = NetworkComms.SendReceiveObject<Communication, Communication>("Communication", localEndPoint.Address.ToString(), localEndPoint.Port, "Communication", 1000, message);
                        //position = customObject2.Message;
                    }
                }
                catch (CommunicationException) { Console.WriteLine("CommunicationException"); }
                catch (ConnectionShutdownException) { Console.WriteLine("ConnectionShutdownException"); }
                catch (Exception) { Console.WriteLine("Autre exception"); }
            }
        }



        public void Bot(int val)
        {
            foreach (System.Net.IPEndPoint localEndPoint in connectedClients)
            {
                //Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);
                try
                {
                    for (int i = 0; i < val; i++)
                    {
                        //NetworkComms.SendObject("Communication", localEndPoint.Address.ToString(), localEndPoint.Port, message);
                        TCPConnection.GetConnection(new ConnectionInfo(localEndPoint), customSendReceiveOptions).SendObject("Deplacement", "bot");
                        //Communication customObject2 = NetworkComms.SendReceiveObject<Communication, Communication>("Communication", localEndPoint.Address.ToString(), localEndPoint.Port, "Communication", 1000, message);
                        //position = customObject2.Message;
                    }
                }
                catch (CommunicationException) { Console.WriteLine("CommunicationException"); }
                catch (ConnectionShutdownException) { Console.WriteLine("ConnectionShutdownException"); }
                catch (Exception) { Console.WriteLine("Autre exception"); }
            }
        }

        public void Left(int val)
        {
            foreach (System.Net.IPEndPoint localEndPoint in connectedClients)
            {
                // Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);
                try
                {
                    for (int i = 0; i < val; i++)
                    {
                        //NetworkComms.SendObject("Communication", localEndPoint.Address.ToString(), localEndPoint.Port, message);
                        TCPConnection.GetConnection(new ConnectionInfo(localEndPoint), customSendReceiveOptions).SendObject("Deplacement", "left");
                        //Communication customObject2 = NetworkComms.SendReceiveObject<Communication, Communication>("Communication", localEndPoint.Address.ToString(), localEndPoint.Port, "Communication", 1000, message);
                        //position = customObject2.Message;
                    }
                }
                catch (CommunicationException) { Console.WriteLine("CommunicationException"); }
                catch (ConnectionShutdownException) { Console.WriteLine("ConnectionShutdownException"); }
                catch (Exception) { Console.WriteLine("Autre exception"); }
            }
        }

        public void Top(int val)
        {
            foreach (System.Net.IPEndPoint localEndPoint in connectedClients)
            {
                //Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);
                try
                {
                    for (int i = 0; i < val; i++)
                    {
                        //NetworkComms.SendObject("Communication", localEndPoint.Address.ToString(), localEndPoint.Port, message);
                        TCPConnection.GetConnection(new ConnectionInfo(localEndPoint), customSendReceiveOptions).SendObject("Deplacement", "top");
                        //Communication customObject2 = NetworkComms.SendReceiveObject<Communication, Communication>("Communication", localEndPoint.Address.ToString(), localEndPoint.Port, "Communication", 1000, message);
                        //position = customObject2.Message;
                    }
                }
                catch (CommunicationException) { Console.WriteLine("CommunicationException"); }
                catch (ConnectionShutdownException) { Console.WriteLine("ConnectionShutdownException"); }
                catch (Exception) { Console.WriteLine("Autre exception"); }
            }
        }

        public void SendMessage()
        {

            message = new Communication(NetworkComms.NetworkIdentifier, position, 1234);

            //Print out the IPs and ports we are now listening on
            Console.WriteLine();
            Console.WriteLine("Server messaging these TCP connections:");

            foreach (System.Net.IPEndPoint localEndPoint in connectedClients)
            {
                Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);
                try
                {
                    //NetworkComms.SendObject("Communication", localEndPoint.Address.ToString(), localEndPoint.Port, message);
                    TCPConnection.GetConnection(new ConnectionInfo(localEndPoint), customSendReceiveOptions).SendObject("Communication", message);
                    //Communication customObject2 = NetworkComms.SendReceiveObject<Communication, Communication>("Communication", localEndPoint.Address.ToString(), localEndPoint.Port, "Communication", 1000, message);
                    //position = customObject2.Message;

                }
                catch (CommunicationException) { Console.WriteLine("CommunicationException"); }
                catch (ConnectionShutdownException) { Console.WriteLine("ConnectionShutdownException"); }
                catch (Exception) { Console.WriteLine("Autre exception"); }
            }

            Console.WriteLine("Server stops messaging");
            Console.WriteLine();


        }

        /*// Deserialize a JSON stream to a User object.  
        public static User ReadToObject(string json)  
        {  
            User deserializedUser = new User();  
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));  
            DataContractJsonSerializer ser = new DataContractJsonSerializer(deserializedUser.GetType());  
            deserializedUser = ser.ReadObject(ms) as User;  
            ms.Close();  
            return deserializedUser;  
        }  //*/

        //JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);

        public void SendMyObject()
        {/*
            Creator crea = new Elements.Creator();
            Cylinder c = crea.CreateCylinder(1,1,1);

            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(c.GetType());
            ser.WriteObject(stream, c);
            stream.Position = 0;
            byte[] json = stream.ToArray();
            stream.Close();//*/

            //Creator crea = new Elements.Creator();
            //Cylinder c = crea.CreateCylinder(1, 1, 1);
            Length c = new Length(1, UnitsNet.Units.LengthUnit.Meter);
            /*
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(c.GetType());
            ser.WriteObject(stream, c);
            stream.Position = 0;
            byte[] json = stream.ToArray();
            stream.Close();//*/
            JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };
            _jsonSerializerSettings.Converters.Add(new UnitsNetJsonConverter());
            string json = JsonConvert.SerializeObject(c, _jsonSerializerSettings).Replace("\r\n", "\n");
            //string json = JsonConvert.SerializeObject(c);
            //messageObject = new CommunicateElement(NetworkComms.NetworkIdentifier, Encoding.UTF8.GetString(json, 0, json.Length), 1234, "Cylinder");
            messageObject = new CommunicateElement(NetworkComms.NetworkIdentifier, json, 1234, "Cylinder");
            //messageObject = new CommunicateElement(NetworkComms.NetworkIdentifier, Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length), 1234, "Cylinder");
            
            //Print out the IPs and ports we are now listening on
            Console.WriteLine();
            Console.WriteLine("Server messaging these TCP connections:");

            //Debug.Log("Number of client connected: "+connectedClients.Count);
            foreach (System.Net.IPEndPoint localEndPoint in connectedClients)
            {

                //Debug.Log(localEndPoint.Address);
                Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);
                try
                {
                    //Debug.Log("sending message...");
                    TCPConnection.GetConnection(new ConnectionInfo(localEndPoint), customSendReceiveOptions).SendObject("Element", messageObject);

                    //Debug.Log("...message sent");
                }
                catch (CommunicationException) { Console.WriteLine("CommunicationException"); }
                catch (ConnectionShutdownException) { Console.WriteLine("ConnectionShutdownException"); }
                catch (Exception) { Console.WriteLine("Autre exception"); }
            }

            Console.WriteLine("Server stops messaging");
            Console.WriteLine();


        }




        /// <summary>
        /// Toggle whether the local application is acting as a server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnableServer_Toggle()
        {
            //Enable or disable the local server mode depending on the checkbox IsChecked value
            if (!serverEnabled)
                ToggleServerMode(true);
            else
                ToggleServerMode(false);
            serverEnabled = !serverEnabled;

        }

        /// Wrap the functionality required to enable/disable the local application server mode
        /// </summary>
        /// <param name="enableServer"></param>
        private void ToggleServerMode(bool enableServer)
        {
            if (enableServer)
            {
                //Start listening for new incoming TCP connections
                //Parameters ensure we listen across all adaptors using a random port
                TCPConnection.StartListening(ConnectionType.TCP, new IPEndPoint(IPAddress.Any, PortServer));
            }

            else
            {
                ShutDown();
            }
        }



        public void ShutDown()
        {
            //We have used NetworkComms so we should ensure that we correctly call shutdown
            NetworkComms.Shutdown();
        }

        public int GetPosition() { return position; }
    }
}
