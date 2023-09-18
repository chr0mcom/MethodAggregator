#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace MethodAggregator;

/// <summary>
///     Provides a way to register methods and execute them by name and/or parameters.
/// </summary>
public interface IMethodAggregator : IDisposable
{
	/// <summary>
	///     Executes a method by name.
	///     The return type is defined by the type of <typeparamref name="T" />.
	/// </summary>
	/// <exception cref="InvalidOperationException">when the method is not found</exception>
	/// <param name="name">The name of the method to be called</param>
	/// <param name="parameters">The parameters passed to the method to be called</param>
	/// <typeparam name="T"></typeparam>
	/// <returns>The returned value of the called function</returns>
	T Execute<T>(string name, params object[] parameters);

	/// <summary>
	///     Executes a method with no return type by name.
	/// </summary>
	/// <exception cref="InvalidOperationException">when the method is not found</exception>
	/// <param name="name">The name of the method to be called</param>
	/// <param name="parameters">The parameters passed to the method to be called</param>
	void Execute(string name, params object[] parameters);

	/// <summary>
	///     Checks if a method is registered.
	/// </summary>
	/// <param name="del">The Method to be checked as a <see cref="Action" /> or <see cref="Func{TResult}" /></param>
	/// <returns>If the method is registered or not</returns>
	bool IsRegistered([NotNull] Delegate del);

	/// <summary>
	///     Checks if a method is registered by name.
	/// </summary>
	/// <seealso cref="IsRegistered(System.Delegate)" />
	/// <param name="name">The name of the Method as a string</param>
	/// <returns>
	///     If the method is registered or not
	/// </returns>
	bool IsRegistered([NotNull] string name);

	/// <summary>
	///     Registers a method.
	///		Adheres to the normal convention of registering methods with the on construction specified Registering method.
	/// </summary>
	/// <example>
	///     <code>
	/// 	int Add(int a, int b) => a + b;
	/// 	IMethodAggregator aggregator = new MethodAggregator();
	/// 	methodAggregator.Register(Add);
	/// </code>
	/// </example>
	/// 
	/// <param name="del">The <see cref="Delegate" /> to be registered</param>
	/// <param name="name">the name to later call the method by</param>
	void Register([NotNull] Delegate del, string name = null);

    /// <summary>
    /// Registers the methods of a specified class for later execution. If an instance is provided,
    /// it will register the instance methods. If omitted, a new instance of type T is created and its methods are registered.
    /// </summary>
    /// <typeparam name="T">The class type containing the methods to be registered.</typeparam>
    /// <param name="instance">The instance of the class. If omitted, a new instance is created and its methods are registered.</param>
    /// <remarks>
    /// Use this method to dynamically register methods from a class. Once registered,
    /// these methods can be executed on-demand using their names and parameters via
    /// the <see cref="Execute{T}(string, object[])" /> method. Instances that are created are managed and 
    /// will be disposed of, if they implement IDisposable, when their methods are unregistered.
    /// </remarks>
    void RegisterClass<T>(T instance = null) where T : class;
    
    /// <summary>
    /// Registers all static methods of the specified type. 
    /// The methods within the specified type can then be executed based on their names and parameters.
    /// </summary>
    /// <typeparam name="T">The type of the class whose static methods will be registered.</typeparam>
    /// <param name="registeringBehaviour">The behavior with which the methods should be registered.</param>
    /// <remarks>
    /// Use this method when you wish to register static methods of a class and 
    /// might want to specify a different registering behaviour than the default one set during construction.
    /// </remarks>
    void RegisterClass<T>(RegisteringBehaviour registeringBehaviour) where T : class;

