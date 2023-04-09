#pragma warning disable CS8618
namespace MethodAggregator.Example
{
    internal class MyClass1 : MyClass2, IMyInterface1
    {
        public string Bar { get; set; }
    }
    
    internal class MyClass2 : IMyInterface2
    {
        public string Foo { get; set; }
    }

    internal interface IMyInterface1 : IMyInterface2
    {
        public string Bar { get; set; }
    }

    internal interface IMyInterface2
    {
        public string Foo { get; set; }
    }
}
