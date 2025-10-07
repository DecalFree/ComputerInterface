using ComputerInterface.Interfaces;

namespace ComputerInterface.Queues;

internal class MinigamesQueue : IQueueInfo {
    public string DisplayName => "Minigames";
    public string QueueName => "MINIGAMES";
    public string Description => "Minigames is for people looking to play with their own set of rules.";
}