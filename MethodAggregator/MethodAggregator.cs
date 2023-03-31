﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace MethodAggregator
{
    public class MethodAggregator : IMethodAggregator
    {
	    [NotNull] private readonly Dictionary<Delegate, string> _registeredMethods = new Dictionary<Delegate, string>();

        private IEnumerable<Delegate> GetMethods(string name = null) => name == null ? _registeredMethods.Keys : _registeredMethods.Where(p => p.Value == name).Select(p => p.Key);

	    [NotNull] private List<Delegate> FilterCountOfParameterMatches(string name, Type returnType, ICollection parameterTypes) => (returnType == typeof(void) ? GetMethods(name)?.Where(d => d != null && parameterTypes != null &&  d.Method.ReturnType == returnType && d.Method.GetParameters().Length == parameterTypes.Count).ToList() : GetMethods(name)?.Where(d => d != null && parameterTypes != null && d.Method.GetParameters().Length == parameterTypes.Count).ToList()) ?? new List<Delegate>();

	    [NotNull] private static List<Delegate> FilterParameterTypesAreAssignable([NotNull] List<Delegate> delegates, [NotNull] Type returnType, [NotNull] IReadOnlyList<Type> parameterTypes)
	    {
            if (delegates == null) throw new ArgumentNullException(nameof(delegates));
            if (returnType == null) throw new ArgumentNullException(nameof(returnType));
            if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));
            List<Delegate> filteredList = new List<Delegate>();
		    foreach (Delegate del in delegates)
		    {
                if (del == null) continue;
			    if (!TypeConversion.IsAssignableOrConvertibleTo(del.Method.ReturnType, returnType)) continue;
			    Type[] methodParameterInfos = del.Method.GetParameters().Select(p => p.ParameterType). ToArray();
			    bool isMatched = !parameterTypes.Where((t, index) => t != null && methodParameterInfos[index] != null && !TypeConversion.IsAssignableOrConvertibleTo(t, methodParameterInfos[index])).Any();
			    if (isMatched) filteredList.Add(del);
		    }

		    return filteredList;
	    }

	    private static Delegate FilterParameterBestTypeMatches([NotNull] List<Delegate> delegates, [NotNull] Type returnType, [NotNull] IReadOnlyList<Type> parameterTypes)
	    {
            if (delegates == null) throw new ArgumentNullException(nameof(delegates));
            if (returnType == null) throw new ArgumentNullException(nameof(returnType));
            if (returnType == null) throw new ArgumentNullException(nameof(returnType));
            if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));
            if (parameterTypes == null) throw new ArgumentNullException(nameof(parameterTypes));
            int parameterTypesCount = parameterTypes.Count;
		    for (int i = 0; i < parameterTypesCount; i++)
		    {
			    List<Type> parameterToCheck = delegates.ToList().Where(del => del != null).Select(del => del.Method.GetParameters()[i].ParameterType).ToList();
                Type parameterType = parameterTypes[i];
                if (parameterType == null) throw new ArgumentNullException(nameof(parameterType));
                Type priorityType = TypeConversion.IsNativeType(parameterType) ? TypeConversion.GetBestNativeTypeMatch(parameterType, parameterToCheck) : TypeConversion.GetHighestInheritedType(parameterType, parameterToCheck);
			    delegates = delegates.Where(d => d != null && d.Method.GetParameters()[i].ParameterType == priorityType).ToList();
		    }

            List<Type> returnTypesToCheck = delegates.Where(d => d != null).Select(d => d.Method.ReturnType).ToList();
            Type highestPriorityType = TypeConversion.IsNativeType(returnType) ? TypeConversion.GetBestNativeTypeMatch(returnType, returnTypesToCheck) : TypeConversion.GetHighestInheritedType(returnType, returnTypesToCheck);
            return delegates.FirstOrDefault(d => d != null && d.Method.ReturnType == highestPriorityType);
	    }

	    private Delegate FindDelegate(string name, [NotNull] Type returnType, [NotNull] object[] parameters)
	    {
            if (returnType == null) throw new ArgumentNullException(nameof(returnType));
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            List<Type> parameterTypes = parameters.Where(o => o != null).Select(o => o.GetType()).ToList();
            List<Delegate> filterCountOfParameterMatches = FilterCountOfParameterMatches(name, returnType, parameterTypes);
            List<Delegate> filterParameterTypesAreAssignable = FilterParameterTypesAreAssignable(filterCountOfParameterMatches, returnType, parameterTypes);
            return FilterParameterBestTypeMatches(filterParameterTypesAreAssignable, returnType, parameterTypes);
	    }

        public T Execute<T>(string name, [NotNull] params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            Delegate del = FindDelegate(name, typeof(T), parameters);
            if (del == null) throw new InvalidOperationException("No method found for given parameters.");
            object invokedValue = del.DynamicInvoke(parameters);
            try
            {
                return (T)invokedValue;
            } catch
            {
                try
                {
                    return (T)Convert.ChangeType(invokedValue, typeof(T));
                } catch 
                {
                    return default;
                }
            }
        }

        public void Execute(string name, [NotNull] params object[] parameters) 
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            Delegate del = FindDelegate(name, typeof(void), parameters);
            if (del == null) throw new InvalidOperationException("No method found for given parameters.");
            del.DynamicInvoke(parameters);
        }

        public T SimpleExecute<T>([NotNull] params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            return Execute<T>(null, parameters);
        }

        public void SimpleExecute([NotNull] params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            Execute(null, parameters);
        }

        public bool TryExecute<T>(out T returnValue, string name, [NotNull] params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            returnValue = Execute<T>(name, parameters);
            return !(returnValue == null || returnValue.Equals(default(T)));
        }

        public bool TryExecute(string name, [NotNull] params object[] parameters)
        {
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            try
            {
                Execute(name, parameters);
                return true;
            } catch
            { 
                return false;
            }
        }

        public bool IsRegistered(Delegate del)
        {
            if (del == null) throw new ArgumentNullException(nameof(del));
            return _registeredMethods.ContainsKey(del);
        }

        public void Register(Delegate del, string name = null)
        {
            if (del == null) throw new ArgumentNullException(nameof(del));
            _registeredMethods.Add(del, name ?? del.Method.Name);
        }

        public void Unregister(Delegate del)
        {
            if (del == null) throw new ArgumentNullException(nameof(del));
            _registeredMethods.Remove(del);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _registeredMethods.Clear();
		    GC.Collect();
        }
    }
}