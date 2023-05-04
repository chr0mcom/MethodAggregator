# MethodAggregator

[![Downloads](https://img.shields.io/nuget/dt/MethodAggregator?style=flat-square)](https://www.nuget.org/packages/MethodAggregator)
[![Pipeline status](https://img.shields.io/github/actions/workflow/status/chr0mcom/MethodAggregator/dotnet.yml?branch=main&style=flat-square)](https://github.com/chr0mcom/MethodAggregator/actions/workflows/dotnet.yml)
[![GitHub](https://img.shields.io/github/license/chr0mcom/MethodAggregator?style=flat-square)](https://github.com/chr0mcom/MethodAggregator/blob/main/LICENSE)

| ![Logo](https://raw.githubusercontent.com/chr0mcom/MethodAggregator/main/gfx/logo_200.png) | MethodAggregator is a C# library that allows you to manage, register, and execute methods dynamically. It provides functionality to find and execute the most suitable method based on the provided parameters and return type. |
| ---------- | ------- | 

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

# The MethodAggregator: Simplifying Access to Methods in Complex Applications

The idea of the **MethodAggregator** emerged to simplify access to and invocation of methods from different components in complex applications. In modern software development, there is a constant effort to find efficient and elegant solutions that make the code more readable, maintainable, and flexible. The MethodAggregator contributes to handling a wide range of use cases and scenarios while improving the efficiency and structure of the development process.

## The Challenge

In complex applications and interactions between multiple components, organizing method calls and dependencies can be challenging. An *IoC container* can help, but if the instantiation or registration order is not optimal, undesirable side effects may occur. If instances are required in a constructor but have not yet been registered in the IoC container, this leads to an error during execution, which is sometimes difficult to recognize. Passing the IoC container into the respective class to obtain the required instance at the time of application is one way to circumvent this problem but feels like bad code design.

Modularity and reusability are crucial for avoiding redundancy and maintainability issues. Event handling or messaging systems enable communication between components but are often not ideal and can appear cumbersome when direct method calls are desired.

The **MethodAggregator**, therefore, provides a better alternative in certain cases by focusing on organizing and invoking methods rather than managing classes and interfaces. While an IoC container is still required for basic dependency management, the MethodAggregator can minimize dependencies between components in specific scenarios and simplify access to methods from different parts of the application without having to consider complex dependencies or special IoC configurations. The advantage of the MethodAggregator lies in its targeted organization and handling of methods in selected use cases, optimizing application structure and logic, and thus improving code quality.

## Method Registration

The MethodAggregator uses a central registration structure in which methods are stored and managed based on a registration name and its parameters. The registration mechanism provides the necessary information for the call.

One of the core functionalities of the MethodAggregator is to simplify the invocation of registered methods using reflection. You can call the desired methods by passing the appropriate parameters without worrying about instantiation or the order of component registration.

The MethodAggregator includes the `Register` method, which is used to register delegate instances. Optionally, a registration name for the method can be provided. If the overload without a name is used, the method name is simply used for registration. After registration, these methods can be called using one of the various `Execute` methods.

## Simple Execution

Let's start with the regular `Execute` method. First, it checks if a match is achieved through the registration parameter. If there are multiple registrations with the same name, the MethodAggregator looks for the method with the same or a corresponding return type that can be converted to the requested type.

The same applies to the method parameters passed to `Execute`. The `SimpleExecute` call works basically the same way, except it does not pre-filter by registration name but simply searches the complete list of methods for a suitable delegate based on return and input parameters. This greatly simplifies the use of the MethodAggregator, especially when the number of registered methods is low, and we want to access a method as simply as possible.

But how does the search for the right method work in detail? For this, the implementation in the `FindDelegate` method uses three helper methods called in succession.

- `FilterCountOfParameterMatches`
- `FilterParameterTypesAreAssignable`
- `FilterParameterBestTypeMatches`

The `FilterCountOfParameterMatches` method filters the methods in the first step, which have the same number of parameters as the provided parameters.

In the next step, the `FilterParameterTypesAreAssignable` method filters the methods whose parameter and return types are assignable or convertible to the specified types.

Finally, the `FilterParameterBestTypeMatches` method filters the methods whose parameter and return types have the highest degree of match with the specified types. Depending on the type, two categories are considered. For native types, the most logical or sensible type is chosen, while for classes and interfaces, inheritance and implementation levels must be traversed. The most obvious match receives the highest degree of match. Developing this priority logic for classes and interfaces was a major challenge. To find the "most obvious" match, a tree structure of inherited classes and implemented interfaces is created during the search. This structure is cached and only needs to be created on the first call of the `FindDelegate` method.

If there is still more than one method left after these three filters, the system can no longer distinguish and simply takes the first entry. If you have such a situation, you must use the registration name when calling to ensure the execution of the correct method.

In addition, thread safety in a multithreading context is ensured by always creating a separate lock object when registering a method. This is then used when calling the method, preventing simultaneous execution.

The following are various application examples for the `MethodAggregator` to better illustrate its functionality and versatility.

**Application Example 1: Registering Methods and Calling**

Suppose you have an application that should perform various mathematical operations.

*Listing 1: Methods for performing addition and multiplication*

```csharp
public int Add(int a, int b) 
{ 
    return a + b; 
} 

public int Multiply(int a, double b) 
{ 
    return (int)(a * b); 
} 
```

*Listing 2: Registering methods*

```csharp
IMethodAggregator aggregator = new MethodAggregator(); 
aggregator.Register(Add); 
aggregator.Register(Multiply); 
```

*Listing 3: Calling with registration names (by default the same as the method name)*

```csharp
int sum = aggregator.Execute<int>("Add", 3, 5); 
int product = aggregator.Execute<int>("Multiply", 3, 5.0); 
```

*Listing 4: Simplified calling (when input and return parameter types are unique)*

```csharp
int sum = aggregator.SimpleExecute<int>(3, 5); 
int product = aggregator.SimpleExecute<int>(3, 5.0); 
```

**Application Example 2: Using Conversion Logic**

As mentioned, the `MethodAggregator` has complex search algorithms to find the method with the highest probability, even when the given parameter types or the requested return type do not explicitly match. The following example (see Listing 5 & 6) should illustrate this again.

*Listing 5: Exemplary class and interface schema*

```csharp
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
```

*Listing 6: Sample code for testing the behavior*

```csharp
void Action1(MyClass1 obj) => Console.WriteLine("Method1 with MyClass1 was called.");
void Action2(IMyInterface1 obj) => Console.WriteLine("Method2 with IMyInterface1 was called.");
void Action3(MyClass2 obj) => Console.WriteLine("Method3 with MyClass2 was called.");
void Action4(IMyInterface2 obj) => Console.WriteLine("Method4 with IMyInterface2 was called.");

aggregator.Register(Action1);
aggregator.Register(Action2);
aggregator.Register(Action3);
aggregator.Register(Action4);

aggregator.SimpleExecute(new MyClass1());
aggregator.SimpleExecute(new MyClass2());
```

**Table 1**: For MyClass1, the following order is obtained

| Priority | Type          |
| -------- | ------------- |
| 1        | MyClass1      |
| 2        | MyClass2      |
| 3        | IMyInterface1 |
| 4        | IMyInterface2 |

**Table 2**: and for MyClass2

| Priority | Type          |
| -------- | ------------- |
| 1        | MyClass2      |
| 2        | IMyInterface2 |

**Example 3**: Use in a plugin architecture

Suppose you have an application with a plugin architecture. Plugins can provide their own methods that can be called by the main application. In this case, the MethodAggregator can be used to manage and call methods from different plugins.

*Listing 7: Interface for the plugins*

```csharp
public interface IPlugin
{
    string Name { get; }
    void RegisterMethods(MethodAggregator aggregator);
}
```

*Listing 8: The two plugins implementing their useful functions*

```csharp
public class PluginA : IPlugin
{
    public string Name => "PluginA";
    public void RegisterMethods(MethodAggregator aggregator)
    {
        aggregator.Register(ToUpper);
    }

    private string ToUpper(string input)
    {
        return input.ToUpper();
    }
}

public class PluginB : IPlugin
{
    public string Name => "PluginB";
    public void RegisterMethods(MethodAggregator aggregator)
    {
        aggregator.Register(Reverse);
    }

    private string Reverse(string input)
    {
        return new string(input.Reverse().ToArray());
    }
}
```

*Listing 9: Registering the methods of the plugins in the main application and calling them*

```csharp
List<IPlugin> plugins = new List<IPlugin> { new PluginA(), new PluginB() };
IMethodAggregator aggregator = new MethodAggregator();
foreach (IPlugin plugin in plugins)
{
    plugin.RegisterMethods(aggregator);
}
string input = "Hello, world!";
string upper = aggregator.Execute<string>("ToUpper", input);
string reversed = aggregator.Execute<string>("Reverse", input);
Console.WriteLine($"Input: {input}");
Console.WriteLine($"ToUpper: {upper}");
Console.WriteLine($"Reverse: {reversed}");
//      Input: Hello, world!
//      ToUpper: HELLO, WORLD!
//      Reverse: !dlrow ,olleH
```

These examples demonstrate how the MethodAggregator can be used to register, manage, and call methods in various scenarios. By using the MethodAggregator, you can make your applications more modular and maintainable while benefiting from a simple and flexible way of calling functions.

## Dependencies
JetBrains.Annotations

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
