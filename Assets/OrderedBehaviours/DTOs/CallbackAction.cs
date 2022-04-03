using System;

namespace OrderedBehaviors
{
    public class CallbackAction
    {
        public Action Action;
        public OrderedGroupCallbackSettings[] OrderedGroupCallbacks;
        public TimedCallbackSettings[] TimedCallbacks;
    }
}
