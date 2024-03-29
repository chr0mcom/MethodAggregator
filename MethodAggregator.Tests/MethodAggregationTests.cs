﻿#region Usings

using System;
using JetBrains.Annotations;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace MethodAggregator.Tests;

public class MethodAggregationUnitTests : MethodAggregatorTestBase
{
	public MethodAggregationUnitTests([NotNull] ITestOutputHelper output) : base(RegisteringBehaviour.MethodName)
	{
		_output = output;
		MethodAggregator = new MethodAggregator();
	}

	[NotNull] private readonly ITestOutputHelper _output;

	[Fact]
	public void Execute_CalledWithNonRegisteredName_ExceptionIsThrown()
	{
		// ReSharper disable once AssignNullToNotNullAttribute
		Should.Throw<InvalidOperationException>(() => MethodAggregator.Execute<int>("NotRegistered")).Message.ShouldBe("No method found for given parameters.");
	}

	[Fact]
	public void Execute_ExecuteRegisteredMethod_MethodIsExecuted()
	{
		#region Arrange

		int GetTwo() => 2;
		MethodAggregator.Register(GetTwo, nameof(GetTwo));

		#endregion

		#region Act

		int returned = MethodAggregator.Execute<int>(nameof(GetTwo));

		#endregion Act

		#region Assert

		returned.ShouldBe(2);

		#endregion Assert
	}

    [Fact]
    public void Execute_ExecuteRegisterClassMethod_MethodIsExecuted()
    {
        #region Arrange

        string testString = nameof(testString);
        MethodAggregator.RegisterClass<ClassForRegisterClassTest>(RegisteringBehaviour.MethodName);

        #endregion

        #region Act

        string returned = MethodAggregator.Execute<string>(nameof(ClassForRegisterClassTest.TestMethod2), testString);

        #endregion Act

        #region Assert

        returned.ShouldBe(testString);

        #endregion Assert
    }

    [Fact]
    public void Execute_ExecuteRegisterClassStaticMethod_StaticMethodIsExecuted()
    {
        #region Arrange

        int testInt = 2;
        MethodAggregator.RegisterClass<ClassForRegisterClassTest>(RegisteringBehaviour.MethodName);

        #endregion

        #region Act

        int returned = MethodAggregator.Execute<int>(nameof(ClassForRegisterClassTest.StaticTestMethod1), testInt);

        #endregion Act

        #region Assert

        returned.ShouldBe(testInt);

        #endregion Assert
    }

	[Fact]
	public void Execute_ExecuteRegisteredMethodWithParameters_MethodIsExecuted()
	{
		#region Arrange

		int GetTwo(int i) => i;
		MethodAggregator.Register(GetTwo, nameof(GetTwo));

		#endregion

		#region Act

		int returned = MethodAggregator.Execute<int>(nameof(GetTwo), 2);

		#endregion Act

		#region Assert

		returned.ShouldBe(2);

		#endregion Assert
	}

	[Fact]
	public void Execute_ExecuteRegisteredMethodWithParametersAndConvertType_MethodIsExecuted()
	{
		#region Arrange

		int GetTwo(int i) => i;
		MethodAggregator.Register(GetTwo, nameof(GetTwo));

		#endregion

		#region Act

		double returned = MethodAggregator.Execute<double>(nameof(GetTwo), 2);

		#endregion Act

		#region Assert

		returned.ShouldBe(2);

		#endregion Assert
	}

	[Fact]
	public void IsRegistered_CalledWithNullDelegate_ExceptionIsThrown()
	{
		// ReSharper disable once AssignNullToNotNullAttribute
		Should.Throw<ArgumentNullException>(() => MethodAggregator.IsRegistered((Delegate) null)).Message.ShouldBe("Value cannot be null. (Parameter 'del')");
	}

	[Fact]
	public void IsRegistered_CalledWithNullName_ExceptionIsThrown()
	{
		// ReSharper disable once AssignNullToNotNullAttribute
		Should.Throw<ArgumentNullException>(() => MethodAggregator.IsRegistered((string) null)).Message.ShouldBe("Value cannot be null. (Parameter 'name')");
	}

