#region Usings

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

#endregion

namespace MethodAggregator
{
	internal class ThreadingDictionary<TK, TV> : ConcurrentDictionary<TK, TV>, IDisposable, ICloneable
	{
		private readonly bool _valueIsValueType = typeof(TV).IsValueType;

        /// <summary>
        ///     Gets or sets the try counter.
        /// </summary>
        /// <value>The try counter.</value>
		private static int TryCounter => 100;

		public object Clone() => MemberwiseClone();

		#region Implementation of IDisposable

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
		{
			foreach (IDisposable value in Values.OfType<IDisposable>()) value.Dispose();
			Clear();
		}

		#endregion

        /// <summary>
        ///     Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <param name="replace">Replace if key already exists</param>
        /// <exception cref="AccessViolationException">No access for adding {nameof(key)}</exception>
        /// <exception cref="System.AccessViolationException">Thrown if the key is not in the dictionary.</exception>
        public void Add([NotNull] TK key, TV value, bool replace = false)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (ContainsKey(key))
			{
				if (replace) RemoveKey(key, true);
				else throw new ArgumentException($"Key {key} already exists in the dictionary.");
			}

			for (int i = 0; i < TryCounter; i++)
			{
				if (TryAdd(key, value)) break;
			}

			if (!ContainsKey(key)) throw new AccessViolationException($"No access for adding {nameof(key)} to the ConcurrentDictionary.");
			OnItemsChanged();
		}

        /// <summary>
        ///     Adds the range.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="replace">if set to <c>true</c> [replace].</param>
        public virtual void AddRange([NotNull] IEnumerable<KeyValuePair<TK, TV>> collection, bool replace = false)
		{
			if (collection == null) throw new ArgumentNullException(nameof(collection));
			foreach (KeyValuePair<TK, TV> keyValuePair in collection) Add(keyValuePair.Key, keyValuePair.Value, replace);
			OnItemsChanged();
		}

		public bool ContainsValue([NotNull] TV value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return this.Any(p => value.Equals(p.Value));
		}

        /// <summary>
        ///     Gets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>V.</returns>
        /// <exception cref="AccessViolationException">No access for getting value {key?.ToString() ?? nameof(key)}</exception>
        /// <exception cref="System.AccessViolationException">Thrown if the value is null.</exception>
        public virtual TV GetValue([NotNull] TK key)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			TV value = default;
			if (!ContainsKey(key)) return value;
			int i = 0;
			bool accessGranted = false;
			while (i++ < TryCounter)
			{
				if (!TryGetValue(key, out value)) continue;
				accessGranted = true;
				break;
			}

			if (!accessGranted) throw new AccessViolationException($"No access for getting value {key?.ToString() ?? nameof(key)} from the ConcurrentDictionary.");
			return value;
		}

        /// <summary>
        ///     One or more items in the Dictionary was added or removed.
        /// </summary>
        public event EventHandler ItemsChanged;

        /// <summary>
        ///     Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="suppressLogging">Suppress logging</param>
        /// <exception cref="AccessViolationException">No access for removing {nameof(key)}</exception>
        /// <exception cref="System.AccessViolationException">Thrown if the key is not in the dictionary.</exception>
        public virtual void RemoveKey([NotNull] TK key, bool suppressLogging = false)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (!ContainsKey(key)) return;
			int i = 0;
			while (i++ < TryCounter)
			{
				if (TryRemove(key, out TV _)) break;
			}

			if (ContainsKey(key)) throw new AccessViolationException($"No access for removing {nameof(key)} from the ConcurrentDictionary.");
			OnItemsChanged();
		}

        /// <summary>
        ///     Removes the value.
        /// </summary>
        /// <param name="value">The value.</param>
        public virtual void RemoveValues(TV value)
		{
			if (this.All(pair => !IsEquals(pair.Value, value))) return;
			foreach (KeyValuePair<TK, TV> pair in this.Where(pair => IsEquals(pair.Value, value))) RemoveKey(pair.Key);
			OnItemsChanged();
		}

        /// <summary>
        ///     Raise event if one or more items in the Dictionary was added or removed.
        /// </summary>
        protected virtual void OnItemsChanged() { ItemsChanged?.Invoke(this, EventArgs.Empty); }

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        private bool IsEquals(TV value1, TV value2) => value1 == null ? value2 == null : _valueIsValueType ? value1.Equals(value2) : ReferenceEquals(value1, value2);
	}
}