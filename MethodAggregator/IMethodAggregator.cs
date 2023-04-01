#region Usings

using System;
using JetBrains.Annotations;

#endregion

namespace MethodAggregator
{
	public interface IMethodAggregator : IDisposable
	{
		T Execute<T>(string name, params object[] parameters);
		void Execute(string name, params object[] parameters);
		bool IsRegistered([NotNull] Delegate del);
		bool IsRegistered([NotNull] string name);
		void Register([NotNull] Delegate del, string name = null);
		T SimpleExecute<T>(params object[] parameters);
		void SimpleExecute(params object[] parameters);
		bool TryExecute<T>(out T returnValue, string name, params object[] parameters);
		bool TryExecute(string name, params object[] parameters);
		void Unregister([NotNull] Delegate del);
	}
}