using System;
using System.Collections.Generic;
using System.Linq;

namespace MokomoGamesLib.Runtime.Event
{
    public static class EventMessageRunner
    {
        public static void Run(List<EventMessage> messages)
        {
            foreach (var eventMessage in
                     messages.OrderBy(x => x.Priority))
                eventMessage.ExecuteAction();
            messages.Clear();
        }
    }

    public class EventMessage
    {
        public readonly Action ExecuteAction;

        public EventMessage(int priority, Action executeAction)
        {
            Priority = priority;
            ExecuteAction = executeAction;
        }

        public int Priority { get; }
    }
}