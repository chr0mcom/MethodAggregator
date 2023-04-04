#region Usings

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

#endregion

[assembly:InternalsVisibleTo("MethodAggregator.Tests")]

namespace MethodAggregator;

internal class TypeConversion
{
	[NotNull] private static readonly ConcurrentDictionary<Type, TypeNode> TypeNodeDictionary = new ();

	public static Type GetBestNativeTypeMatch([NotNull] Type inputType, [NotNull] List<Type> checkTypes)
	{
		if (inputType == null) throw new ArgumentNullException(nameof(inputType));
		if (checkTypes == null) throw new ArgumentNullException(nameof(checkTypes));
		Type bestMatch = null;
		int bestScore = 0;
		foreach (Type targetType in checkTypes)
		{
			if (targetType == null) continue;
			int score = GetMatchScore(inputType, targetType);
			if (score <= bestScore) continue;
			bestMatch = targetType;
			bestScore = score;
		}

		return bestMatch;
	}

	public static Type GetHighestInheritedType([NotNull] Type callerType, [NotNull] IEnumerable<Type> methodTypes)
	{
		if (callerType == null) throw new ArgumentNullException(nameof(callerType));
		if (methodTypes == null) throw new ArgumentNullException(nameof(methodTypes));
		if (!TypeNodeDictionary.TryGetValue(callerType, out TypeNode buildTypeNode))
		{
			buildTypeNode = BuildTypeTree(callerType);
			TypeNodeDictionary.TryAdd(callerType, buildTypeNode);
		}

		if (buildTypeNode == null) throw new ArgumentNullException(nameof(buildTypeNode));
		IEnumerable<(Type methodType, TypeNode assignableType)> assignableTypes = GetAssignableTypes(buildTypeNode, methodTypes.ToList());
		return assignableTypes.OrderBy(t => t.assignableType.Level).ThenBy(t => t.assignableType.Order).Select(t => t.methodType).FirstOrDefault() ?? throw new InvalidOperationException("Could not find Type for method parameter");
	}

	public static bool IsAssignableOrConvertibleTo([NotNull] Type fromType, [NotNull] Type toType)
	{
		if (fromType == null) throw new ArgumentNullException(nameof(fromType));
		if (toType == null) throw new ArgumentNullException(nameof(toType));
		return toType.IsAssignableFrom(fromType) || IsConvertibleTo(fromType, toType);
	}

	public static bool IsConvertibleTo([NotNull] Type fromType, [NotNull] Type toType)
	{
		if (fromType == null) throw new ArgumentNullException(nameof(fromType));
		if (toType == null) throw new ArgumentNullException(nameof(toType));
		try
		{
			return Convert.ChangeType(Activator.CreateInstance(fromType), toType) != null;
		} catch
		{
			return false;
		}
	}

