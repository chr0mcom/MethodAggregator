﻿#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

#endregion

namespace MethodAggregator;

/// <inheritdoc />
public class MethodAggregator : IMethodAggregator
{
    [NotNull] private readonly List<string> _objectMethodNames = typeof(object).GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public).Select(m => m.Name).ToList();
    [NotNull] private readonly ThreadingDictionary<Delegate, (string name, object lockObject)> _registeredMethods = new();
    [NotNull] private readonly HashSet<object> _instanceContainer = new();
	private readonly RegisteringBehaviour _registeringBehaviour;

    /// <summary>
	///    Instantiates a new <see cref="MethodAggregator" />.
	/// </summary>
	/// <param name="registeringBehaviour">defines the convention for registering methods.</param>
	/// <seealso cref="RegisteringBehaviour"/>
	public MethodAggregator(RegisteringBehaviour registeringBehaviour = RegisteringBehaviour.ClassAndMethodName) { _registeringBehaviour = registeringBehaviour; }

    #region Executes

    /// <inheritdoc />
    public T Execute<T>(string name, [NotNull] params object[] parameters)
    {
        Delegate del = FindDelegate(name, typeof(T), parameters);
        if (del == null) throw new InvalidOperationException("No method found for given parameters.");
        object lockObject = _registeredMethods.GetValue(del).lockObject ?? throw new ArgumentNullException(nameof(lockObject));
        object invokedValue;
        lock (lockObject) invokedValue = del.DynamicInvoke(parameters);
        try
        {
            return (T) invokedValue;
        } catch
        {
            try
            {
                return (T) Convert.ChangeType(invokedValue, typeof(T));
            } catch
            {
                return default;
            }
        }
    }

    /// <inheritdoc />
    public void Execute(string name, [NotNull] params object[] parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        Delegate del = FindDelegate(name, typeof(void), parameters);
        if (del == null) throw new InvalidOperationException("No method found for given parameters.");
        del.DynamicInvoke(parameters);
    }

    /// <inheritdoc />
    public T SimpleExecute<T>([NotNull] params object[] parameters) => Execute<T>(null, parameters);

    /// <inheritdoc />
    public void SimpleExecute([NotNull] params object[] parameters) => Execute(null, parameters);

    /// <inheritdoc />
    public bool TryExecute<T>(out T returnValue, string name, [NotNull] params object[] parameters)
    {
        returnValue = Execute<T>(name, parameters);
        return !(returnValue == null || returnValue.Equals(default(T)));
    }

    /// <inheritdoc />
    public bool TryExecute(string name, [NotNull] params object[] parameters)
    {
        try
        {
            Execute(name, parameters);
            return true;
        } catch
        {
            return false;
        }
    }

    #endregion Executes

    #region IsRegistered

    /// <inheritdoc />
    public bool IsRegistered(Delegate del)
    {
        if (del == null) throw new ArgumentNullException(nameof(del));
        return _registeredMethods.ContainsKey(del);
    }

    /// <inheritdoc />
    public bool IsRegistered(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return _registeredMethods.Any(d => d.Value.name == name);
    }

    #endregion IsRegistered

    #region Registers

	/// <inheritdoc />
	public void Register(Delegate del, string name = null)
	{
		if (del == null) throw new ArgumentNullException(nameof(del));
		Register(del, _registeringBehaviour, name);
	}

    /// <inheritdoc />
    public void RegisterClass<T>(T instance = null) where T : class
    {
        if (instance == null) RegisterClass(typeof(T));
        else RegisterClass(instance);
    }

    /// <inheritdoc />
    public void RegisterClass<T>(RegisteringBehaviour registeringBehaviour) where T : class => RegisterClass(typeof(T), registeringBehaviour);

    /// <inheritdoc />
    public void RegisterClass<T>(T instance, RegisteringBehaviour registeringBehaviour) where T : class
    {
        Type classType = instance?.GetType() ?? typeof(T);
        if (instance == null && classType.IsAbstract) throw new ArgumentNullException(nameof(instance));

        List<MethodInfo> methodInfos = classType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)
                                                .Where(m => !_objectMethodNames.Contains(m.Name)).ToList();

        foreach (MethodInfo methodInfo in methodInfos)
        {
            if (methodInfo == null) continue;
            Register(methodInfo.CreateDelegate(instance), registeringBehaviour);
        }
    }

    /// <inheritdoc />
    public void RegisterClass([NotNull] Type classType) => RegisterClass(classType, _registeringBehaviour);

    /// <inheritdoc />
    public void RegisterClass(Type classType, RegisteringBehaviour registeringBehaviour)
    {
        if (classType == null) throw new ArgumentNullException(nameof(classType));

        object classInstance = null;

        if (!classType.IsAbstract)
        {
            classInstance = _instanceContainer.SingleOrDefault(classType.IsInstanceOfType);
            if (classInstance == null)
            {
                classInstance = Activator.CreateInstance(classType);
                _instanceContainer.Add(classInstance);
            }
        }

        RegisterClass(classInstance, registeringBehaviour);
    }

    /// <inheritdoc />
	public void Register(Delegate del, RegisteringBehaviour registeringBehaviour, string name = null)
	{
		if (del == null) throw new ArgumentNullException(nameof(del));
		name ??= registeringBehaviour switch
		{
				RegisteringBehaviour.MethodName => del.Method.Name,
				RegisteringBehaviour.ClassAndMethodName => $"{del.Method.DeclaringType?.Name}.{del.Method.Name}",
				_ => throw new ArgumentOutOfRangeException()
		};
		_registeredMethods.Add(del, (name, new object()));
	}
	
    #endregion Registers

    #region Unregisters
	/// <inheritdoc />
	public void Unregister(Delegate del)
	{
		if (del == null) throw new ArgumentNullException(nameof(del));
		_registeredMethods.RemoveKey(del);
        if (del.GetMethodInfo()?.IsStatic ?? del.Target == null) return;
        if (_registeredMethods.Any(p => p.Key?.Target == del.Target)) return;
        _instanceContainer.Remove(del.Target);
        if (del.Target is IDisposable disposable) disposable.Dispose();
    }

	/// <inheritdoc />
	public void Unregister(string name)
	{
		if (name == null) throw new ArgumentNullException(nameof(name));
		Delegate del = _registeredMethods.FirstOrDefault(d => d.Value.name == name).Key;
		if (del == null) throw new InvalidOperationException($"No method found with given name: '{name}'.");
		Unregister(del);
	}
    #endregion Unregisters

	/// <summary>
	///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		_registeredMethods.Clear();
        foreach (IDisposable disposable in _instanceContainer.OfType<IDisposable>()) disposable.Dispose();
        _instanceContainer.Clear();
		GC.Collect();
		GC.SuppressFinalize(this);
	}

    #region private methods

    private Delegate FindDelegate(string name, [NotNull] Type returnType, [NotNull] IEnumerable<object> parameters)
    {
        List<Type> parameterTypes = parameters.Where(o => o != null).Select(o => o.GetType()).ToList();
        List<Delegate> filterCountOfParameterMatches = FilterCountOfParameterMatches(name, returnType, parameterTypes);
        List<Delegate> filterParameterTypesAreAssignable = FilterParameterTypesAreAssignable(filterCountOfParameterMatches, returnType, parameterTypes);
        return FilterParameterBestTypeMatches(filterParameterTypesAreAssignable, returnType, parameterTypes);
    }

    private IEnumerable<Delegate> GetMethods(string name = null) => name == null ? _registeredMethods.Keys : _registeredMethods.Where(p => p.Value.name == name).Select(p => p.Key);

    [NotNull]
    private List<Delegate> FilterCountOfParameterMatches(string name, Type returnType, ICollection parameterTypes) => (returnType == typeof(void) ? GetMethods(name)?.Where(d => d != null && parameterTypes != null && d.Method.ReturnType == returnType && d.Method.GetParameters().Length == parameterTypes.Count).ToList() : GetMethods(name)?.Where(d => d != null && parameterTypes != null && d.Method.GetParameters().Length == parameterTypes.Count).ToList()) ?? new List<Delegate>();
	
    [NotNull]
    private static List<Delegate> FilterParameterTypesAreAssignable([NotNull] List<Delegate> delegates, [NotNull] Type returnType, [NotNull] IReadOnlyList<Type> parameterTypes)
    {
        if (delegates == null) throw new ArgumentNullException(nameof(delegates));
        if (returnType == null) throw new ArgumentNullException(nameof(returnType));
        if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));
        List<Delegate> filteredList = new();
        foreach (Delegate del in delegates)
        {
            if (del == null) continue;
            if (!TypeConversion.IsAssignableOrConvertibleTo(del.Method.ReturnType, returnType)) continue;
            Type[] methodParameterInfos = del.Method.GetParameters().Select(p => p.ParameterType).ToArray();
            bool isMatched = !parameterTypes.Where((t, index) => t != null && methodParameterInfos[index] != null && !TypeConversion.IsAssignableOrConvertibleTo(t, methodParameterInfos[index])).Any();
            if (isMatched) filteredList.Add(del);
        }

        return filteredList;
    }
	
    private static Delegate FilterParameterBestTypeMatches([NotNull] List<Delegate> delegates, [NotNull] Type returnType, [NotNull] IReadOnlyList<Type> parameterTypes)
    {
        if (delegates == null) throw new ArgumentNullException(nameof(delegates));
        if (returnType == null) throw new ArgumentNullException(nameof(returnType));
        if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));
        int parameterTypesCount = parameterTypes.Count;
        for (int i = 0; i < parameterTypesCount; i++)
        {
            List<Type> parameterToCheck = delegates.ToList().Where(del => del != null).Select(del => del.Method.GetParameters()[i].ParameterType).ToList();
            Type parameterType = parameterTypes[i];
            if (parameterType == null) throw new ArgumentNullException(nameof(parameterType));
            Type priorityType = TypeConversion.IsNativeType(parameterType) ? TypeConversion.GetBestNativeTypeMatch(parameterType, parameterToCheck) : TypeConversion.GetHighestInheritedType(parameterType, parameterToCheck);
            if (priorityType == null) continue;
            delegates = delegates.Where(d => d != null && d.Method.GetParameters()[i].ParameterType == priorityType).ToList();
        }

        List<Type> returnTypesToCheck = delegates.Where(d => d != null).Select(d => d.Method.ReturnType).ToList();
        Type highestPriorityType = TypeConversion.IsNativeType(returnType) ? TypeConversion.GetBestNativeTypeMatch(returnType, returnTypesToCheck) : TypeConversion.GetHighestInheritedTypeInverted(returnType, returnTypesToCheck);
        if (highestPriorityType == null) return null;
        return delegates.FirstOrDefault(d => d != null && d.Method.ReturnType == highestPriorityType);
    }

    #endregion private methods
}