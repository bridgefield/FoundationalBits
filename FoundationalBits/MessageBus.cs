using bridgefield.FoundationalBits.Messaging;

namespace bridgefield.FoundationalBits
{
    public static class MessageBus
    {
        public static IMessageBus Create() => new AgentBasedMessageBus();
    }
}