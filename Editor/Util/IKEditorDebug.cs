using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rehcub
{
    public static class IKEditorDebug
    {
        private static float _axisSize = 0.2f;
        private static float _handleSize = 0.04f;
        private static float _handleSizeSmall = 0.01f;

        public static void DrawHandle(SerializedProperty prop)
        {
            if (prop == null)
                return;

            Vector3 position = prop.vector3Value;
            prop.vector3Value = DrawHandle(position);
        }

        public static Vector3 DrawHandle(Vector3 position)
        {
            Vector3 newPosition = MyHandles.DragHandle(position, _handleSize, Handles.SphereHandleCap, Color.yellow, out MyHandles.DragHandleResult result);

            if (result == MyHandles.DragHandleResult.LMBDrag)
            {
                GUI.changed = true;
                return newPosition;
            }
            return position;
        }

        public static Vector3 DrawHandle(Vector3 position, out bool changed)
        {
            Vector3 newPosition = MyHandles.DragHandle(position, _handleSize, Handles.SphereHandleCap, Color.yellow, out MyHandles.DragHandleResult result);

            if (result == MyHandles.DragHandleResult.LMBDrag)
            {
                GUI.changed = true;
                changed = true;
                return newPosition;
            }
            changed = false;
            return position;
        }

        public static void DrawPose(Armature armature, Pose pose)
        {
            foreach (Bone bone in pose.GetBones())
            {
                BoneTransform model = pose.GetModelTransform(bone.boneName);
                Axis axis = new Axis(bone.alternativeForward, bone.alternativeUp);

                DrawWireBone(model, axis, bone.length);
                Handles.color = Color.green;

                Vector3 position = pose.ModelToWorld(bone.model.position);
                
                Handles.SphereHandleCap(1, position, Quaternion.identity, _handleSizeSmall, EventType.Repaint);

                Bone parentBone = pose[bone.parentName];
                if (parentBone == null)
                    continue;

                Vector3 parentPosition = pose.ModelToWorld(parentBone.model.position);
                Handles.DrawLine(parentPosition, position);
            }
        }

        public static void DrawPose(List<Bone> bones)
        {
            foreach (Bone bone in bones)
            {
                Handles.color = Color.green;
                Vector3 position = bone.model.position;                
                Handles.SphereHandleCap(1, position, Quaternion.identity, _handleSizeSmall, EventType.Repaint);
                DrawWireBone(bone.model, new Axis(bone.alternativeForward, bone.alternativeUp), bone.length);
            }
        }

        public static void DrawWireBone(BoneTransform model, Axis axis, float length)
        {
            Handles.color = Color.red;

            Vector3 up = model.rotation * axis.up;
            Vector3 forward = model.rotation * axis.forward;

            Quaternion q = Quaternion.AngleAxis(90, forward);

            Vector3 tip = model.position + forward * length;
            Vector3 mid = model.position + forward * (length * 0.1f);

            for (int i = 0; i < 4; i++)
            {
                Handles.color = Color.red;
                if (i == 0)
                    Handles.color = Color.green;
                Vector3 pos = mid + up * (length * 0.1f);
                up = q * up;
                Vector3 temp = mid + up * (length * 0.1f);
                Handles.DrawLine(pos, tip);
                Handles.DrawLine(pos, model.position);

                Handles.color = Color.red;
                Handles.DrawLine(pos, temp);
            }
        }

        public static void DrawChain(Chain chain)
        {
            Vector3 start = chain.First().model.position;
            Quaternion r = chain.First().model.rotation;

            Vector3 direction = r * chain.alternativeUp;
            direction = start + direction * _axisSize;
            Handles.color = Color.blue;
            Handles.DrawLine(start, direction);

            direction = r * chain.alternativeForward;
            direction = start + direction * _axisSize;
            Handles.color = Color.green;
            Handles.DrawLine(start, direction);
            //DrawHandle(up);
        }

        public static void DrawBone(Pose pose, Bone bone, IKBone ikBone)
        {
            Vector3 position = pose.GetWorldTransform(bone).position;

            Handles.color = Color.blue;
            Handles.DrawLine(position, position + ikBone.direction * _axisSize);
            Handles.color = Color.green;
            Handles.DrawLine(position, position + ikBone.twist * _axisSize);
        }
    }
}
