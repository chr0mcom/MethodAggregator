#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace MethodAggregator.Tests;

public class MethodAggregatorTestBase : IDisposable
{
	[NotNull] protected MethodAggregator MethodAggregator;

	/// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
	protected MethodAggregatorTestBase() { MethodAggregator = new MethodAggregator(); }

	/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
	public void Dispose()
	{
		MethodAggregator.Dispose();
		GC.SuppressFinalize(this);
	}
}