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
            //base.OnInspectorGUI();

            IKRig rig = target as IKRig;

            serializedObject.Update();

            SerializedProperty tPose = serializedObject.FindProperty("_tPose");

            EditorGUILayout.PropertyField(tPose);

            if(GUILayout.Button("Show Configurator"))
            {
                IKRigConfigurator.ShowConfigurator(rig);
            }
            if(GUILayout.Button("Reset Pose to bind Pose"))
            {
                //rig.ResetToTPose();
                rig.ResetToBindPose();
            }
            if(GUILayout.Button("Reset Pose to Clip"))
            {
                rig.ResetToTPose();
            }
            if(GUILayout.Button("Create IK Targets"))
            {
                CreateIKTarget(SourceChain.ARM, SourceSide.LEFT);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateIKTarget(SourceChain sourceChain, SourceSide side)
        {
            GameObject go = new GameObject($"{sourceChain} {side}");

            IKTargetObject target = go.AddComponent<IKTargetObject>();

            IKRig rig = this.target as IKRig;

            Chain chain = rig.Armature.GetChains(sourceChain, side).First();
            Vector3 pos = chain.Last().model.position;
            Vector3 pole = chain.First().model.position + chain.First().model.rotation * chain.alternativeUp;

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(go.transform);
            cube.transform.position = pos;
            cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = pole;
            sphere.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            sphere.transform.SetParent(go.transform);


            SerializedObject serializedObject = new SerializedObject(target);

            serializedObject.FindProperty("_rig").objectReferenceValue = this.target;
            serializedObject.FindProperty("_target").objectReferenceValue = cube.transform;
            serializedObject.FindProperty("_poleTarget").objectReferenceValue = sphere.transform;
            serializedObject.FindProperty("_targetChain").enumValueIndex = (int) sourceChain;
            serializedObject.FindProperty("_targetSide").enumValueIndex = (int) side;
            serializedObject.FindProperty("blend").floatValue = 1f;

            serializedObject.ApplyModifiedProperties();

            target.Init();
        }
    }
}