	[Fact]
	public void Register_CalledWithNullDelegate_ExceptionIsThrown()
	{
		// ReSharper disable once AssignNullToNotNullAttribute
		Should.Throw<ArgumentNullException>(() => MethodAggregator.Register(null)).Message.ShouldBe("Value cannot be null. (Parameter 'del')");
	}

    [Fact]
    public void Register_RegisterClass_MethodsInClassAreRegistered()
    {
        #region Arrange

        

        #endregion

        #region Act

		MethodAggregator.RegisterClass<ClassForRegisterClassTest>(RegisteringBehaviour.MethodName);
        
        #endregion Act

        #region Assert

        MethodAggregator.IsRegistered(nameof(ClassForRegisterClassTest.TestMethod1)).ShouldBeTrue();
        MethodAggregator.IsRegistered(nameof(ClassForRegisterClassTest.TestMethod2)).ShouldBeTrue();
        MethodAggregator.IsRegistered(nameof(ClassForRegisterClassTest.TestMethod3)).ShouldBeTrue();
        MethodAggregator.IsRegistered(nameof(ClassForRegisterClassTest.StaticTestMethod1)).ShouldBeTrue();

        #endregion Assert
    }

	[Fact]
	public void Register_RegisterMethod_MethodIsRegistered()
	{
		#region Arrange

		void WriteMethod() => Console.WriteLine(2);

		#endregion

		#region Act

		MethodAggregator.Register(WriteMethod);

		#endregion Act

		#region Assert

		MethodAggregator.IsRegistered(WriteMethod).ShouldBeTrue();

		#endregion Assert
	}

	[Fact]
	public void Register_RegisterMethodTwice_ThrowsException()
	{
		#region Arrange

		void WriteMethod() => Console.WriteLine(2);

		#endregion

		#region Act

		MethodAggregator.Register(WriteMethod);

		#endregion Act

		#region Assert

		Should.Throw<ArgumentException>(() => MethodAggregator.Register(WriteMethod));

		#endregion Assert
	}

	[Fact]
	public void Register_RegisterMethodWithDifferentName_MethodIsRegistered()
	{
		#region Arrange

		void WriteMethod() => Console.WriteLine(2);

		#endregion

		#region Act

		MethodAggregator.Register(WriteMethod, "WriteMethod");

		#endregion Act

		#region Assert

		MethodAggregator.IsRegistered(WriteMethod).ShouldBeTrue();
		MethodAggregator.IsRegistered("WriteMethod").ShouldBeTrue();

		#endregion Assert
	}

	[Fact]
	public void Register_RegisterMethodWithDifferentNameAndMethodTwice_ThrowsException()
	{
		#region Arrange

		void WriteMethod() => Console.WriteLine(2);

		#endregion

		#region Act

		MethodAggregator.Register(WriteMethod, "WriteMethod");

		#endregion Act

		#region Assert

		Should.Throw<ArgumentException>(() => MethodAggregator.Register(WriteMethod, "WriteMethod2"));

		#endregion Assert
	}

	[Fact]
	public void Register_RegisterMethodWithDifferentNameTwice_ThrowsException()
	{
		#region Arrange

		void WriteMethod() => Console.WriteLine(2);

		#endregion

		#region Act

		MethodAggregator.Register(WriteMethod, "WriteMethod");

		#endregion Act

		#region Assert

		Should.Throw<ArgumentException>(() => MethodAggregator.Register(WriteMethod, "WriteMethod"));

		#endregion Assert
	}

	[Fact]
	public void SimpleExecute_ExecuteOnlyRegisteredMethod_MethodIsExecuted()
	{
		#region Arrange

		bool wasExecuted = false;
		Exception e = null;
		void TestMethod() => wasExecuted = true;

		#endregion Arrange

		MethodAggregator.Register(TestMethod);

		#region Act

		try
		{
			MethodAggregator.SimpleExecute();
		} catch (Exception ex)
		{
			e = ex;
		}

		#endregion Act

		#region Assert

		e.ShouldBeNull();
		wasExecuted.ShouldBeTrue();

		#endregion Assert
	}

