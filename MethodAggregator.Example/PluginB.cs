namespace MethodAggregator.Example;

internal class PluginB : IPlugin
    {
        public string Name => "PluginB";

        public void RegisterMethods(IMethodAggregator aggregator)
        {
            aggregator.Register(Reverse);
        }

        private string Reverse(string input) => new (input.Reverse().ToArray());
    }