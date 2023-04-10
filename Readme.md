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

Add your model to the scene and add the component <b>ArmatureBuilder</b>.
* Open the configurator.
* Go to the hip of the rig in the hierachy. Go to Add and "add bone and children". Now all the bones of your rig should be added to the armature.
* But there might be some problems. The direction and length of the bone is calaculated based on the first child of the bone. So when for example the first child of the hip is not the spine, the hip will be wrongly calculated. Let’s fix that:
  * Go to the bone tab, and select the hip.
  * Look at the child names and click “Shift Children” until the spine is the first in the array.
  * To apply that we need to recalculate the forward and length, just hit the 2 buttons at the top.
* This could always accoure when the bone has more then 1 child bone. Just repeat the last step for this area.
* Another point are the hands, here I find it more suitable to set them manually because when you use the IK target object later, the forward and up of the bone will be matched with the forward and up of the IK target object’s transform. So I find it easiest to set:
  * The forward to point in the direction of the hand
  * The up to point to the back of the hand
* For the rest of the bones it is not really necessary to set their up direction, but it should be consistent between armatures.
* To add a chain go back to the Transform tab:
  * Select all transforms that are part of a chain. For example: (upperleg, lowerleg, foot).
  * Go to Add and select Add Chain.
  * Repeat that for every chain in your armature.

* After that go to Chain tab (Here you sould adjust the forward and up directions of the chain):

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
  

When you finished editing the armature you can create a "IKSource" or "IKRig" by clicking the little armature symbol on the top left.

IKSource
---

The IKSource is for creating the IKAnimations.
* Open the Configurator 
* Drag in you Animation Clip
* When the animation has Root motion this will be displayed at the bottom.
* When the Root motion is baked into the hip there will be a selection to extract that.
  * If the position of the hip at the first and last frame does not match the corresponding axis will be selected for extraction.
  + If the rotation around the Y-Axis does not match the angle will be calculated and extracted. (It is advisable to set that manual to 90° if it is close to for example 90°)
* Hit "Create IK Animation"
The IK animation object will be saved in the IKAnimation folder

IKRig
---

The IKRig is the core component for applying IKPoses

* Open the Configurator
* Go to the Animation Tab
* Drag your IK Animation Clips into the list on the left side
* Play back the animation or create a Unity Animation Clips

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
