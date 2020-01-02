# unity-fsm

# What is it?
A lightweight finite state machine implementation that supports custom and conditional transitions

## What are the requirements?
 * Unity 2018.x

## How to use it.

Full example:
```c#
var fsm = new FiniteStateMachine<string>("closed", "open", "locked");
fsm.AddTransition("closed", "open", OPEN_COMMAND)
   .AddTransition("closed", "locked", LOCK_COMMAND, customTransition) // using a custom transition
   .AddTransition("locked", "closed", UNLOCK_COMMAND, () => user.HasKey()) // using a condition
   .AddTransition("open", "closed", CLOSE_COMMAND)
   .OnEnter(open, () => Debug.Log("The door is now open!"))
   .OnExit(closed, HandleDoorIsNoLongerClosed);
```

A finite state machine is composed of `STATES` and `TRANSITIONS`, and is controled by `COMMANDS`

Imagine the example diagram modelling a door:
```
          "CLOSE"               "LOCK"
    +-------->---------+  +-------->---------+
    |                  |  |                  |
 +----+              +------+              +------+
 |OPEN|              |CLOSED|              |LOCKED|
 +----+              +------+              +------+
    |                  |  |                  |
    +--------<---------+  +--------<---------+
           "OPEN"                "UNLOCK"
```

Here we have 3 states, `OPEN`, `CLOSED`, and `LOCKED`, with 4 transitions, controlled by 4 commands `OPEN`, `CLOSE`, `LOCK` and `UNLOCK`.


Now let's break it down
### Step 1: Create it
First let's create the fsm object using it's constructor that takes any number of `STATES`. A state can be any IComparable. In this case we will use strings.
```c#
var fsm = new FiniteStateMachine<string>("closed", "open", "locked");
```

we could also use the static helper `FromEnum`, that will take an enum and use every value as a state
```c#
var fsm =  FiniteStateMachine<DoorStates>.FromEnum();
```

### Step 2: Configure it
Next we can configure the finite state machine's transitions using a fluent interface to add the transitions.

`AddTransition` takes, a from `STATE`, to `STATE` and a `COMMAND` that will trigger it.
It literally reads like, _when in state X, I can move to state Y with the command Z.
It also optionally takes a condition check function or a custom transition for further control

```c#
fsm.AddTransition("closed", "open", OPEN_COMMAND)
   .AddTransition("closed", "locked", LOCK_COMMAND, customTransition) // using a custom transition
   .AddTransition("locked", "closed", UNLOCK_COMMAND, () => user.HasKey()) // using a condition
   .AddTransition("open", "closed", CLOSE_COMMAND);
```

#### Condition functions
The condition function is useful for guarding transitions that depend on state outside the domain of the thing you are modelling.
In this case "does the user have the key" is a peice of state that does not belong to the door, but should control whether or not the door can be unlocked.

#### Custom transitions
Implement your own custom transition by extending the abstract class `Transition<TState>`. Here you can control the transition between the states, such as animating objects, or the timing of the transition.


### Step 3 (optional): Add your own hooks
Optional, but quite likely required will be to add your own hooks. Again using the fluent interface, you can get callbacks when you enter, or exit a specific state, or go between 2 specific states, or just any change
```c#
fsm
    // Called on every state change
    .OnChange((fromState, toState) => { Debug.Log("State has changed from fromState to toState"); });
    // Called on when we go from open to closed
    .OnChange("locked", "open", () => { Debug.Log("The door was unlcocked"); });
    // Called on entry to the specified state
    .OnEnter("open", () => Debug.Log("The door is open everyone!"))
    // Called on exit from the specified state
    .OnExit("closed", HandleDoorIsNoLongerClosed);
```

### Step 4: Begin and start issuing commands

```c#
fsm.Begin(closed);
fsm.IssueCommand(openCommand); // door should now be open
fsm.IssueCommand(lockCommand); // nothing will happen (no transition from open using lock command)


// get the current state
fsm.CurrentState; // will equal "open"
```

## Motivation
The door example above could be modelled quite easily with a class containing 2 boolean values for our states `isLocked` and `isOpen`. 
Then a bunch of functions `Open()` `Close()` `Lock()` `Unlock` for our commands. The problem is the user can then call `Lock()` and `Open()` and the state would look like this:

```c#
isLocked = true;
isOpen = true;
```

This is not a valid state for our door model. We would have to add checks in all those functions. It's not a lot in this case but in other situations it can turn into a lot of over complicated code. Using the FSM, we can configure which transitions are allowed, and it cannot possibly get into an invalid state, because that state doesn't exist!  

To recreate this model we would need checks in all of our command functions and to expose and implement the required event hooks.
There are a lot of situations where we can just replace this with a fsm.

### FAQs
**Can I use this to control my game states?**

This is not a solution for high level so called "game state" management. It can be used to control that but normally those require fully connected graphs. and integration with asset loaded. It's not recommended. The term _state_ is a misnomer in this context anyway.

**Is there an easy way to add commands to go to any state from any state?**
 
No, that would negate the need to have a finite state machine. By using this you are describing how a domain is allowed to change state. If you want to go anywhere from anywhere, you probably just want a single enum variable and the ability to set it.


## Releasing
* Use [gitflow](https://nvie.com/posts/a-successful-git-branching-model/)
* Create a release branch for the release
* On that branch, bump version number in package json file, any other business (docs/readme updates)
* Merge to master via pull request and tag the merge commit on master.
* Merge back to development.

## DUCK

This repo is part of DUCK (dubit unity component kit)
DUCK is a series of repos containing reusable component, utils, systems & tools. 

DUCK packages can be added to a project as git submodules or by using [Unity Package Manager](https://docs.unity3d.com/Manual/upm-git.html). 

