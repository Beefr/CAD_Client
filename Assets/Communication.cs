using NetworkCommsDotNet.DPSBase;
using NetworkCommsDotNet.Tools;
using ProtoBuf;
using System.IO;

    /// <summary>
    /// A wrapper class for the messages that we intend to send and receive.
    /// The [ProtoContract] attribute informs NetworkComms .Net that we intend to
    /// serialise (turn into bytes) this object. At the base level the
    /// serialisation is performed by protobuf.net.
    /// </summary>
    [ProtoContract]
    class Communication //: IExplicitlySerialize
    {
        /// <summary>
        /// The source identifier of this ChatMessage.
        /// We use this variable as the constructor for the ShortGuid.
        /// The [ProtoMember(1)] attribute informs the serialiser that when
        /// an object of type ChatMessage is serialised we want to include this variable
        /// </summary>
        [ProtoMember(1)]
        string _sourceIdentifier;

        /// <summary>
        /// The source identifier is accessible as a ShortGuid
        /// </summary>
        public ShortGuid SourceIdentifier { get { return new ShortGuid(_sourceIdentifier); } }


        /// <summary>
        /// The actual message.
        /// </summary>
        [ProtoMember(2)]
        public int Message { get; private set; }

        /// <summary>
        /// the secret key to verify whether it is a real message or not
        /// </summary>
        [ProtoMember(3)]
        public int SecretKey { get; private set; }



        /// <summary>
        /// We must include a private constructor to be used by the deserialisation step.
        /// </summary>
        private Communication() { } // protected ?

        /// <summary>
        /// Create a new ChatMessage
        /// </summary>
        /// <param name="sourceIdentifier">The source identifier</param>
        /// <param name="sourceName">The source name</param>
        /// <param name="message">The message to be sent</param>
        /// <param name="messageIndex">The index of this message</param>
        public Communication(ShortGuid sourceIdentifier, int message, int secretKey)
        {
            this._sourceIdentifier = sourceIdentifier;
            this.Message = message;
            this.SecretKey = secretKey;
        }

    /*
        /// <summary>
        /// Before serialising this object convert the image into binary data
        /// </summary>
        [ProtoBeforeSerialization]
        private void Serialize()
        {
                //We need to decide how to convert our image to its raw binary form here
               // using (MemoryStream inputStream = new MemoryStream())
               // {


                    //Store the binary image data as bytes[]
               //     _sourceIdentifier = inputStream.ToString();
               // }
        }

        /// <summary>
        /// When deserialising the object convert the binary data back into an image object
        /// </summary>
        [ProtoAfterDeserialization]
        private void Deserialize()
        {

        }*/
        /*
        public void Serialize(Stream outputStream)
        {
            return;

        }

        public void Deserialize(Stream inputStream)
        {
        return;
        }*/

    


    }

