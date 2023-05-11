namespace MethodAggregator;

/// <summary>
///     Configures the registering behaviour of the <see cref="MethodAggregator" />.
/// </summary>
public enum RegisteringBehaviour
{
	/// <summary>
	///    Only the method name is used as key.
	/// </summary>
	/// <example>
	///	<code>
	/// public class Class1
	/// {
	///		public void Method1() { }
	/// }
	/// methodAggregator.Execute("Method1");
	/// </code>
	/// </example>
	MethodName,
	/// <summary>
	///		 The class name and the method name are used as key.
	/// </summary>
	/// <example>
	///	<code>
	/// public class Class1
	/// {
	///		public void Method1() { }
	/// }
	/// methodAggregator.Execute("Class1.Method1");
	/// </code>
	/// </example>
	ClassAndMethodName
}