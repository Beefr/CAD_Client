using ProtoBuf;
using NetworkCommsDotNet.Tools;


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
    /// The source identifier of this Communication.
    /// We use this variable as the constructor for the ShortGuid.
    /// The [ProtoMember(1)] attribute informs the serialiser that when
    /// an object of type Communication is serialised we want to include this variable
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
    /// We must include a private constructor to be used by the deserialisation step.
    /// </summary>
    protected Communication() { }

    /// <summary>
    /// Create a new Communication
    /// </summary>
    /// <param name="sourceIdentifier">The source identifier</param>
    /// <param name="message">The message to be sent</param>
    public Communication(ShortGuid sourceIdentifier, int message)
    {
        this._sourceIdentifier = sourceIdentifier;
        this.Message = message;
    }

       
}

