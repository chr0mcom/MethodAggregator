# MethodAggregator
## Simplifying Access to Methods in Complex Applications

[![Downloads](https://img.shields.io/nuget/dt/MethodAggregator?style=flat-square)](https://www.nuget.org/packages/MethodAggregator)
[![Pipeline status](https://img.shields.io/github/actions/workflow/status/chr0mcom/MethodAggregator/dotnet.yml?branch=main&style=flat-square)](https://github.com/chr0mcom/MethodAggregator/actions/workflows/dotnet.yml)
[![GitHub](https://img.shields.io/github/license/chr0mcom/MethodAggregator?style=flat-square)](https://github.com/chr0mcom/MethodAggregator/blob/main/LICENSE)

| ![Logo](https://raw.githubusercontent.com/chr0mcom/MethodAggregator/main/gfx/logo_200.png) | MethodAggregator is a C# library that allows you to manage, register, and execute methods dynamically. It provides functionality to find and execute the most suitable method based on the provided parameters and return type. |
| ---------- | ------- | 

The idea of the **MethodAggregator** emerged to simplify access to and invocation of methods from different components in complex applications. In modern software development, there is a constant effort to find efficient and elegant solutions that make the code more readable, maintainable, and flexible. The MethodAggregator contributes to handling a wide range of use cases and scenarios while improving the efficiency and structure of the development process.

In complex applications and interactions between multiple components, organizing method calls and dependencies can be challenging. Modularity and reusability are crucial for avoiding redundancy and maintainability issues. Event handling or messaging systems enable communication between components but are often not ideal and can appear cumbersome when direct method calls are desired.

The **MethodAggregator**, provides an alternative to an IoC container in certain cases by focusing on organizing and invoking methods rather than managing classes and interfaces. The advantage of the MethodAggregator lies in the targeted organization and handling of methods in selected use cases, the optimization of the application structure and logic, and thus the improvement of code quality.

## Registration

The MethodAggregator operates as a centralized structure that manages methods using a registration name and parameters, simplifying method invocation via reflection, hence eliminating concerns about instantiation or order of component registration. Its core functionality includes the `Register` method, allowing delegate instances registration optionally with a provided name, or by default using the method name when the overload without a name is applied. Post-registration, these methods can be invoked through different `Execute` methods.

## Execution
The `Execute` method in MethodAggregator first checks for a match via the registration parameter and, when multiple registrations have the same name, it identifies a method with an equivalent or convertible return type. This applies to method parameters passed to `Execute` as well. The `SimpleExecute` call, however, does not pre-filter by registration name; instead, it scans the entire method list for an appropriate delegate based on return and input parameters, streamlining the use of the MethodAggregator, particularly when the number of registered methods is low, and straightforward access to a method is desired.

But how does the search for the right method work in detail? For this, the implementation in the `FindDelegate` method uses three helper methods called in succession.

- `FilterCountOfParameterMatches`
- `FilterParameterTypesAreAssignable`
- `FilterParameterBestTypeMatches`

The method `FilterCountOfParameterMatches` initially filters methods that share the same number of parameters as the provided ones. This is followed by `FilterParameterTypesAreAssignable`, which filters based on parameter and return types being assignable or convertible to specific types. The final filtration is performed by `FilterParameterBestTypeMatches`, selecting methods with the highest degree of match with the designated types, considering both native types and classes or interfaces. For the latter, a tree structure of inheritance and implementation levels is created, cached, and used for finding the "most obvious" match. If multiple methods remain post filtration, the first one is chosen; if specificity is required, method registration names should be used. Concurrent execution is prevented through the use of separate lock objects created during method registration, ensuring thread safety in a multithreading context.

The following are various application examples for the `MethodAggregator` to better illustrate its functionality and versatility.

**Application Example 1: Registering Methods and Calling**
Suppose you have an application that should perform various mathematical operations.

*Snippet 1: Methods for performing addition and multiplication*

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

*Snippet 2: Registering methods*

```csharp
IMethodAggregator aggregator = new MethodAggregator(RegisteringBehaviour.MethodName); 
aggregator.Register(Add); 
aggregator.Register(Multiply); 
```

*Snippet 3: Calling with registration names (by default the same as the method name)*

```csharp
int sum = aggregator.Execute<int>("Add", 3, 5); 
int product = aggregator.Execute<int>("Multiply", 3, 5.0); 
```

*Snippet 4: Simplified calling (when input and return parameter types are unique)*

```csharp
int sum = aggregator.SimpleExecute<int>(3, 5); 
int product = aggregator.SimpleExecute<int>(3, 5.0); 
```

**Application Example 2: Using Conversion Logic**
As mentioned, the `MethodAggregator` has complex search algorithms to find the method with the highest probability, even when the given parameter types or the requested return type do not explicitly match. The following example (see Snippet 5 & 6) should illustrate this again.

*Snippet 5: Exemplary class and interface schema*

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

*Snippet 6: Sample code for testing the behavior*

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

*Snippet 7: Interface for the plugins*

```csharp
public interface IPlugin
{
    string Name { get; }
    void RegisterMethods(MethodAggregator aggregator);
}
```

*Snippet 8: The two plugins implementing their useful functions*

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

*Snippet 9: Registering the methods of the plugins in the main application and calling them*

```csharp
List<IPlugin> plugins = new List<IPlugin> { new PluginA(), new PluginB() };
IMethodAggregator aggregator = new MethodAggregator(RegisteringBehaviour.ClassAndMethodName);
foreach (IPlugin plugin in plugins)
{
    plugin.RegisterMethods(aggregator);
}
string input = "Hello, world!";
string upper = aggregator.Execute<string>("PluginA.ToUpper", input);
string reversed = aggregator.Execute<string>("PluginB.Reverse", input);
Console.WriteLine($"Input: {input}");
Console.WriteLine($"ToUpper: {upper}");
Console.WriteLine($"Reverse: {reversed}");
//      Input: Hello, world!
//      ToUpper: HELLO, WORLD!
//      Reverse: !dlrow ,olleH
```

These examples demonstrate how the MethodAggregator can be used to register, manage, and call methods in various scenarios. By using the MethodAggregator, you can make your applications more modular and maintainable while benefiting from a simple and flexible way of calling functions.

## `RegisterClass` Methods

The `IMethodAggregator` interface has been extended with four new `RegisterClass` methods. These methods are designed to provide more versatility when registering methods from a specified class, whether they are instance or static methods.

### Features:

1. **Dynamic Method Registration**: Dynamically register methods from a specified class without explicitly defining each one.
2. **Automatic Instance Management**: If an instance of the class isn't provided during registration, the system will automatically create and manage it. These instances are also tracked and properly disposed of, if they implement the `IDisposable` interface, upon unregistering the related methods.
3. **Flexible Registration Behavior**: Choose your preferred registration behavior with the `RegisteringBehaviour` parameter.

### Method Overviews:

1. **Generic Type with Optional Instance**:
    ```csharp
    void RegisterClass<T>(T instance = null) where T : class;
    ```
    Use this method when you wish to register methods from a class, specifying either an instance or the static methods.

2. **Generic Type with Registration Behavior (MethodName or ClassAndMethodName)**:
    ```csharp
    void RegisterClass<T>(RegisteringBehaviour registeringBehaviour) where T : class;
    ```
    Register static methods from a class with the flexibility to define a specific registering behavior.

3. **Generic Type with Instance and Registration Behavior (MethodName or ClassAndMethodName)**:
    ```csharp
    void RegisterClass<T>(T instance, RegisteringBehaviour registeringBehaviour) where T : class;
    ```
    Register methods from a specific instance of a class with your desired registering behavior.

4. **Using Class Type**:
    ```csharp
    void RegisterClass(Type classType);
    ```
    Allows for the registration of static methods without specifying generic type parameters.

5. **Using Class Type with Registration Behavior (MethodName or ClassAndMethodName)**:
    ```csharp
    void RegisterClass(Type classType, RegisteringBehaviour registeringBehaviour);
    ```
    Register static methods from a class using a specific registering behavior without specifying generic type parameters.

### Usage:

1. **Registering instance methods**:
    ```csharp
    var aggregator = new MethodAggregator();
    MyClass obj = new MyClass();
    aggregator.RegisterClass(obj);
    ```

2. **Registering static methods with a behavior**:
    ```csharp
    var aggregator = new MethodAggregator();
    aggregator.RegisterClass<MyClass>(RegisteringBehaviour.ClassAndMethodName);
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
- `Register(Delegate del, string name = null)`: Registers the specified delegate with the in the constructor specified RegisteringBehaviour or the custom given name
- `Register(Delegate del, RegisteringBehavoiur registeringBehaviour, string name = null)`: Registers the specified delegate with the given name adhering to the specified registering behaviour
- `Unregister(Delegate del)`: Unregisters the specified delegate from the aggregator
- `Dispose()`: Releases all resources used by the MethodAggregator instance

## Dependencies
JetBrains.Annotations

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

