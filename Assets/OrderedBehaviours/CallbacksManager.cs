using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Compilation;
using UnityEngine;
using Assembly = System.Reflection.Assembly;

namespace OrderedBehaviors
{
    public static class CallbacksManager
    {
        private static readonly string[] _excludedAssemblyKeywords = new string[] { "UnityEngine", "Unity" };

        private static MethodCallback[] _methodCallbacks;
        private static Dictionary<int, CallbackActionsOfType> _callbackActions = new Dictionary<int, CallbackActionsOfType>();

        private static List<(Action action, int instanceId, TimedCallbackSettings[] settings)> _enabledTimedCallbacks = new List<(Action action, int instanceId, TimedCallbackSettings[] settings)>();
        private static Dictionary<string, List<(Action action, int instanceId, int order)>> _enabledOrderedGroupCallbacks = new Dictionary<string, List<(Action action, int instanceId, int order)>>();

        public static void RunTimedCallbacks()
        {
            var currentTime = Time.unscaledTime;

            foreach (var timedCallback in _enabledTimedCallbacks)
            {
                foreach (var callbackSettings in timedCallback.settings)
                {
                    if (currentTime - callbackSettings.LastRun >= callbackSettings.TimeInterval)
                    {
                        timedCallback.action.Invoke();
                        callbackSettings.LastRun = currentTime;
                    }
                }
            }
        }

        public static void RunOrderedGroupCallbacks(string group)
        {
            if (!_enabledOrderedGroupCallbacks.ContainsKey(group))
                return;

            foreach (var orderedAction in _enabledOrderedGroupCallbacks[group])
            {
                orderedAction.action.Invoke();
            }
        }

        public static void EnableCallbacksForObject(object objectToActivate, int id)
        {
            CompileCallbackActionsIfNeeded(id, objectToActivate);
            AddTimedActions(id);
            AddOrderedGroupsActions(id);
        }

        public static void DisableCallbacksForObject(int id)
        {
            if (!_callbackActions.ContainsKey(id))
                return;

            _callbackActions[id].IsEnabled = false;

            RemoveTimedActions(id);
            RemoveOrderedGroupActions(id);
        }

        public static void DestroyCallbackForObject(int id)
        {
            if (!_callbackActions.ContainsKey(id))
                return;

            _callbackActions.Remove(id);
        }

        public static CallbackAction[] GetCompiledActions(object objectToCompileActionsFor) =>
            _methodCallbacks.Where(methodCallback => methodCallback.Method.DeclaringType == objectToCompileActionsFor.GetType())
                .Select(methodCallback => new CallbackAction()
                {
                    Action = (Action)methodCallback.Method.CreateDelegate(typeof(Action), objectToCompileActionsFor),
                    OrderedGroupCallbacks = methodCallback.GroupCallbacks,
                    TimedCallbacks = methodCallback.TimedCallbacks
                })
                .ToArray();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadOrderedMethodsInfo()
        {
            var assemblies = GetNonUnityPlayerAssemblies();
            var orderedBehaviourTypes = assemblies.GetOrderedBehaviourTypes();
            _methodCallbacks = orderedBehaviourTypes.GetMethodsCallbacks();
        }

        private static void CompileCallbackActionsIfNeeded(int id, object objectToActivate)
        {
            if (_callbackActions.ContainsKey(id))
            {
                if (_callbackActions[id].IsEnabled)
                    return;
            }
            else
            {
                _callbackActions.Add(id, new CallbackActionsOfType()
                {
                    CallbackActions = GetCompiledActions(objectToActivate),
                    IsEnabled = true
                });
            }
        }

        private static void AddTimedActions(int id)
        {
            var currentTime = Time.unscaledTime;

            var timedActions = _callbackActions[id].CallbackActions
                .Where(callback => callback.TimedCallbacks != null)
                .Select(callback => (callback.Action, id, callback.TimedCallbacks.PrepareTimedCallbacksStartDelay(currentTime)));
            _enabledTimedCallbacks.AddRange(timedActions);
        }

        private static void AddOrderedGroupsActions(int id)
        {
            var updatedGroups = new List<string>();

            var orderedGroupActions = _callbackActions[id].CallbackActions
                .Where(callback => callback.OrderedGroupCallbacks != null)
                .Select(callback => (callback.Action, id, callback.OrderedGroupCallbacks));

            foreach (var groupCallback in orderedGroupActions)
            {
                foreach (var group in groupCallback.OrderedGroupCallbacks)
                {

                    if (!_enabledOrderedGroupCallbacks.ContainsKey(group.GroupId))
                    {
                        _enabledOrderedGroupCallbacks.Add(group.GroupId, new List<(Action action, int instanceId, int order)>()
                        {
                            (groupCallback.Action, id, group.Order)
                        });
                    }
                    else
                    {
                        _enabledOrderedGroupCallbacks[group.GroupId].Add((groupCallback.Action, id, group.Order));
                    }

                    updatedGroups.Add(group.GroupId);
                }
            }

            ResortGroups(updatedGroups);
        }

