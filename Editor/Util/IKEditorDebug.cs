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

        public static void DrawConstraint(SerializedProperty modProp)
        {
            if (modProp.arraySize <= 0)
                return;

            for (int i = 0; i < modProp.arraySize; i++)
            {
                SerializedProperty constraint = modProp.GetArrayElementAtIndex(i);

                if (constraint != null)
                {
                    SerializedProperty targetProp = constraint.FindPropertyRelative("position");
                    Handles.color = Color.blue;
                    DrawHandle(targetProp);
                    targetProp = constraint.FindPropertyRelative("hint");
                    Handles.color = Color.green;
                    DrawHandle(targetProp);
                }
            }
        }

        public static void DrawConstraint(Armature armature, List<BoneConstraint> mods)
        {
            if (mods.Count <= 0)
                return;

            for (int i = 0; i < mods.Count; i++)
            {
                BoneConstraint constraint = mods[i];

                if (constraint is FixedPosition fixedPosition)
                {
                    Handles.color = Color.blue;

                    Vector3 newPosition = MyHandles.DragHandle(fixedPosition.position, _handleSize, 
                        Handles.SphereHandleCap, Color.yellow, out MyHandles.DragHandleResult result);

                    if (result == MyHandles.DragHandleResult.LMBPress)
                    {
                        for (int j = 0; j < mods.Count; j++)
                        {
                            if (mods[j] is FixedPosition fp)
                                fp.Reset(armature);
                        }
                    }
                    if (result == MyHandles.DragHandleResult.LMBDrag)
                    {
                        fixedPosition.position = newPosition;
                        GUI.changed = true;
                    }
                    if (result == MyHandles.DragHandleResult.LMBRelease)
                    {
                        for (int j = 0; j < mods.Count; j++)
                        {
                            if (mods[j] is FixedPosition fp)
                                fp.Reset(armature);
                        }
                    }
                    Handles.color = Color.green;
                    fixedPosition.hint = DrawHandle(fixedPosition.hint);
                }
            }
        }

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

        public static void DrawCurve(Animation animation, Bone bone, bool root)
        {
            Pose pose = animation.GetFrame(0);
            Bone poseBone = pose[bone];
            Vector3 positionPrev = poseBone.model.position;

            Vector3 root1 = pose.rootTransform.position;
            DrawHandle(positionPrev);
            //Handles.SphereHandleCap(1, positionPrev, Quaternion.identity, _handleSizeSmall, EventType.Repaint);

            for (int i = 1; i < animation.FrameCount; i++)
            {
                pose = animation.GetFrame(i);
                poseBone = pose[bone];
                Bone parentBone = pose[bone.parentName];

                Vector3 rootPos = pose.rootTransform.position;
                Vector3 position = poseBone.model.position;
                if (root)
                    position += rootPos;

                Vector3 newPosition = DrawHandle(position);
                if (root)
                    newPosition -= rootPos;

                pose.SetBoneModel(bone.boneName, newPosition);

                if (parentBone != null)
                {
                    newPosition = parentBone.model - newPosition;
                }

                pose.SetBoneLocal(bone.boneName, newPosition);

                //Handles.SphereHandleCap(1, position, Quaternion.identity, _handleSizeSmall, EventType.Repaint);
                Handles.DrawLine(positionPrev, position);
                positionPrev = position;
            }
        }

        public static void DrawChainCurve(Armature armature, Animation animation, Chain chain, bool root)
        {
            Pose pose = animation.GetFrame(0);
            Bone lastBone = pose[chain.Last()];

            Vector3 positionPrev = lastBone.model.position;
            DrawHandle(positionPrev);

            for (int i = 1; i < animation.FrameCount; i++)
            {
                pose = animation.GetFrame(i);
                Bone firstBone = pose[chain.First()];

                Bone secBone = pose[chain[1]];
                lastBone = pose[chain.Last()];

                Vector3 rootPos = pose.rootTransform.position;
                Vector3 position = lastBone.model.position;
                
                if (root)
                    position += rootPos;


                Vector3 newPosition = DrawHandle(position, out bool changed);
                

                if(changed == false)
                {
                    Handles.DrawLine(positionPrev, position);
                    positionPrev = position;
                    continue;
                }

                Debug.Log("1");
                Debug.Log(position);
                Debug.Log(newPosition);

                if (root)
                    newPosition -= rootPos;

                Vector3 ikDirection = newPosition - firstBone.model.position;
                Vector3 jointDirection = firstBone.model.rotation * chain.alternativeUp;
                Vector3 leftDir = Vector3.Cross(jointDirection.normalized, ikDirection).normalized;
                jointDirection = Vector3.Cross(ikDirection, leftDir).normalized;

                IKChain ikChain = new IKChain(firstBone.model.position, newPosition, jointDirection, chain.length);
                chain.Solve(armature, ikChain, pose);
                
                position = lastBone.model.position;
                Debug.Log("2");
                Debug.Log(position);
                //Handles.SphereHandleCap(1, position, Quaternion.identity, _handleSizeSmall, EventType.Repaint);
                Handles.DrawLine(positionPrev, position);
                positionPrev = position;
            }
        }

        public static void DrawPose(Armature armature, Pose pose)
        {
            foreach (Bone bone in pose.GetBones())
            {
                DrawWireBone(pose, pose[bone.boneName]);
                Handles.color = Color.green;

                Vector3 position = pose.ModelToWorld(bone.model.position);
                //Vector3 position = bone.model.position;

                //Transform transform = armature.GetTransform(bone.boneName);
                //Handles.SphereHandleCap(1, transform.position, Quaternion.identity, _handleSizeSmall, EventType.Repaint);
                //Handles.ArrowHandleCap(1, transform.position, transform.rotation, _handleSize, EventType.Repaint);
                
                Handles.SphereHandleCap(1, position, Quaternion.identity, _handleSizeSmall, EventType.Repaint);

                Bone parentBone = pose[bone.parentName];
                if (parentBone == null)
                    continue;

                Vector3 parentPosition = pose.ModelToWorld(parentBone.model.position);
                //Vector3 parentPosition = parentBone.model.position;
                Handles.DrawLine(parentPosition, position);
            }
        }

        public static void DrawPose(List<Bone> bones)
        {
            foreach (Bone bone in bones)
            {
                //DrawWireBone(pose, pose[bone.boneName]);
                Handles.color = Color.green;

                Vector3 position = bone.model.position;
                //Vector3 position = bone.model.position;

                //Transform transform = armature.GetTransform(bone.boneName);
                //Handles.SphereHandleCap(1, transform.position, Quaternion.identity, _handleSizeSmall, EventType.Repaint);
                //Handles.ArrowHandleCap(1, transform.position, transform.rotation, _handleSize, EventType.Repaint);
                
                Handles.SphereHandleCap(1, position, Quaternion.identity, _handleSizeSmall, EventType.Repaint);

                /*Bone parentBone = pose[bone.parentName];
                if (parentBone == null)
                    continue;

                Vector3 parentPosition = pose.ModelToWorld(parentBone.model.position);
                //Vector3 parentPosition = parentBone.model.position;
                Handles.DrawLine(parentPosition, position);*/
            }
        }

        public static void DrawWireBone(Pose pose, Bone bone)
        {
            Handles.color = Color.red;
            BoneTransform model = pose.ModelToWorld(bone.model);

            Vector3 forward = model.rotation * bone.alternativeUp;
            Vector3 up = model.rotation * bone.alternativeForward;

            Quaternion q = Quaternion.AngleAxis(90, up);

            Vector3 tip = model.position + up * bone.length;
            Vector3 mid = model.position  + up * (bone.length * 0.1f);

            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = mid + forward * (bone.length * 0.1f);
                forward = q * forward;
                Vector3 temp = mid + forward * (bone.length * 0.1f);
                Handles.DrawLine(pos, tip);
                Handles.DrawLine(pos, model.position);
                Handles.DrawLine(pos, temp);

                /*Vector3[] verts = new Vector3[]
                {
                    temp,
                    tip,
                    pos,
                    tip
                };
                Handles.DrawSolidRectangleWithOutline(verts, Color.red, Color.black);
                verts = new Vector3[]
                {
                    temp,
                    model.position,
                    pos,
                    model.position
                };
                Handles.DrawSolidRectangleWithOutline(verts, Color.red, Color.black);*/
            }
        }

        public static void DrawHip(Armature armature, IKPose pose)
        {
            Bone hip = armature.GetBones(SourceBone.HIP)[0];
            Vector3 worldBindPosition = hip.model.position;
            Vector3 worldPosition = armature.currentPose.GetWorldTransform(hip).position;


            Handles.color = Color.green;
            //Handles.DrawLine(worldBindPosition, worldPosition);
            Handles.color = Color.grey;
            Handles.DrawLine(worldBindPosition, worldBindPosition + pose.hip.movement);

            Handles.color = Color.red;
            Handles.SphereHandleCap(0, worldPosition, Quaternion.identity, _handleSizeSmall, EventType.Repaint);

            Handles.color = Color.cyan;
            Handles.DrawLine(worldPosition, pose.hip.direction * _axisSize + worldPosition);
        }

        public static void DrawIKChain(Pose pose, Chain chain, IKChain ikChain)
        {
            float len = chain.length * ikChain.lengthScale;
            Vector3 posA = pose.GetWorldTransform(chain.First()).position;     // Starting Point in Limb
            Vector3 posB = posA + pose.rootTransform.rotation * ikChain.direction.normalized * len;       // Direction + Length to End Effector
            Vector3 posC = posA + pose.rootTransform.rotation * ikChain.jointDirection * _axisSize; // Direction of Joint


            Handles.color = Color.yellow;
            Handles.SphereHandleCap(1, posA, Quaternion.identity, _handleSizeSmall, EventType.Repaint);
            Handles.color = Color.red;
            Handles.SphereHandleCap(1, posB, Quaternion.identity, _handleSizeSmall, EventType.Repaint);

            Handles.color = Color.yellow;
            Handles.DrawDottedLine(posA, posB, HandleUtility.GetHandleSize(posA));
            Handles.color = Color.red;
            Handles.DrawLine(posA, posC);
            /*Handles.color = Color.green;
            Handles.DrawLine(posA, posA + chain.alternativeForward * _axisSize);
            Handles.DrawLine(posA, posA + chain.alternativeUp * _axisSize);*/

            DrawBone(pose, pose[chain.Last().boneName], ikChain.endEffector);
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

        public static void DrawChain(Chain chain, Pose pose)
        {
            Vector3 start = pose[chain.First()].model.position;
            Quaternion r = pose[chain.First()].model.rotation;

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

        public static void DrawChain(SerializedProperty chain)
        {
            SerializedProperty bone = chain.FindPropertyRelative("_bones").GetArrayElementAtIndex(0);
            Vector3 start = bone.FindPropertyRelative("model.position").vector3Value;
            //Vector3 forward = start + chain.alternativeForward *_axisSize;

            SerializedProperty altUpSP = chain.FindPropertyRelative("alternativeUp");
            Vector3 up = start + altUpSP.vector3Value * _axisSize;

            Handles.color = Color.yellow;
            Handles.DrawLine(start, up);
            Vector3 newUp = DrawHandle(up);

            altUpSP.vector3Value = (newUp - start).normalized; 
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
