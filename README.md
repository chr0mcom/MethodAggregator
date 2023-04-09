# MethodAggregator

<p align="center">
    <a href="https://www.nuget.org/packages/MethodAggregator" alt="Downloads">
        <img src="https://img.shields.io/nuget/dt/MethodAggregator?style=flat-square" />
    </a>
    <a href="https://github.com/chr0mcom/MethodAggregator/actions/workflows/dotnet.yml" alt="Pipeline status">
        <img src="https://img.shields.io/github/actions/workflow/status/chr0mcom/MethodAggregator/dotnet.yml?branch=main&style=flat-square" />
    </a>
    <a href="https://github.com/chr0mcom/MethodAggregator/blob/main/LICENSE" alt="License">
        <img alt="GitHub" src="https://img.shields.io/github/license/chr0mcom/MethodAggregator?style=flat-square">
    </a>
</p>

| ![Logo](https://raw.githubusercontent.com/chr0mcom/MethodAggregator/main/gfx/logo_200.png) | MethodAggregator is a C# library that allows you to manage, register, and execute methods dynamically. It provides functionality to find and execute the most suitable method based on the provided parameters and return type. |
| ---------- | ------- | 

## Usage

How to use the Execute method:
```csharp
using MethodAggregator;

public class Program
{
    public static void Main(string[] args)
    {
        IMethodAggregator aggregator = new MethodAggregator();
        
        // Register methods
        aggregator.Register((Func<int, int, int>)Add);
        aggregator.Register((Func<double, double, double>)Add);
        
        // Execute methods
        int intResult = aggregator.Execute<int>("Add", 1, 2);
        double doubleResult = aggregator.Execute<double>("Add", 1.0, 2.0);

        // Unregister methods
        aggregator.Unregister((Func<int, int, int>)Add);
        aggregator.Unregister((Func<double, double, double>)Add);
    }

    public static int Add(int a, int b)
    {
        return a + b;
    }

    public static double Add(double a, double b)
    {
        return a + b;
    }
}
```

How to use the SimpleExecute method:
```csharp
using MethodAggregator;

public class Program
{
    public static void Main(string[] args)
    {
        IMethodAggregator aggregator = new MethodAggregator();

        // Register methods
        aggregator.Register((int x, int y) => x + y, "Add");
        aggregator.Register((int x, double y) => x * y, "Multiply");

        // Examples for SimpleExecute
        int sumResult = aggregator.SimpleExecute<int>(10, 20); // Invokes the Add method
        int multiplyResult = aggregator.SimpleExecute<int>(10, 5.5); // Invokes the Multiply method

        Console.WriteLine($"Sum: {sumResult}");
        Console.WriteLine($"Multiplication: {multiplyResult}");
    }
}
```

## Methods

The MethodAggregator class provides the following methods for managing and executing registered methods:

- `Execute<T>(string name, params object[] parameters)`: Executes the method with the specified name and parameters, returning a value of type T
- `Execute(string name, params object[] parameters)`: Executes the method with the specified name and parameters, without returning a value
- `SimpleExecute<T>(params object[] parameters)`: Executes the method with the specified parameters, without specifying a name, and returns a value of type T
- `SimpleExecute(params object[] parameters)`: Executes the method with the specified parameters, without specifying a name or returning a value
- `TryExecute<T>(out T returnValue, string name, params object[] parameters)`: Attempts to execute the method with the specified name and parameters, returning a value of type T, and returns a boolean indicating whether the operation succeeded
- `TryExecute(string name, params object[] parameters)`: Attempts to execute the method with the specified name and parameters, without returning a value, and returns a boolean indicating whether the operation succeeded
- `IsRegistered(Delegate del)`: Returns a boolean indicating whether the specified delegate is registered in the aggregator
- `Register(Delegate del, string name = null)`: Registers the specified delegate with the given name, or the delegate's method name by default
- `Unregister(Delegate del)`: Unregisters the specified delegate from the aggregator
- `Dispose()`: Releases all resources used by the MethodAggregator instance

## Dependencies
JetBrains.Annotations

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
