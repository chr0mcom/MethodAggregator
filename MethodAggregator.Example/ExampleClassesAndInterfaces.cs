#pragma warning disable CS8618
namespace MethodAggregator.Example;

class MyClass1 : MyClass2, IMyInterface1
{
    public string Bar { get; set; }
}

class MyClass2 : IMyInterface2
{
    public string Foo { get; set; }
}

interface IMyInterface1 : IMyInterface2
{
    string Bar { get; set; }
}

interface IMyInterface2
{
    string Foo { get; set; }
}
