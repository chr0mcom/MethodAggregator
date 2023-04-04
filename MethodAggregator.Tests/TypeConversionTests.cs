using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

namespace MethodAggregator.Tests;

public class TypeConversionTests : TypeConversionTestBase
{
	[Theory]
	[InlineData(typeof(int), typeof(int))]
	[InlineData(typeof(uint), typeof(uint))]
	[InlineData(typeof(byte), typeof(byte))]
	[InlineData(typeof(short), typeof(short))]
	[InlineData(typeof(sbyte), typeof(sbyte))]
	[InlineData(typeof(ushort), typeof(ushort))]
	[InlineData(typeof(long), typeof(long))]
	[InlineData(typeof(ulong), typeof(ulong))]
	[InlineData(typeof(float), typeof(float))]
	[InlineData(typeof(double), typeof(double))]
	[InlineData(typeof(decimal), typeof(decimal))]
	[InlineData(typeof(bool), typeof(bool))]
	[InlineData(typeof(char), typeof(char))]
	[InlineData(typeof(string), typeof(string))]
	public void GetHighestPriorityNativeType_GetSameType_SameTypeIsReturned([NotNull] Type inputType, [NotNull] Type expectedOutputType)
	{
		#region Arrange

		List<Type> types = new()
							{
									typeof(int), typeof(uint), typeof(byte), typeof(short),
									typeof(sbyte), typeof(ushort), typeof(long), typeof(ulong),
									typeof(float), typeof(double), typeof(decimal), typeof(bool),
									typeof(char), typeof(string)
							};

		#endregion

		#region Act

		Type outputType = TypeConversion.GetBestNativeTypeMatch(inputType, types);

		#endregion Act

		#region Assert

		outputType.ShouldBe(expectedOutputType);

		#endregion Assert
	}

	[Fact]
	public void GetHighestPriorityNativeType_GetForUnitWithUshortAndDecimal_DecimalIsReturned()
	{
		#region Arrange

		const uint value = 5;
		Type expectedType = typeof(decimal);

		#endregion Arrange

		#region Act

		// ist decimal wirklich besser als ushort für eine 5?
		Type bestNativeTypeMatch = TypeConversion.GetBestNativeTypeMatch(value.GetType(), new List<Type> {typeof(decimal), typeof(float), typeof(ushort)});

		#endregion Act

		#region Assert

		bestNativeTypeMatch.ShouldBe(expectedType);

		#endregion Assert
	}
	
	
	[Theory]
	[InlineData(typeof(Device), new[] {typeof(Serial), typeof(IObject), typeof(IObject4), typeof(IObject5)}, typeof(Serial))]
	[InlineData(typeof(Device), new[] {typeof(IObject3), typeof(IObject), typeof(IObject4), typeof(IObject5)}, typeof(IObject))]
	[InlineData(typeof(Device), new[] {typeof(Device), typeof(IObject2), typeof(IObject), typeof(IObject4), typeof(IObject5)}, typeof(Device))]
	public void GetHighestPriorityType_CheckForTypeFromList_ExpectedTypeIsFound(Type callerType, Type[] types, Type expectedResult)
	{
		#region Act

		Type highestPriorityType = TypeConversion.GetHighestInheritedType(callerType, types);

		#endregion Act

		#region Assert

		highestPriorityType.ShouldBe(expectedResult);

		#endregion Assert
	}
}