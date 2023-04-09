namespace MethodAggregator.Example;

internal class PluginA : IPlugin
{
    public string Name => "PluginA";

    public void RegisterMethods(IMethodAggregator aggregator)
    {
        aggregator.Register(ToUpper);
    }

    private string ToUpper(string input) => input.ToUpper();
}
