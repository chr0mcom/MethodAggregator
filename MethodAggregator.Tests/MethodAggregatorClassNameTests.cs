using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MethodAggregator.Tests;

public class MethodAggregatorClassNameTests : MethodAggregatorTestBase
{
	private readonly ITestOutputHelper _output;
	
	public MethodAggregatorClassNameTests(ITestOutputHelper output) : base(RegisteringBehaviour.ClassAndMethodName) { _output = output; }


	[Fact]
	public void Register_RegisterWithNamingConvention_MethodIsRegisteredWithClassName()
	{
		#region Arrange

		Class1 class1 = new ();

		#endregion Arrange

		#region Act
		
		MethodAggregator.Register(Class1.Method1);

		#endregion Act

		#region Assert
		MethodAggregator.IsRegistered("Class1.Method1").ShouldBeTrue();
		MethodAggregator.IsRegistered("Method1").ShouldBeFalse();

		#endregion Assert
	}
}