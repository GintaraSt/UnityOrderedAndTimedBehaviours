using System;

namespace OrderedBehaviors
{
    [AttributeUsage(AttributeTargets.Method)]
    public class OrderedGroupAttribute : Attribute
    {
        public readonly string GroupId;
        public readonly int Order;

        public OrderedGroupAttribute(string groupId, int order)
        {
            GroupId = groupId;
            Order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TimedCallbackAttribute : Attribute
    {
        public readonly float TimeInterval;
        public readonly float StartDelay;

        public TimedCallbackAttribute(float timeInterval, float startDelay)
        {
            TimeInterval = timeInterval;
            StartDelay = startDelay;
        }
    }
}
