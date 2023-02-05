using System;

namespace MokomoGamesLib.Runtime.StateMachine
{
    public class StateHistoryItem
    {
        public StateHistoryItem(Type stateType, StateChangeRequest changeRequest)
        {
            StateType = stateType;
            ChangeRequest = changeRequest;
        }

        public Type StateType { get; }
        public StateChangeRequest ChangeRequest { get; }
    }
}