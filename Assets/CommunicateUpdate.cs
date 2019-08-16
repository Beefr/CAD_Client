using NetworkCommsDotNet.Tools;
using ProtoBuf;


/// <summary>
/// type of message to update an element
/// </summary>
[ProtoContract]
class CommunicateUpdate
{
    /// <summary>
    /// The source identifier of this ChatMessage.
    /// We use this variable as the constructor for the ShortGuid.
    /// The [ProtoMember(1)] attribute informs the serialiser that when
    /// an object of type ChatMessage is serialised we want to include this variable
    /// </summary>
    [ProtoMember(1, DataFormat = DataFormat.FixedSize, IsRequired = true)]
    string _sourceIdentifier;

    /// <summary>
    /// The source identifier is accessible as a ShortGuid
    /// </summary>
    public ShortGuid SourceIdentifier { get { return new ShortGuid(_sourceIdentifier); } }


    /// <summary>
    /// The actual message.
    /// </summary>
    [ProtoMember(2, DataFormat = DataFormat.FixedSize, IsRequired = true)]
    public string Message { get; private set; }

   

    /// <summary>
    /// the secret key to verify whether it is a real message or not
    /// </summary>
    [ProtoMember(3, DataFormat = DataFormat.FixedSize, IsRequired = true)]
    public int ID { get; private set; }



    /// <summary>
    /// We must include a private constructor to be used by the deserialisation step.
    /// </summary>
    protected CommunicateUpdate() { }


    /// <summary>
    /// Create a new ChatMessage
    /// </summary>
    /// <param name="sourceIdentifier">The source identifier</param>
    /// <param name="sourceName">The source name</param>
    /// <param name="message">The message to be sent</param>
    /// <param name="messageIndex">The index of this message</param>
    public CommunicateUpdate(ShortGuid sourceIdentifier, string message, int ID)
    {
        this._sourceIdentifier = sourceIdentifier;
        this.Message = message;
        this.ID = ID;
    }

}

