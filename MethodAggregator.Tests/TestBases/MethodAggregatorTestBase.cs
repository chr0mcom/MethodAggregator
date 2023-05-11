#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace MethodAggregator.Tests;

public class MethodAggregatorTestBase : IDisposable
{
	protected class Class1
	{
		public static void Method1() { }
	}
	
	[NotNull] protected IMethodAggregator MethodAggregator;

	/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
	protected MethodAggregatorTestBase(RegisteringBehaviour registeringBehaviour) { MethodAggregator = new MethodAggregator(registeringBehaviour); }

	/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
	public void Dispose()
	{
		MethodAggregator.Dispose();
		GC.SuppressFinalize(this);
	}
}