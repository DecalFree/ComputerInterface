using ComputerInterface.Interfaces;

namespace ComputerInterface.Queues;

internal class CompetitiveQueue : IQueueInfo {
    public string DisplayName => "Competitive";
    public string QueueName => "COMPETITIVE";
    public string Description => "Competitive is for players who want to play the game, while trying as hard as they can.";
}