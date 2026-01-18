using NATS.Client.JetStream.Models;

namespace Common.Nats.Nats;

public class JetStreamConfiguration
{
    public string StreamName { get; set; }
    public string SubjectWildcard { get; set; }
    public StreamConfigRetention Retention { get; set; }
    public int MaxAgeMin { get; set; }
    public long MaxMessages { get; set; }
    public long MaxBytes { get; set; }
    public StreamConfigDiscard  DiscardPolicy { get; set; }
    public StreamConfigStorage  Storage { get; set; }
}