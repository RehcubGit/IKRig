IKRig
===
[![Discord](https://discordapp.com/api/guilds/846757180828286976/widget.png?style=shield)](https://discord.gg/B9uceFXMpd)
[![Patreon](https://img.shields.io/badge/Patreon-donate-red?style=flat-square&logo=youtube)](https://www.patreon.com/rehcub)

<b>Retarget your animation to any type of armature.</b><br>
![gif](https://media.giphy.com/media/PrwvR2FPKLPRl7r35W/giphy-downsized-large.gif)

How to install
---

Download Zip or clone GithHub and put  under Assets/IKRig into your project. 

How to use
===

ArmatureBuilder
---

First of all your model should be in T-Pose.
Then you add your model to the scene and add the component ArmatureBuilder.
* Open the configurator.
* Select the root bone (which is preferably your hip bone) of your armature and hit "Add All Bones" this adds the selected root bone and all its children to the armature.
* Select (in the configurator) multiple bones and hit "Add Chain" to group them into a chain.
Make sure you select them in desending order and that the bones are in a parent child relation.
* Go to the "Chain" tab and check if they are assigned the right way.
Here you sould adjust the forward and up directions of the chain.

  * The forward direction is the direction which points from the first bone to the second bone in local space of the first bone.<br>
  So when the y axis of the first bone points to the second bone then the forward direction is (0, 1, 0).

  * The up direction is the direction which points in the direction in which the chain will bend (better known as pole target). <br>
  For example a normal human leg will bend forward so the up direction in this case will be the axis which points forward in the local space of the first bone.

* Select the solver you want for this chain
  * Aim: Points the chain or bone in the target direction and ignores the distance to the target
  * LookAndTwist: Slerps between the target/pole dirction and the direction of the end effector
  * Limb: A Two bone solver (perfect for Human Legs and Arms)
  * Zig-Zag: A Three bone solver
  * Spring: A multi bone procedural solver
  

When you finished editing the armature you can then create a "IKSource" or "IKRig".

IKSource
---

The IKSource is for creating the IKAnimations.
* Open the Configurator 
* Drag in you Animation Clip
* Hit "Create IK Animation"
The IK animation object you now be saved in the IKAnimation folder

IKRig
---

The IKRig is the core component for applying IKPoses

* Open the Configurator
* Go to the Animation Tab
* Drag your IK Animation Clips into the list on the left side
* Play back the animation, add modifiers or export them as Unity Animation Clips

You can also create IK Target Objects, these are for manualy manipulating the ik chains
These work in addition to the ik animations.

PoseAnimator
---
WIP! But the rest works with Mecanim, so for now this is only for testing.

To play an ikAnimation you just need to pass the "IKAnimationData" object to the PoseAnimator

```csharp
private PoseAnimator _animator;
private IKAnimationData _idle;

private void PlayAnimation()
{
    _animator.Play(_idle);
}
```

Future Plans
---

- [ ] Implement extra single pass IK Solvers
- [ ] Implement FABRIK
- [ ] Implement CCD
- [ ] Complete FullBody IK Prototype
- [ ] Bone Constraints
- [ ] Implement Ragdoll
- [ ] Procedural Animation ProtoTyping

License
---
under the MIT Licesne.

External Libarys
* SerializableDictionary -> https://github.com/neuecc/SerializableDictionary
* SerializedPropertyExtensions -> https://github.com/MechWarrior99/Bewildered-Core
