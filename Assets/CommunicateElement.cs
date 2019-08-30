using NetworkCommsDotNet.Tools;
using ProtoBuf;

/// <summary>
/// A wrapper class for the messages that we intend to send and receive.
/// The [ProtoContract] attribute informs NetworkComms .Net that we intend to
/// serialise (turn into bytes) this object. At the base level the
/// serialisation is performed by protobuf.net.
/// </summary>
[ProtoContract]
class CommunicateElement
{
    /// <summary>
    /// The source identifier of this CommunicateElement.
    /// We use this variable as the constructor for the ShortGuid.
    /// The [ProtoMember(1)] attribute informs the serialiser that when
    /// an object of type CommunicateElement is serialised we want to include this variable
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
    /// type of object
    /// </summary>
    [ProtoMember(3, DataFormat = DataFormat.FixedSize, IsRequired = true)]
    public string Type { get; private set; }


    /// <summary>
    /// We must include a private constructor to be used by the deserialisation step.
    /// </summary>
    protected CommunicateElement() { }


    /// <summary>
    /// Create a new CommunicateElement
    /// </summary>
    /// <param name="sourceIdentifier">The source identifier</param>
    /// <param name="message">The message to be sent</param>
    /// <param name="type">The type of the message as a string</param>
    public CommunicateElement(ShortGuid sourceIdentifier, string message, string type)
    {
        this._sourceIdentifier = sourceIdentifier;
        this.Message =  message;
        this.Type = type;
    }


}