	[Fact]
	public void SimpleExecute_ExecuteOnlyRegisteredMethodWithParameters_MethodIsExecuted()
	{
		#region Arrange

		bool wasExecuted = false;

		int TestMethod(int i)
		{
			wasExecuted = true;
			return i;
		}

		#endregion Arrange

		MethodAggregator.Register(TestMethod);

		#region Act

		int returned = MethodAggregator.SimpleExecute<int>(2);

		#endregion Act

		#region Assert

		returned.ShouldBe(2);
		wasExecuted.ShouldBeTrue();

		#endregion Assert
	}

	[Fact]
	public void TryExecute_TryExecuteOnNonRegisteredMethod_FalseIsReturned()
	{
		#region Act

		bool wasExecuted = MethodAggregator.TryExecute("NotRegisteredMethod");

		#endregion Act

		#region Assert

		wasExecuted.ShouldBeFalse();

		#endregion Assert
	}

	[Fact]
	public void TryExecute_TryExecuteOnRegisteredMethod_MethodIsCalled()
	{
		#region Arrange

		int GetTwo() => 2;
		MethodAggregator.Register(GetTwo, nameof(GetTwo));

		#endregion Arrange

		#region Act

		MethodAggregator.TryExecute(out int ret, nameof(GetTwo));

		#endregion Act

		#region Assert

		ret.ShouldBe(2);

		#endregion Assert
	}

	[Fact]
	public void TryExecute_TryExecuteOnRegisteredVoidMethod_MethodIsCalled()
	{
		#region Arrange

		bool wasExecuted = false;
		void MethodToExecute() => wasExecuted = true;
		MethodAggregator.Register(MethodToExecute, nameof(MethodToExecute));

		#endregion Arrange

		#region Act

		MethodAggregator.TryExecute(nameof(MethodToExecute));

		#endregion Act

		#region Assert

		wasExecuted.ShouldBeTrue();

		#endregion Assert
	}

	[Fact]
	public void Unregister_CalledWithNonRegisteredName_ExceptionIsThrown()
	{
		// ReSharper disable once AssignNullToNotNullAttribute
		Should.Throw<InvalidOperationException>(() => MethodAggregator.Unregister("NotRegistered")).Message.ShouldBe("No method found with given name: 'NotRegistered'.");
	}

	[Fact]
	public void Unregister_CalledWithNullDelegate_ExceptionIsThrown()
	{
		// ReSharper disable once AssignNullToNotNullAttribute
		Should.Throw<ArgumentNullException>(() => MethodAggregator.Unregister((Delegate) null)).Message.ShouldBe("Value cannot be null. (Parameter 'del')");
	}

	[Fact]
	public void Unregister_CalledWithNullName_ExceptionIsThrown()
	{
		// ReSharper disable once AssignNullToNotNullAttribute
		Should.Throw<ArgumentNullException>(() => MethodAggregator.Unregister((string) null)).Message.ShouldBe("Value cannot be null. (Parameter 'name')");
	}

	[Fact]
	public void Unregister_RegisterAndUnregisterMethod_MethodIsUnregistered()
	{
		#region Arrange

		void MethodToExecute() => Console.WriteLine(2);
		MethodAggregator.Register(MethodToExecute, nameof(MethodToExecute));

		#endregion Arrange

		#region Act

		MethodAggregator.Unregister(MethodToExecute);

		#endregion Act

		#region Assert

		MethodAggregator.IsRegistered(MethodToExecute).ShouldBeFalse();
		MethodAggregator.IsRegistered(nameof(MethodToExecute)).ShouldBeFalse();

		#endregion Assert
	}

	[Fact]
	public void Unregister_RegisterAndUnregisterMethodByName_MethodIsUnregistered()
	{
		#region Arrange

		void MethodToExecute() => Console.WriteLine(2);
		MethodAggregator.Register(MethodToExecute, nameof(MethodToExecute));

		#endregion Arrange

		#region Act

		MethodAggregator.Unregister(nameof(MethodToExecute));

		#endregion Act

		#region Assert

		MethodAggregator.IsRegistered(MethodToExecute).ShouldBeFalse();
		MethodAggregator.IsRegistered(nameof(MethodToExecute)).ShouldBeFalse();

		#endregion Assert
	}
}