    /// <summary>
    /// Registers all methods of the provided instance of the specified type. If the instance is not provided,
    /// a new instance of type T is created and its methods are registered.
    /// </summary>
    /// <typeparam name="T">The type of the class whose methods will be registered.</typeparam>
    /// <param name="instance">The instance of the class. If omitted, a new instance is created.</param>
    /// <param name="registeringBehaviour">The behavior with which the methods should be registered.</param>
    /// <remarks>
    /// Use this method to register methods of a class instance. Instances that are created are managed and 
    /// will be disposed of, if they implement IDisposable, when their methods are unregistered.
    /// </remarks>
    void RegisterClass<T>(T instance, RegisteringBehaviour registeringBehaviour) where T : class;

    /// <summary>
    /// Registers all static methods of the specified class type. 
    /// The methods within the class type can then be executed based on their names and parameters.
    /// </summary>
    /// <param name="classType">The type of the class whose static methods will be registered.</param>
    /// <remarks>
    /// Use this method to register static methods of a class without the need to specify generic type parameters.
    /// </remarks>
    void RegisterClass(Type classType);

    /// <summary>
    /// Registers all static methods of the specified class type with the provided registering behaviour.
    /// The methods within the class type can then be executed based on their names and parameters.
    /// </summary>
    /// <param name="classType">The type of the class whose static methods will be registered.</param>
    /// <param name="registeringBehaviour">The behavior with which the methods should be registered.</param>
    /// <remarks>
    /// Use this method when you wish to register static methods of a class using a specific registering behaviour 
    /// and without the need to specify generic type parameters.
    /// </remarks>
    void RegisterClass(Type classType, RegisteringBehaviour registeringBehaviour);

	/// <summary>
	///    Registers a method.
	///		Use this overload when the method to be registered should adhere a different RegisteringBehaviour than the one specified on construction.
	/// </summary>
	/// <param name="del">The <see cref="Delegate" /> to be registered</param>
	/// <param name="registeringBehaviour"><see cref="RegisteringBehaviour"/></param>
	/// <param name="name">the name to later call the method by</param>
	void Register(Delegate del, RegisteringBehaviour registeringBehaviour, string name = null);

	/// <summary>
	///     Finds the best fitting method by return type and parameters, and executes it.
	/// </summary>
	/// <param name="parameters">The parameters passed to the method to be called</param>
	/// <typeparam name="T">The return Type of the function to be called</typeparam>
	/// <returns></returns>
	T SimpleExecute<T>(params object[] parameters);

	/// <summary>
	///     Finds the best fitting method by parameters, and executes it.
	///     The called method must not return a value.
	/// </summary>
	/// <param name="parameters">The parameters passed to the method to be called</param>
	void SimpleExecute(params object[] parameters);

	/// <summary>
	///     Does the same as <see cref="Execute{T}(string, object[])" /> but throws no exception.
	///     The return value of the called method is returned  via the parameter <paramref name="returnValue" />
	/// </summary>
	/// <param name="returnValue">The <code>out</code> parameter for the return value</param>
	/// <param name="name">The name of the method to be called</param>
	/// <param name="parameters">The parameters passed to the method to be called</param>
	/// <typeparam name="T">The return type of the method to be called (used for finding the best fitting method)</typeparam>
	/// <returns><code>true</code> when the method was found and executed <code>false</code> when not</returns>
	bool TryExecute<T>(out T returnValue, string name, params object[] parameters);

	/// <summary>
	///     same as <see cref="TryExecute{T}" /> but for <code>void</code> methods.
	/// </summary>
	/// <param name="name">The name of the method to be called</param>
	/// <param name="parameters">The parameters passed to the method to be called</param>
	/// <returns><code>true</code> when the method was found and executed <code>false</code> when not</returns>
	bool TryExecute(string name, params object[] parameters);

	/// <summary>
	///     Unregisters a method.
	/// </summary>
	/// <param name="del">the method to be unregistered</param>
	void Unregister([NotNull] Delegate del);

	/// <summary>
	///     Unregisters a method by name.
	/// </summary>
	/// <seealso cref="Unregister(System.Delegate)" />
	/// <param name="name"></param>
	void Unregister([NotNull] string name);
}