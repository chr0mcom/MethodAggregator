using System;

namespace MethodAggregator.Tests;

public class TypeConversionTestBase : IDisposable
{
	
	protected class Device : Serial, IObject { }

	protected class Serial : IObject4, IDevice { }

	protected interface IDevice : IObject2 { }

	protected interface IObject { }

	protected interface IObject2 : IObject3 { }

	protected interface IObject3 { }

	protected interface IObject4 : IObject5 { }

	protected interface IObject5 { }
	
	public TypeConversionTestBase() { }

	/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
	}
}