# UnityOrderedAndTimedBehaviours
This contains a simple system for Unity game engine that introduces two attributes:
- TimedCallbackAttribute - applied to method, will call target method at specified time intervals, starting from the moment when Component that contains the method is enabled. It also allows for start delay - this is usefull if you want your method to not start executing with first update loop or to not execute in the same update loop as other method with the same time interval.
- OrderedGroupAttribute - applied to method, by default you can choose from 3 groups `FirstStart`, `SecondStart` and `ThirdStart`. Target method will be called at from Start method in defined order. So if you have 3 behaviours and each have methods with `FirstStart` attribute, they will be called by the order parameter. Lowest order - called first. Default groups are called in this order: FirstStart, SecondStart, ThirdStart.
To use this system make sure to have `GameObject` with `OrderedBehaviorManager` component in the Scene (only 1 game object in the Scene should have it)
To use Timed Callbacks:
- Make sure you inherit from `OrderedBehaviour` instead of `MonoBehaviour`.
- Add attribute `TimedCallback` to your method that you want to be ran.
  - Pass time interval you want your method to be ran at.
  - Pass start delay.
  - Example attribute: `[TimedCallback(timeInterval: 2.5f, startDelay: 1f)]` - this will make your method run every 2.5 seconds, starting 1 second after component enabling (OnEnable call) or at earliest update (if gap between enabling and update is larger than start delay).
To use Ordered Callbacks for start methods:
- Make sure you inherit from `OrderedBehaviour` instead of `MonoBehaviour`.
- Add attribute `OrderedGroup` to your method that you want to be ran.
  - Pass group id - by default ether of: `CallbackGroup.FirstStart`, `CallbackGroup.SecondStart`, `CallbackGroup.ThirdStart`.
  - Pass order - lower order method is called first. Order is taken into account per group (meaning FirstStart group methods are always called before SecondGroup methods even if SecondGroup method has lower order).
  - Example `[OrderedGroup(groupId: CallbackGroup.FirstStart, order: 0)]` - this will make your method run in FirstStart group with lowest order.

One method may have multiple attributes, this will cause it to be called multiple times.

Adding new groups:
`OrderedGroup` attribute can take any string as `groupId` parameter. So, as an example, if you want to have ordered callbacks for update method you could add attribute like this:
`[OrderedGroup(groupId: "MyUpdateCallback", order: 0)]`.
This, however, will do nothing until you run the group callbacks. To do that you can ether create your own callbacks manager or modify `OrderedBehaviourManager`.
So lets say you want `MyUpdateCallback` to be ran every update, you would simply need to call `RunOrderedGroupCallbacks` to Update method of `OrderedBehaviourManager` class:
`CallbacksManager.RunOrderedGroupCallbacks("MyUpdateCallback");`.Doing this will run all methods in `MyUpdateCallback` group using the provided order every frame.
You can further modify this behaviour to run lets say every n-th frame, or call them multiple times per frame etc...

Note:
I would suggest to always make sure that you have only one instance of callbacks manager as multiple instances will cause all methods to be called multiple times.
When creating your own groups: be careful where you put your `RunOrderedGroupCallbacks` calls. Calling group from `Awake` or `OnEnable` may be unpredictable as Unity may not have loeaded all the components yet. In this case - only the loaded ones will receive callback. This is unpredictable as unity may load components in a bit of a random order.
Also be careful to not call from methods like OnDissable, OnDestroy, OnApplicationQuit and so on.
Generally safe methods to use `RunOrderedGroupCallbacks` are: `Start`, `Update`, `FixedUpdate`, `LateUpdate`.
Currently there is no way to pass methods to callbacks.

If you have any questions feel free to ask.
