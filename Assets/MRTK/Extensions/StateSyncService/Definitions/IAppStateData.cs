using System.Collections.Generic;
using System;

namespace Microsoft.MixedReality.Toolkit.Extensions.Sharing
{
    public interface IStateDataManager : IEnumerable<IStateArrayBase>
    {
        Action<Type, List<object>> OnReceiveChangedStates { get; set; }
        bool Synchronized { get; }

        void SetSharingService(ISharingService sharingService);
        void CreateStateArray(Type stateType);
        bool ContainsStateType(Type stateType);
        bool TryGetData(Type stateType, out IStateArrayBase stateArray);
        bool TryGetData<T>(out IStateArray<T> stateArray) where T : struct, IState, IStateComparer<T>;
    }
}