	public static bool IsNativeType([NotNull] Type type)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));
		return type.Namespace == typeof(int).Namespace;
	}

	[NotNull]
	private static TypeNode BuildTypeTree(Type type)
	{
		TypeNode root = new TypeNode {Type = type, Level = 0};
		BuildTypeTreeRecursively(root);
		return root;
	}

	private static void BuildTypeTreeRecursively([NotNull] TypeNode node)
	{
		if (node == null) throw new ArgumentNullException(nameof(node));
		if (node.Type == null) throw new ArgumentNullException(nameof(node.Type));
		List<Type> types = GetCurrentLevelTypes(node.Type);
		int level = node.Level + 1;
		foreach (Type type in types)
		{
			TypeNode childNode = new TypeNode {Type = type, Level = level, Parent = node};
			node.Children?.Add(childNode);
			BuildTypeTreeRecursively(childNode);
		}
	}

	[NotNull]
	private static List<Type> GetAllBaseTypes([NotNull] Type type)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));
		List<Type> retList = new List<Type>();
		if (type.BaseType == null || type.BaseType == typeof(object)) return retList;
		retList.Add(type.BaseType);
		retList.AddRange(GetAllBaseTypes(type.BaseType));
		return retList;
	}

	[NotNull]
	private static List<Type> GetAllImplementedTypes([NotNull] Type type)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));
		HashSet<Type> hashSet = new HashSet<Type>();
		foreach (Type baseType in GetAllBaseTypes(type)) hashSet.Add(baseType);
		foreach (Type interfaceType in type.GetInterfaces()) hashSet.Add(interfaceType);
		return hashSet.ToList();
	}

	private static int GetAlphanumericMatchScore(Type targetType)
	{
		if (targetType == typeof(string))
		{
			// string is preferred over char
			return 4;
		}

		if (targetType == typeof(char))
		{
			// char is less preferred than string
			return 3;
		}

		// no match
		return 0;
	}

	private static TypeNode GetAssignableType([NotNull] TypeNode typeNode, Type targetType)
	{
		if (typeNode == null) throw new ArgumentNullException(nameof(typeNode));
		return typeNode.Type == targetType ? typeNode : typeNode.Children?.Where(child => child != null).Select(child => GetAssignableType(child, targetType)).FirstOrDefault(assignableType => assignableType != null);
	}

	[NotNull]
	private static IEnumerable<(Type methodType, TypeNode assignableType)> GetAssignableTypes([NotNull] TypeNode typeNode, [NotNull] List<Type> methodTypes)
	{
		if (typeNode == null) throw new ArgumentNullException(nameof(typeNode));
		if (methodTypes == null) throw new ArgumentNullException(nameof(methodTypes));
		List<(Type methodType, TypeNode assignableType)> result = new List<(Type methodType, TypeNode assignableType)>();
		foreach (Type methodType in methodTypes)
		{
			TypeNode assignableType = GetAssignableType(typeNode, methodType);
			result.Add((methodType, assignableType));
		}

		return result;
	}

	[NotNull]
	private static List<Type> GetCurrentLevelTypes([NotNull] Type rootType)
	{
		if (rootType == null) throw new ArgumentNullException(nameof(rootType));
		List<Type> types = GetAllImplementedTypes(rootType);
		foreach (Type type in types.ToList())
			if (type != null)
				types.RemoveAll(i => GetAllImplementedTypes(type).Contains(i));
		return types;
	}

	private static int GetIntegerMatchScore(Type inputType, Type targetType)
	{
		if (targetType == typeof(long))
		{
			if (inputType == typeof(ulong))
			{
				// ulong can be converted to long, but not the other way around
				return 4;
			}

			// check for compatibility based on range
			if (GetMaxValue(inputType) <= long.MaxValue)
			{
				// inputType has same or smaller range than targetType
				return 4;
			}
		}

		if (targetType == typeof(int))
		{
			if (inputType == typeof(long) || inputType == typeof(ulong))
			{
				// long and ulong can be converted to int, but not the other way around
				return 4;
			}

			// check for compatibility based on range
			if (GetMaxValue(inputType) <= int.MaxValue)
			{
				// inputType has same or smaller range than targetType
				return 4;
			}
		}

		if (targetType == typeof(short))
		{
			if (inputType == typeof(int) || inputType == typeof(long) || inputType == typeof(ulong))
			{
				// int, long, and ulong can be converted to short, but not the other way around
				return 4;
			}

			// check for compatibility based on range
			long inputMax = (long) GetMaxValue(inputType);
			if (inputMax <= short.MaxValue)
			{
				// inputType has same or smaller range than targetType
				return 4;
			}
		}

		if (targetType == typeof(sbyte))
		{
			if (inputType == typeof(short) || inputType == typeof(int) || inputType == typeof(long) || inputType == typeof(ulong))
			{
				// short, int, long, and ulong can be converted to sbyte, but not the other way around
				return 4;
			}

			// check for compatibility based on range
			long inputMax = (long) GetMaxValue(inputType);
			if (inputMax <= sbyte.MaxValue)
			{
				// inputType has same or smaller range than targetType
				return 4;
			}
		}

		if (targetType == typeof(double))
		{
			if (inputType == typeof(decimal))
			{
				// decimal can be converted to double, but not the other way around
				return 4;
			}

			// check for compatibility based on range
			double inputMax = (double) GetMaxValue(inputType);
			if (inputMax <= double.MaxValue)
			{
				// inputType has same or smaller range than targetType
				return 4;
			}
		}

		if (targetType == typeof(float))
		{
			if (inputType == typeof(decimal) || inputType == typeof(double))
			{
				// decimal and double can be converted to float, but not the other way around
				return 4;
			}

			// check for compatibility based on range
			float inputMax = (float) GetMaxValue(inputType);
			if (inputMax <= float.MaxValue)
			{
				// inputType has same or smaller range than targetType
				return 4;
			}
		}

		if (targetType == typeof(decimal))
		{
			if (!HasDecimalPlaces(inputType))
			{
				// inputType has no decimal places
				return 3;
			}

			// check for compatibility based on range
			decimal inputMax = GetMaxValue(inputType);
			decimal targetMax = GetMaxValue(targetType);
			if (inputMax <= targetMax)
			{
				// inputType has same or smaller range
				return 3;
			}
		}

		if (targetType == typeof(uint))
		{
			if (inputType == typeof(ulong))
			{
				// ulong can be converted to uint, but not the other way around
				return 2;
			}

			// check for compatibility based on range
			decimal inputMax = GetMaxValue(inputType);
			if (inputMax <= uint.MaxValue)
			{
				// inputType has same or smaller range than targetType
				return 2;
			}
		}

		if (targetType == typeof(ushort))
		{
			if (inputType == typeof(uint) || inputType == typeof(ulong))
			{
				// uint and ulong can be converted to ushort, but not the other way around
				return 2;
			}

			// check for compatibility based on range
			decimal inputMax = GetMaxValue(inputType);
			if (inputMax <= ushort.MaxValue)
			{
				// inputType has same or smaller range than targetType
				return 2;
			}
		}

		if (targetType == typeof(byte))
		{
			if (inputType == typeof(ushort) || inputType == typeof(uint) || inputType == typeof(ulong))
			{
				// ushort, uint, and ulong can be converted to byte, but not the other way around
				return 2;
			}

			// check for compatibility based on range
			decimal inputMax = GetMaxValue(inputType);
			if (inputMax <= byte.MaxValue)
			{
				// inputType has same or smaller range than targetType
				return 2;
			}
		}

		// no match
		return 0;
	}

	private static ulong GetIntegerMaxValue(Type type)
	{
		if (type == typeof(byte)) return byte.MaxValue;
		if (type == typeof(sbyte)) return (ulong) sbyte.MaxValue;
		if (type == typeof(short)) return (ulong) short.MaxValue;
		if (type == typeof(ushort)) return ushort.MaxValue;
		if (type == typeof(int)) return int.MaxValue;
		if (type == typeof(uint)) return uint.MaxValue;
		if (type == typeof(long)) return long.MaxValue;
		if (type == typeof(ulong)) return ulong.MaxValue;
		throw new ArgumentException($"Type {type} is not an integer type.");
	}

	private static int GetMatchScore([NotNull] Type inputType, [NotNull] Type targetType)
	{
		if (inputType == null) throw new ArgumentNullException(nameof(inputType));
		if (targetType == null) throw new ArgumentNullException(nameof(targetType));
		int score = 0;
		if (targetType == inputType)
		{
			// exact match
			score = 6;
		}
		else if (IsNumericType(targetType) && IsNumericType(inputType))
		{
			// both types are numeric
			score = GetNumericMatchScore(inputType, targetType);
		}
		else if (IsAlphanumericType(targetType) && IsAlphanumericType(inputType))
		{
			// both types are alphanumeric
			score = GetAlphanumericMatchScore(targetType);
		}

		if (score == 0 && IsAssignableOrConvertibleTo(inputType, targetType))
		{
			// inputType can be assigned to targetType
			score = 1;
		}

		return score;
	}

	private static decimal GetMaxValue(Type type)
	{
		if (IsIntegerType(type) || IsUnsignedIntegerType(type)) return GetIntegerMaxValue(type);
		if (type == typeof(float)) return (decimal) Math.Round(float.MaxValue, 0);
		if (type == typeof(double)) return (decimal) Math.Round(double.MaxValue, 0);
		if (type == typeof(decimal)) return Math.Round(decimal.MaxValue, 0);
		throw new ArgumentException($"Type {type} is not a numeric type.");
	}

	private static int GetNumericMatchScore(Type inputType, Type targetType)
	{
		if (targetType == typeof(double))
		{
			// double is preferred over decimal or float
			return 4;
		}

		if (IsIntegerType(targetType) && IsIntegerType(inputType))
		{
			// both types are integer types
			return GetIntegerMatchScore(inputType, targetType);
		}

		if (targetType == typeof(decimal))
		{
			if (HasDecimalPlaces(inputType))
			{
				// inputType has decimal places
				return 3;
			}

			// check for compatibility based on range
			decimal inputMax = GetMaxValue(inputType);
			decimal targetMax = GetMaxValue(targetType);
			if (inputMax <= targetMax)
			{
				// inputType has same or smaller range than targetType
				return 3;
			}
		}

		if (IsUnsignedIntegerType(targetType) && IsUnsignedIntegerType(inputType))
		{
			// both types are unsigned integer types
			return GetUnsignedIntegerMatchScore(inputType, targetType);
		}

		// no match
		return 0;
	}

	private static int GetUnsignedIntegerMatchScore(Type inputType, Type targetType)
	{
		if (targetType == typeof(uint))
		{
			// uint is preferred over ushort or byte
			return 4;
		}

		if (targetType == typeof(ushort))
		{
			if (inputType == typeof(uint))
			{
				// uint can be converted to ushort, but not the other way around
				return 3;
			}

			// check for compatibility based on range
			decimal inputMax = GetMaxValue(inputType);
			if (inputMax <= ushort.MaxValue)
			{
				// inputType has same or smaller range than targetType
				return 3;
			}
		}

		if (targetType == typeof(byte))
		{
			if (inputType == typeof(ushort) || inputType == typeof(uint))
			{
				// ushort and uint can be converted to byte, but not the other way around
				return 3;
			}

			// check for compatibility based on range
			decimal inputMax = GetMaxValue(inputType);
			if (inputMax <= byte.MaxValue)
			{
				// inputType has same or smaller range than targetType
				return 3;
			}
		}

		// no match
		return 0;
	}

	private static bool HasDecimalPlaces(Type type)
	{
		if (type == typeof(float)) return true;
		if (type == typeof(double)) return true;
		if (type == typeof(decimal)) return true;
		return false;
	}

	private static bool IsAlphanumericType(Type type) => type == typeof(string) || type == typeof(char);

	private static bool IsIntegerType(Type type) => type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong);

	private static bool IsNumericType(Type type) => type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int) || type == typeof(uint) || type == typeof(long) || type == typeof(ulong) || type == typeof(float) || type == typeof(double) || type == typeof(decimal);

	private static bool IsUnsignedIntegerType(Type type) => type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);

	private class TypeNode
	{
		public Type Type { get; set; }
		public List<TypeNode> Children { get; } = new List<TypeNode>();
		public int Level { get; set; }
		public TypeNode Parent { get; set; }
		public int Order => Parent != null ? (Parent.Children?.IndexOf(this) ?? 0) + 1 : 0;
	}
}