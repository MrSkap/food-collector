namespace Common.Nats.Nats;

public class NatsConfiguration
{
    public static string SectionName = "NatsConfiguration";
    public required string ConnectionString { get; set; }
    public required string ServiceName { get; set; }
    public string? Credentials { get; set; }
    public List<JetStreamConfiguration>? JetStreamConfigurations { get; set; }
}