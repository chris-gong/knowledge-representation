NPC Framework Documentation

Version: 0.1.dev

General Description
-------------------

This is a brief introduction to the NPC Framework.

The NPC framework is a controller-centralized set of components to manage every aspect of humanoid agents within games and simulations in Unity. These characters can be player controlled or not. Some of these aspects are: perception radius, IK, path finding, steering, obstacles detection, social forces, behaviors and animations, among other agent services. The most important aspect of the framework is its capacity to be fully extendable and customizable based on anyone's needs. These can be done in two simple ways:

1. Parameterizing the basic built in services.
2. Via the implementation of plug n' play modules using provided interfaces.

Finally, the framework provides many extendable components, such as an Animator Controller, which can also be fully extended based on development needs. 

On a higher level, the implemention is mainly divided the following way:

    NPCController       - single point of interface for all the agent's components.
        NPCPerception
        NPCBody
        NPCAI

    .. modules and subcomponents

Please see the documentation for usage, examples and details.