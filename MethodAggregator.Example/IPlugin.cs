namespace MethodAggregator.Example
{
    internal interface IPlugin
    {
        string Name { get; }
        void RegisterMethods(IMethodAggregator aggregator);
    }
}
