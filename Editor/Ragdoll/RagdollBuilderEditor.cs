using UnityEngine;
using UnityEditor;

namespace Rehcub
{
    [CustomEditor(typeof (RagdollBuilder))]
    public class RagdollBuilderEditor : Editor 
    {
        private void OnEnable () 
        {
        	
        }

        public override void OnInspectorGUI () 
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Show Configurator"))
            {
                RagdollBuilder builder = (RagdollBuilder) target;
                Armature armature = builder.GetComponent<IKRig>().Armature;
                RagdollBuilderWindow.ShowConfigurator(builder, armature);
            }
        }
    }
}
