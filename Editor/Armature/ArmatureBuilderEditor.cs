using UnityEngine;
using UnityEditor;

namespace Rehcub
{
    [CustomEditor(typeof (ArmatureBuilder))]
    public class ArmatureBuilderEditor : Editor 
    {
        private void OnEnable () 
        {
        	
        }

        public override void OnInspectorGUI () 
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Show Configurator"))
            {
                ArmatureBuilderWindow.ShowConfigurator((ArmatureBuilder)target);
            }
        }
    }
}
