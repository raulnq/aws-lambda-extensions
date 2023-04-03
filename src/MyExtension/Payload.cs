namespace MyExtension
{
    public enum EventType
    {
        INVOKE,
        SHUTDOWN,
    }

    public class Payload
    {
        public EventType EventType { get; set; }
        public string? RequestId { get; set; }
        public string? InvokedFunctionArn { get; set; }
        public decimal DeadlineMs { get; set; }
        public string? ShutdownReason { get; set; }
    }
}
