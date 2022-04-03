using System.Reflection;

namespace OrderedBehaviors
{
    public class MethodCallback
    {
        public MethodInfo Method;
        public OrderedGroupCallbackSettings[] GroupCallbacks;
        public TimedCallbackSettings[] TimedCallbacks;
    }
}
