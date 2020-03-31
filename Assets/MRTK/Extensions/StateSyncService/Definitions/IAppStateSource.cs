using System.Collections.Generic;
using System;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
	public abstract class StateGenerator : ScriptableObject
	{
		public abstract IEnumerable<Type> StateTypes { get; }

		public abstract void GenerateRequiredStates(IStateSyncService appState);

		public virtual int ExecutionOrder { get { return 0; } }
	}
}