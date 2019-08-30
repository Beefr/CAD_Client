using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections.TCP;
using System;
using UnityEngine;

namespace Assets
{
    public delegate void MyConnectionHandler(object source, ConnectionEventArgs e);
    
    /// <summary>
    /// tries to connect to the client
    /// </summary>
    public sealed class Connector 
    {
        public event MyConnectionHandler NoConnection; // is used in TryConnecting()

        private TCPConnection serverConnection = null;
        private ConnectionInfo connectionInfo;
        private SendReceiveOptions customSendReceiveOptions;

        private static readonly Lazy<Connector> _lazy = new Lazy<Connector>(() => new Connector());
        public static Connector Instance { get { return _lazy.Value; } }
        private Connector()
        {
            // Now lets test the event contained in the above class.
            this.NoConnection += new MyConnectionHandler(Connect);
        }

        /// <summary>
        /// getter for the tcpConnection
        /// </summary>
        /// <returns></returns>
        public TCPConnection GetServerConnection()
        {
            return serverConnection;
        }


        /// <summary>
        /// set the parameters of the communication before TryConnecting()
        /// </summary>
        /// <param name="connectionInfo">Networkcommsdotnet.connectionInfo containing ipaddress and port</param>
        /// <param name="customSendReceiveOptions">networkcommsdotnet.sendreceiveoptions containing the protobuf serializer</param>
        public void SetParameters(ConnectionInfo connectionInfo, SendReceiveOptions customSendReceiveOptions)
        {
            this.connectionInfo = connectionInfo;
            this.customSendReceiveOptions = customSendReceiveOptions;
        }

        /// <summary>
        /// tries to connect and, if it does, update the parameters but also sends a message to the server to tell him that this is our first connection
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        void Connect(object source, ConnectionEventArgs e)
        {
            if (e.ConnectionAlive())
            {
                serverConnection = e.GetConnection();
                Debug.Log("We connected successfully!");
            }
        }

        /// <summary>
        /// tries to connect to the server
        /// </summary>
        public void TryConnecting()
        {

#if UNITY_EDITOR
#else

            if (ConnectionAlive() == false)
                NoConnection(this, new ConnectionEventArgs(connectionInfo, customSendReceiveOptions));
            // else it is connected
            
#endif
        }

        /// <summary>
        /// returns true or false if resp. the connection is alive or not
        /// </summary>
        /// <returns></returns>
        public bool ConnectionAlive()
        {
            if (this.serverConnection == null)
                return false;
            else if (this.serverConnection.ConnectionAlive() == false)
                return false;
            return true;
        }
    }
    
    /// <summary>
    /// it tries to connect and retrieves the connection status
    /// </summary>
    public class ConnectionEventArgs : EventArgs
    {
        private TCPConnection serverConnection;
        public ConnectionEventArgs(ConnectionInfo connectionInfo, SendReceiveOptions customSendReceiveOptions)
        {
            try
            {
                Debug.Log("Tries to connect...");
                this.serverConnection = TCPConnection.GetConnection(connectionInfo, customSendReceiveOptions);
            } catch (Exception e)
            {
                Debug.Log("Could not connect to the server...");
                //We can decide what to do here if the synchronous send and receive timed out after the specified 1000ms
            }
        }
        public TCPConnection GetConnection() { return serverConnection; }
        public bool ConnectionAlive()
        {
            if (this.serverConnection == null)
                return false;
            else if (this.serverConnection.ConnectionAlive() == false)
                return false;
            return true;
        }
    }
}