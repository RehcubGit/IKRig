using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Rehcub 
{
    public class RagdollBuilderWindow : ExtendedEditorWindow
    {

        private static RagdollBuilderWindow _window;


        private RagdollBuilder _builder;
        private Armature _armature;
        private ReorderableList reorderableBones;
        private int _selectedIndex;

        public static void ShowConfigurator(RagdollBuilder builder, Armature armature)
        {
            _window = GetWindow<RagdollBuilderWindow>("Ragdoll Builder");
            _window.serializedObject = new SerializedObject(builder);
            _window._builder = builder;
            _window._armature = armature;
            _window.Init();
            
        }

        public void Init()
        {
            currentProperty = serializedObject.FindProperty("boneInfos");
            reorderableBones = new ReorderableList(serializedObject, currentProperty, true, true, false, true)
            {
                drawElementCallback = DrawBoneListItems,
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Bones"),
                onRemoveCallback = (list) => OnRemoveBoneItem(list)
            };
        }

        private void OnGUI()
        {
            serializedObject.Update();
            if (GUILayout.Button("Test"))
                _builder.BuildStandardHumanRagdoll();
            DrawBoneList();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBoneList()
        {
            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(180), GUILayout.ExpandHeight(true)))
                {
                    reorderableBones.DoLayoutList();
                    Drag();
                }

                using (new GUILayout.VerticalScope("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    if (selectedProperty == null)
                        return;

                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUILayout.PropertyField(selectedProperty, true, GUILayout.ExpandHeight(true));

                        if (check.changed)
                        {
                            serializedObject.ApplyModifiedProperties();
                            RagdollBoneInfo bone = _builder.boneInfos[reorderableBones.index];
                            bone.UpdateComponents();
                        }
                    }

                    if(GUILayout.Button("Mirror Collider"))
                    {
                        RagdollBoneInfo bone = _builder.boneInfos[reorderableBones.index];
                        RagdollBoneInfo mirrorBone = GetMirrorBone(bone);

                        if (mirrorBone == null)
                            return;

                        mirrorBone.radiusScale = bone.radiusScale;
                        mirrorBone.colliderType = bone.colliderType;
                        mirrorBone.UpdateComponents();
                    }

                }
            }
        }

        private RagdollBoneInfo GetMirrorBone(RagdollBoneInfo info)
        {
            RagdollBoneInfo ragdollBoneInfo = _builder.boneInfos.Find((b) => b.bone.side != info.bone.side && b.bone.source == info.bone.source);
            return ragdollBoneInfo;
        }

        void DrawBoneListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderableBones.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty name = element.FindPropertyRelative("name");
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), name.stringValue);

            element.isExpanded = isActive;
            if (isActive == false)
                return;

            selectedProperty = element;
            _selectedIndex = index;
        }

        private void OnRemoveBoneItem(ReorderableList list)
        {
            selectedProperty = null;

            RagdollBoneInfo bone = _builder.boneInfos[list.index];
            _builder.boneInfos.RemoveAt(list.index);
            bone.RemoveRagdoll();

            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
        }

        private void Drag()
        {
            object[] objects = DropZone(100, 100, typeof(Transform));

            if (objects == null)
                return;

            Transform transform = (Transform) objects.First();
            Bone bone = _armature.GetBone(transform.name);

            if (bone == null)
                return;

            Rigidbody parentBody = transform.GetComponentInParent<Rigidbody>();
            RagdollBoneInfo parent = null;
            if (parentBody != null)
                parent = _builder.boneInfos.Find((b) => b.anchor == parentBody.transform);

            RagdollBoneInfo boneInfo = new RagdollBoneInfo(bone, parent, transform);

            if (parent != null)
                parent.children.Add(boneInfo);


            _builder.boneInfos.Add(boneInfo);
        }
    }
}
