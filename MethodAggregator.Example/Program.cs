using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#pragma warning disable CS8321

namespace MethodAggregator.Example
{
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute"), SuppressMessage("ReSharper", "PossibleNullReferenceException")] 
    internal class Program
    {
        static void Main(string[] args)
        {
            IMethodAggregator aggregator = new MethodAggregator();

            Example1(aggregator);
            Example2(aggregator);
            Example3(aggregator);

            Console.ReadLine();
        }

        //Registering methods and execute
        private static void Example1(IMethodAggregator aggregator)
        {
            int Add(int a, int b) => a + b;
            int Multiply(int a, double b) => (int)(a * b);

            aggregator.Register(Add, nameof(Add));
            aggregator.Register(Multiply, nameof(Multiply));

            int sumExecute = aggregator.Execute<int>("Add", 3, 5);
            int productExecute = aggregator.Execute<int>("Multiply", 3, 5.0);

            Console.WriteLine($"sumExecute 3 + 5: Result {sumExecute}");
            Console.WriteLine($"productExecute 3 x 5.0: Result {productExecute}");

            int sumSimpleExecute = aggregator.SimpleExecute<int>( 3, 5);
            int productSimpleExecute = aggregator.SimpleExecute<int>( 3, 5.0);

            Console.WriteLine($"sumSimpleExecute 3 + 5: Result {sumSimpleExecute}");
            Console.WriteLine($"productSimpleExecute 3 x 5.0: Result {productSimpleExecute}");
        }

        //Call by best match search with Actions
        private static void Example2(IMethodAggregator aggregator)
        {
            void Action1(MyClass1 obj) => Console.WriteLine("Method1 with MyClass1 was called.");
            void Action2(IMyInterface1 obj) => Console.WriteLine("Method2 with IMyInterface1 was called.");
            void Action3(MyClass2 obj) => Console.WriteLine("Method3 with MyClass2 was called.");
            void Action4(IMyInterface2 obj) => Console.WriteLine("Method4 with IMyInterface2 was called.");
            
            //aggregator.Register(Action1);
            aggregator.Register(Action2);
            aggregator.Register(Action3);
            aggregator.Register(Action4);
            
            MyClass1 instance1 = new ();
            MyClass2 instance2 = new ();
            
            aggregator.SimpleExecute(instance1); 
            aggregator.SimpleExecute(instance2); 
        }

        //Use in a plug-in architecture
        private static void Example3(IMethodAggregator aggregator)
        {
            List<IPlugin> plugins = new List<IPlugin> { new PluginA(), new PluginB() };

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
        }

        //TODO: Not working yet!
        //Call by best match search with return parameter
        private static void Example4(IMethodAggregator aggregator)
        {
            MyClass1 Func1() => new () {Foo = "Func1 was called."};
            IMyInterface1 Func2() => new MyClass1 {Foo = "Func2 was called."};
            MyClass2 Func3() => new () {Foo = "Func3 was called."};
            IMyInterface2 Func4() => new MyClass2 {Foo = "Func4 was called."};
            
            aggregator.Register(Func1);
            //aggregator.Register(Func2);
            aggregator.Register(Func3);
            aggregator.Register(Func4);
            
            IMyInterface1 instance1 = aggregator.SimpleExecute<IMyInterface1>();
            IMyInterface2 instance2 = aggregator.SimpleExecute<IMyInterface2>();

            Console.WriteLine(instance1.Foo);
            Console.WriteLine(instance2.Foo);
        }
    }
}