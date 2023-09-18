using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace MethodAggregator;

internal static class MethodInfoExtension
{
    public static Delegate CreateDelegate([NotNull] this MethodInfo methodInfo, object target = null) 
    {
        if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
        bool isAction = methodInfo.ReturnType == typeof(void);
        IEnumerable<Type> types = methodInfo.GetParameters().Select(p => p.ParameterType);

        Func<Type[], Type> getType;
        if (isAction) getType = Expression.GetActionType;
        else 
        {
            getType = Expression.GetFuncType;
            types = types.Concat(new[] { methodInfo.ReturnType });
        }

        Type type = getType(types.ToArray()) ?? throw new ArgumentNullException("getType(types.ToArray())");
        if (methodInfo.IsStatic) return methodInfo.CreateDelegate(type);
        if (target == null) throw new ArgumentNullException(nameof(target));
        return methodInfo.CreateDelegate(type, target);
    }
}