        private static void ResortGroups(IEnumerable<string> groupsToResort)
        {
            foreach (var group in groupsToResort)
            {
                _enabledOrderedGroupCallbacks[group] = _enabledOrderedGroupCallbacks[group]
                    .OrderBy(action => action.order).ToList();
            }
        }

        private static void RemoveTimedActions(int id)
        {
            var timedActionsToRemove = _enabledTimedCallbacks.RemoveAll(
                timedCallback => timedCallback.instanceId == id);
        }

        private static void RemoveOrderedGroupActions(int id)
        {
            foreach (var orderedGroup in _enabledOrderedGroupCallbacks)
            {
                orderedGroup.Value.RemoveAll(orderedGroupAction => orderedGroupAction.instanceId == id);
            }
        }

        private static TimedCallbackSettings[] PrepareTimedCallbacksStartDelay(this TimedCallbackSettings[] timedCallbackSettings, float currentTime) =>
             timedCallbackSettings.Select(settings => new TimedCallbackSettings()
             {
                 TimeInterval = settings.TimeInterval,
                 StartDelay = settings.StartDelay,
                 LastRun = currentTime + settings.StartDelay
             })
             .ToArray();

        private static Assembly[] GetNonUnityPlayerAssemblies()
        {
            var unityPlayerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player)
                .Select(assembly => assembly.name);

            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => unityPlayerAssemblies.Contains(assembly.GetName().Name))
                .Where(assembly => !_excludedAssemblyKeywords.Any(keyword => assembly.FullName.Contains(keyword)))
                .ToArray();
        }

        private static Type[] GetOrderedBehaviourTypes(this Assembly[] assemblies) =>
            assemblies.SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(OrderedBehaviour).IsAssignableFrom(type))
                .ToArray();

        private static MethodCallback[] GetMethodsCallbacks(this Type[] types) =>
            types.SelectMany(type => type.GetMethodCallbacks())
                .Where(methodCallback => methodCallback != null)
                .ToArray();

        private static MethodCallback[] GetMethodCallbacks(this Type type) =>
             type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(method =>
                {
                    var groupCallbacks = GetOrderedGroupCallbacks(method);
                    var timedCallbacks = GetTimedCallbacks(method);

                    if (groupCallbacks == null && timedCallbacks == null)
                        return null;

                    return new MethodCallback()
                    {
                        Method = method,
                        GroupCallbacks = GetOrderedGroupCallbacks(method),
                        TimedCallbacks = GetTimedCallbacks(method)
                    };
                })
                .ToArray();

        private static OrderedGroupCallbackSettings[] GetOrderedGroupCallbacks(MethodInfo method)
        {
            var orderedGroupAttributes = method.GetCustomAttributes(typeof(OrderedGroupAttribute)).ToArray();
            if (orderedGroupAttributes.Length < 1)
                return null;

            var orderedGroupMethods = new List<OrderedGroupCallbackSettings>();
            foreach (var attribute in orderedGroupAttributes)
            {
                var orderedGroup = attribute as OrderedGroupAttribute;
                orderedGroupMethods.Add(
                    new OrderedGroupCallbackSettings()
                    {
                        GroupId = orderedGroup.GroupId,
                        Order = orderedGroup.Order
                    });
            }

            return orderedGroupMethods.ToArray();
        }

        private static TimedCallbackSettings[] GetTimedCallbacks(MethodInfo method)
        {
            var timedCallbackAttributes = method.GetCustomAttributes(typeof(TimedCallbackAttribute)).ToArray();
            if (timedCallbackAttributes.Length < 1)
                return null;

            var timedMethods = new List<TimedCallbackSettings>();
            foreach (var attribute in timedCallbackAttributes)
            {
                var timedCallback = attribute as TimedCallbackAttribute;
                timedMethods.Add(
                    new TimedCallbackSettings()
                    {
                        TimeInterval = timedCallback.TimeInterval,
                        StartDelay = timedCallback.StartDelay
                    });
            }

            return timedMethods.ToArray();
        }
    }
}
