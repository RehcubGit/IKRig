using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rehcub
{
    public enum ColliderType { Capsule, Box, Sphere }

    [System.Serializable]
    public class RagdollBoneInfo
    {
        [ReadOnly]
        public string name;

        [ReadOnly]
        public Bone bone;
        [ReadOnly]
        public Transform anchor;
        [ReadOnly]
        public RagdollBoneInfo parent;
        [ReadOnly]
        public List<RagdollBoneInfo> children;

        [ReadOnly]
        public Rigidbody body;
        [ReadOnly]
        public Collider collider;
        [ReadOnly]
        public ConfigurableJoint joint;

        [Range(0, -180)]
        public float lowXLimit;
        [Range(0, 180)]
        public float highXLimit;
        [Range(0, 180)]
        public float yLimit;
        [Range(0, 180)]
        public float zLimit;

        public Vector3 axis;
        public Vector3 normalAxis;

        public float radiusScale;
        public ColliderType colliderType;

        //public ArrayList children = new ArrayList();
        public float density;
        public float summedMass;// The mass of this and all children bodies

        public RagdollBoneInfo(Transform anchor)
        {
            this.anchor = anchor;

            children = new List<RagdollBoneInfo>();

            body = Undo.AddComponent<Rigidbody>(anchor.gameObject);
            body.useGravity = false;
        }

        public RagdollBoneInfo(Bone bone, RagdollBoneInfo parent, Transform anchor)
        {
            this.bone = bone;
            this.name = bone.boneName;
            this.anchor = anchor;

            this.parent = parent;
            children = new List<RagdollBoneInfo>();


            body = Undo.AddComponent<Rigidbody>(anchor.gameObject);
            if(parent != null)
                joint = Undo.AddComponent<ConfigurableJoint>(anchor.gameObject);
            colliderType = ColliderType.Capsule;
            collider = Undo.AddComponent<CapsuleCollider>(anchor.gameObject);


            Axis axis = new Axis(bone.alternativeForward, bone.alternativeUp);

            this.axis = axis.right;
            normalAxis = axis.forward;

        }

        public void RemoveRagdoll()
        {
            Undo.DestroyObjectImmediate(joint);
            Undo.DestroyObjectImmediate(body);
            Undo.DestroyObjectImmediate(collider);
        }

        public void UpdateComponents()
        {
            body.mass = density;
            UpdateCapsule();
            UpdateBox();
            UpdateSphereCollider();

            UpdateJoint();

        }

        public T GetCollider<T>() where T : Collider
        {
            T collider = this.collider as T;
            if (collider != null)
                return collider;

            UnityEngine.Object.DestroyImmediate(this.collider);
            collider = anchor.gameObject.AddComponent<T>();
            this.collider = collider;
            return collider;
        }

        private void UpdateCapsule()
        {
            if (colliderType != ColliderType.Capsule)
                return;

            CapsuleCollider capsuleCollider = GetCollider<CapsuleCollider>();

            normalAxis.CalculateDirection(out int direction, out float distance);
            capsuleCollider.direction = direction;

            distance = bone.length;
            Vector3 center = Vector3.zero;
            center[direction] = distance * 0.5f;
            capsuleCollider.center = center;
            capsuleCollider.height = Mathf.Abs(distance);
            capsuleCollider.radius = Mathf.Abs(distance * radiusScale);
        }

        private void UpdateBox()
        {
            if (colliderType != ColliderType.Box)
                return;

            BoxCollider collider = GetCollider<BoxCollider>();


            BoneTransform hip = bone.model;
            Bounds bounds = new Bounds();

            foreach (RagdollBoneInfo child in children)
            {
                BoneTransform childTransform = child.bone.model; 
                bounds.Encapsulate((hip - childTransform).position);
            }

            Vector3 size = bounds.size;
            size[bounds.size.SmallestComponent()] = size[bounds.size.LargestComponent()] * radiusScale;

            collider.center = bounds.center;
            collider.size = size;
        }

        private void UpdateSphereCollider()
        {
            if (colliderType != ColliderType.Sphere)
                return;

            SphereCollider sphere = GetCollider<SphereCollider>();
           
            normalAxis.CalculateDirection(out int direction, out float distance);

            distance = bone.length;
            Vector3 center = Vector3.zero;
            center[direction] = distance * 0.5f;
            sphere.center = center;
            sphere.radius = radiusScale;
        }

        private void UpdateJoint()
        {
            if (joint == null)
                return;


            joint.axis = axis;
            joint.secondaryAxis = normalAxis;
            joint.anchor = Vector3.zero;
            if(parent != null)
                joint.connectedBody = parent.body;
            joint.enablePreprocessing = false; // turn off to handle degenerated scenarios, like spawning inside geometry.

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            if(lowXLimit == 0 && highXLimit == 0 && yLimit == 0 && zLimit == 0)
            {
                joint.angularXMotion = ConfigurableJointMotion.Free;
                joint.angularYMotion = ConfigurableJointMotion.Free;
                joint.angularZMotion = ConfigurableJointMotion.Free;

                return;
            }

            SoftJointLimit limit = new SoftJointLimit();
            limit.contactDistance = 0; // default to zero, which automatically sets contact distance.

            limit.limit = lowXLimit;
            joint.lowAngularXLimit = limit;

            limit.limit = highXLimit;
            joint.highAngularXLimit = limit;

            limit.limit = yLimit;
            joint.angularYLimit = limit;

            limit.limit = zLimit;
            joint.angularZLimit = limit;

            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

        }
    }

    public class RagdollBuilder : MonoBehaviour
    {
        private Transform _root;
        private Armature _armature;

        public RagdollBoneInfo root;
        public List<RagdollBoneInfo> boneInfos;

        public float totalMass = 20;
        public float strength = 0.0f;

        private bool createSeparateObjects = false;

        private void Start()
        {
            Ragdoll ragdoll = GetComponent<Ragdoll>();
            ragdoll.CreateBoneMatcher(boneInfos);
        }

        public void Init()
        {
            _root = transform;
            _armature = GetComponent<IKRig>().Armature;
            boneInfos = new List<RagdollBoneInfo>();
        }

        public void BuildBones()
        {
            foreach (RagdollBoneInfo boneInfo in boneInfos)
            {
                Bone bone = _armature.GetBone(boneInfo.name);
                Bone parent = _armature.GetBone(boneInfo.parent.name);

                BuildBone(bone, parent, boneInfo.radiusScale, boneInfo.summedMass, boneInfo.lowXLimit, boneInfo.highXLimit, boneInfo.yLimit, boneInfo.zLimit);
            }
        }

        public void BuildStandardHumanRagdoll()
        {
            Bone pelvis = _armature.GetBones(SourceBone.HIP)[0];

            /*Transform anchor = _armature.GetTransform(pelvis.boneName);
            root = new RagdollBoneInfo(anchor.parent);*/
            BuildBone(pelvis, 0.25f, 2.5f, 0, 0, 0, 0);

            Chain spine = _armature.GetChains(SourceChain.SPINE)[0];
            BuildBone(spine.Last(), 1f, 2.5f, -20f, 20f, 10f, 5f);


            Chain leftLeg = _armature.GetChains(SourceChain.LEG, SourceSide.LEFT)[0]; 
            Chain rightLeg = _armature.GetChains(SourceChain.LEG, SourceSide.RIGHT)[0];

            BuildBone(leftLeg.First(), 0.3f, 1.5f, -110f, 30, 45, 30);
            BuildBone(rightLeg.First(), 0.3f, 1.5f, -110f, 30, 45, 30);

            BuildBone(leftLeg[1], 0.25f, 1.5f, -150f, 5f, 0, 0);
            BuildBone(rightLeg[1], 0.25f, 1.5f, -150f, 5f, 0, 0);

            BuildBone(leftLeg.Last(), 0.25f, 1.5f, -80f, 0, 0, 0);
            BuildBone(rightLeg.Last(), 0.25f, 1.5f, -80f, 0, 0, 0);


            Chain leftArm = _armature.GetChains(SourceChain.ARM, SourceSide.LEFT)[0];
            Chain rightArm = _armature.GetChains(SourceChain.ARM, SourceSide.RIGHT)[0];

            BuildBone(leftArm.First(), 0.25f, 1.0f, -90f, 90, 90, 90);
            BuildBone(rightArm.First(), 0.25f, 1.0f, -90f, 90, 90, 90);

            BuildBone(leftArm[1], 0.20f, 1.0f, -90f, 0, 0, 0);
            BuildBone(rightArm[1], 0.20f, 1.0f, -90f, 0, 0, 0);

            BuildBone(leftArm.Last(), 0.20f, 1.0f, -90f, 0, 0, 0);
            BuildBone(rightArm.Last(), 0.20f, 1.0f, -90f, 0, 0, 0);


            Bone head = _armature.GetBones(SourceBone.HEAD)[0]; 
            BuildBone(head, 1.0f, 1.0f, -40f, 25f, 25f, 0);

        }


        /*public void BuildBones()
        {
            BuildBone(pelvis, _root, 0.25f, 2.5f);

            // x 110 -130 / 30  y 40 / 45  z 30 - 45 / 20-30 
            BuildBone(leftHips, pelvis, 0.3f, 1.5f, -110f, 30, 45, 30);
            BuildBone(rightHips, pelvis, 0.3f, 1.5f, -110f, 30, 45, 30);
            
            // x 5 / 150  y 0  z 0
            BuildBone(leftKnee, leftHips, 0.25f, 1.5f, -150f, 5f, 0, 0);
            BuildBone(rightKnee, rightHips, 0.25f, 1.5f, -150f, 5f, 0, 0);

            // x 45 / 60  y   z 30 / 20
            //BuildBone(leftFoot, leftKnee, 0.25f, 1.5f, -80f, 0, 0, 0);
            //BuildBone(rightFoot, rightKnee, 0.25f, 1.5f, -80f, 0, 0, 0);
            
            // x 180 / 60  y 130 / 45  z 180 / 135
            BuildBone(leftArm, pelvis, 0.25f, 1.0f, -90f, 90, 90, 90);
            BuildBone(rightArm, pelvis, 0.25f, 1.0f, -90f, 90, 90, 90);

            // x 0  y 5 / 150  z 0
            BuildBone(leftElbow, leftArm, 0.20f, 1.0f, -90f, 0, 0, 0);
            BuildBone(rightElbow, rightArm, 0.20f, 1.0f, -90f, 0, 0, 0);

            // x 150 / 45  y 20 / 30-50  z 70 / 80-90
            //BuildBone(leftHand, leftElbow, 0.20f, 1.0f, -90f, 0, 0, 0);
            //BuildBone(rightHand, rightElbow, 0.20f, 1.0f, -90f, 0, 0, 0);

            // x 55 / 70 - 90 (with neck)  y 70 / 70  z 35 / 35
            BuildBone(head, pelvis, 1.0f, 1.0f, -40f, 25f, 25f, 0);
        }*/

        

        private void BuildBone(Bone bone, float radiusScale, float mass, float lowXLimit, float highXLimit, float yLimit, float zLimit)
        {
            Transform anchor = _armature.GetTransform(bone.boneName);
            Rigidbody parentBody = anchor.GetComponentInParent<Rigidbody>();
            RagdollBoneInfo parent = null;
            if (parentBody != null)
                parent = boneInfos.Find((b) => b.anchor == parentBody.transform);

            /*if (parent == null)
                parent = root;*/

            RagdollBoneInfo boneInfo = new RagdollBoneInfo(bone, parent, anchor)
            {
                radiusScale = radiusScale,
                density = mass,

                lowXLimit = lowXLimit,
                highXLimit = highXLimit,
                yLimit = yLimit,
                zLimit = zLimit
            };

            boneInfo.UpdateComponents();

            if (parent != null)
                parent.children.Add(boneInfo);

            boneInfos.Add(boneInfo);
        }

        private void BuildBone(Bone bone, Bone parent, float radiusScale, float mass, float lowXLimit, float highXLimit, float yLimit, float zLimit)
        {
            BuildBody(bone, mass);

            if (parent == null)
            {
                BuildTorsoCollider(bone);
                return;
            }

            BuildCapsule(bone, radiusScale);
            BuildJoint(bone, parent, lowXLimit, highXLimit, yLimit, zLimit);
        }

        private void BuildBone(Bone bone, Bone parent, float radiusScale, float mass)
        {
            BuildBody(bone, mass);

            if (parent == null)
            {
                BuildTorsoCollider(bone);
                return;
            }

            BuildCapsule(bone, radiusScale);
            BuildJoint(bone, parent);
        }

        private void BuildBone(Bone bone, Transform parent, float radiusScale, float mass)
        {
            BuildBody(bone, mass);

            if (parent == null)
            {
                BuildTorsoCollider(bone);
                return;
            }

            BuildCapsule(bone, radiusScale);
            BuildJoint(bone, parent);
        }

        private void BuildCapsule(Bone bone, float radiusScale)
        {
            Transform anchor = _armature.GetTransform(bone.boneName);

            if (anchor.TryGetComponent<Collider>(out _))
                return;

            CapsuleCollider collider = Undo.AddComponent<CapsuleCollider>(anchor.gameObject);

            CalculateDirection(bone.alternativeForward, out int direction, out float distance);
            collider.direction = direction;

            Vector3 center = Vector3.zero;
            center[direction] = distance * 0.5F;
            collider.center = center;
            collider.height = Mathf.Abs(distance);
            collider.radius = Mathf.Abs(distance * radiusScale);
        }

        private void BuildTorsoCollider(Bone bone)
        {
            Transform anchor = _armature.GetTransform(bone.boneName);

            if (anchor.TryGetComponent<Collider>(out _))
                return;

            BoneTransform hip = _armature.bindPose.GetModelTransform(bone);/*
            BoneTransform leftArm = _armature.bindPose.GetModelTransform(this.leftArm);
            BoneTransform rightArm = _armature.bindPose.GetModelTransform(this.rightArm);
            BoneTransform leftLeg = _armature.bindPose.GetModelTransform(this.leftHips);
            BoneTransform rightLeg = _armature.bindPose.GetModelTransform(this.rightHips);*/

            Bounds bounds = new Bounds();
            /*bounds.Encapsulate((hip - leftLeg).position);
            bounds.Encapsulate((hip - rightLeg).position);
            bounds.Encapsulate((hip - leftArm).position);
            bounds.Encapsulate((hip - rightArm).position);*/

            Vector3 size = bounds.size;
            size[SmallestComponent(bounds.size)] = size[LargestComponent(bounds.size)] / 2.0F;

            BoxCollider collider = Undo.AddComponent<BoxCollider>(anchor.gameObject);
            collider.center = bounds.center;
            collider.size = size;
        }

        private void BuildBody(Bone bone, float mass)
        {
            Transform anchor = _armature.GetTransform(bone.boneName);

            if(anchor.TryGetComponent(out Rigidbody rigidbody) == false)
            {
                rigidbody = Undo.AddComponent<Rigidbody>(anchor.gameObject);
            }

            rigidbody.mass = mass;
        }

        private void BuildBody(Transform anchor, float mass)
        {
            if(anchor.TryGetComponent(out Rigidbody rigidbody) == false)
            {
                rigidbody = Undo.AddComponent<Rigidbody>(anchor.gameObject);
            }

            rigidbody.mass = mass;
        }



        private void BuildJoint(Bone bone, Bone parent, float lowXLimit, float highXLimit, float yLimit, float zLimit)
        {
            Transform anchor = _armature.GetTransform(bone.boneName);
            Transform parentAnchor = _armature.GetTransform(parent.boneName);

            if (anchor.TryGetComponent(out ConfigurableJoint joint) == false)
            {
                joint = Undo.AddComponent<ConfigurableJoint>(anchor.gameObject);
            }

            // Setup connection and axis

            Axis axis = new Axis(bone.alternativeForward, bone.alternativeUp);

            joint.axis = CalculateDirectionAxis(axis.right);
            Debug.Log($"{bone.boneName} : {axis.right} {bone.alternativeUp} - {joint.axis}");
            joint.secondaryAxis = CalculateDirectionAxis(axis.forward);
            joint.anchor = Vector3.zero;
            joint.connectedBody = parentAnchor.GetComponent<Rigidbody>();
            joint.enablePreprocessing = false; // turn off to handle degenerated scenarios, like spawning inside geometry.

            // Setup limits
            SoftJointLimit limit = new SoftJointLimit();
            limit.contactDistance = 0; // default to zero, which automatically sets contact distance.

            limit.limit = lowXLimit;
            joint.lowAngularXLimit = limit;

            limit.limit = highXLimit;
            joint.highAngularXLimit = limit;

            limit.limit = yLimit;
            joint.angularYLimit = limit;

            limit.limit = zLimit;
            joint.angularZLimit = limit;

            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
        }

        private void BuildJoint(Bone bone, Bone parent)
        {
            Transform anchor = _armature.GetTransform(bone.boneName);
            Transform parentAnchor = _armature.GetTransform(parent.boneName);

            if (anchor.TryGetComponent(out ConfigurableJoint joint) == false)
            {
                joint = Undo.AddComponent<ConfigurableJoint>(anchor.gameObject);
            }

            // Setup connection and axis

            Axis axis = new Axis(bone.alternativeForward, bone.alternativeUp);

            joint.axis = CalculateDirectionAxis(axis.right);
            joint.secondaryAxis = CalculateDirectionAxis(axis.forward);
            joint.anchor = Vector3.zero;
            joint.connectedBody = parentAnchor.GetComponent<Rigidbody>();
            joint.enablePreprocessing = false; // turn off to handle degenerated scenarios, like spawning inside geometry.

            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
        }

        private void BuildJoint(Bone bone, Transform parent)
        {
            Transform anchor = _armature.GetTransform(bone.boneName);

            if (anchor.TryGetComponent(out ConfigurableJoint joint) == false)
            {
                joint = Undo.AddComponent<ConfigurableJoint>(anchor.gameObject);
            }

            // Setup connection and axis

            Axis axis = new Axis(bone.alternativeForward, bone.alternativeUp);

            joint.axis = CalculateDirectionAxis(axis.right);
            joint.secondaryAxis = CalculateDirectionAxis(axis.forward);
            joint.anchor = Vector3.zero;
            joint.connectedBody = parent.GetComponent<Rigidbody>();
            joint.enablePreprocessing = false; // turn off to handle degenerated scenarios, like spawning inside geometry.

            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;

            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
        }

        public static void RemoveRagdoll(Transform transform)
        {
            RemoveComponents(transform);
            Undo.DestroyObjectImmediate(transform.GetComponent(typeof(RagdollBuilder)));
        }

        private static void RemoveComponents(Transform anchor)
        {
            Component[] joints = anchor.GetComponentsInChildren(typeof(Joint));
            foreach (Joint joint in joints)
                Undo.DestroyObjectImmediate(joint);

            Component[] bodies = anchor.GetComponentsInChildren(typeof(Rigidbody));
            foreach (Rigidbody body in bodies)
                Undo.DestroyObjectImmediate(body);

            Component[] colliders = anchor.GetComponentsInChildren(typeof(Collider));
            foreach (Collider collider in colliders)
                Undo.DestroyObjectImmediate(collider);
        }

        private static void CalculateDirection(Vector3 point, out int direction, out float distance)
        {
            // Calculate longest axis
            direction = LargestComponent(point);
            distance = point[direction];
        }

        private static Vector3 CalculateDirectionAxis(Vector3 point)
        {
            CalculateDirection(point, out int direction, out float distance);
            Vector3 axis = Vector3.zero;
            if (distance > 0)
                axis[direction] = 1.0f;
            else
                axis[direction] = -1.0f;
            return axis;
        }

        private static int SmallestComponent(Vector3 point)
        {
            int direction = 0;
            if (Mathf.Abs(point[1]) < Mathf.Abs(point[0]))
                direction = 1;
            if (Mathf.Abs(point[2]) < Mathf.Abs(point[direction]))
                direction = 2;
            return direction;
        }

        private static int LargestComponent(Vector3 point)
        {
            int direction = 0;
            if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
                direction = 1;
            if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
                direction = 2;
            return direction;
        }
    }
}
