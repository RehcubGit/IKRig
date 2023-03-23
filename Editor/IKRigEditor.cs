using UnityEngine;
using UnityEditor;

namespace Rehcub
{
    [CustomEditor(typeof (IKRig))]
    public class IKRigEditor : Editor 
    {
        private void OnEnable ()
        {
            IKRig rig = target as IKRig;
            rig.Init();
        }

        public override void OnInspectorGUI () 
        {
            IKRig rig = target as IKRig;

            serializedObject.Update();

            if(GUILayout.Button("Show Configurator"))
                IKRigConfigurator.ShowConfigurator(rig);

            if(GUILayout.Button("Reset to bind Pose"))
                rig.ResetToBindPose();

            if(GUILayout.Button("Create IK Targets"))
            {
                GameObject targetParent = new GameObject
                {
                    name = "IK Targets"
                };
                targetParent.transform.SetParent(rig.transform);
                IKTargetObject.Create(rig, targetParent, SourceChain.SPINE, SourceSide.MIDDLE);
                IKTargetObject.Create(rig, targetParent, SourceChain.ARM, SourceSide.LEFT);
                IKTargetObject.Create(rig, targetParent, SourceChain.ARM, SourceSide.RIGHT);
                IKTargetObject.Create(rig, targetParent, SourceChain.LEG, SourceSide.LEFT);
                IKTargetObject.Create(rig, targetParent, SourceChain.LEG, SourceSide.RIGHT);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_unityOnPreApplyPose"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
