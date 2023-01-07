using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.IMGUI.Controls;

namespace Rehcub 
{
    [System.Serializable]
    public class ArmatureBuilderWindow : ExtendedEditorWindow
    {
        private static ArmatureBuilderWindow _window;
        [SerializeField] private ArmatureBuilder _builder;

        private ReorderableList reorderableTransform;
        private ReorderableList reorderableBones;
        private ReorderableList reorderableChains;

        // SerializeField is used to ensure the view state is written to the window 
        // layout file. This means that the state survives restarting Unity as long as the window
        // is not closed. If the attribute is omitted then the state is still serialized/deserialized.
        [SerializeField] TreeViewState _treeViewState;

        //The TreeView is not serializable, so it should be reconstructed from the tree data.
        TransformTreeView transformTreeView;

        private bool _showTransforms = true;
        private bool _showBones;
        private bool _showChains;


        private void OnEnable()
        {
            SceneView.duringSceneGui += DuringSceneGUI;

            if (_builder == null)
                return;

            if(_window == null)
                _window = GetWindow<ArmatureBuilderWindow>("Armature Builder");

            _window.serializedObject = new SerializedObject(_builder);
            _window.Init();
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
        }
        private void OnDestroy()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
        }

        private void DuringSceneGUI(SceneView sceneView)
        {
            IKEditorDebug.DrawPose(_builder.bones);

            /*foreach (Chain chain in _builder.chains)
            {
                IKEditorDebug.DrawChain(chain);
            }*/
        }

        public static void ShowConfigurator(ArmatureBuilder builder)
        {
            _window = GetWindow<ArmatureBuilderWindow>("Armature Builder");
            _window.serializedObject = new SerializedObject(builder);
            _window._builder = builder;
            _window.Init();
        }

        public void Init()
        {
            currentProperty = serializedObject.FindProperty("boneTransforms");     
            reorderableTransform = new ReorderableList(serializedObject, currentProperty, true, true, true, true)
            {
                drawElementCallback = DrawTransformListItems,
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Transforms")
            };

            currentProperty = serializedObject.FindProperty("bones");
            reorderableBones = new ReorderableList(serializedObject, currentProperty, true, true, true, true)
            {
                drawElementCallback = DrawBoneListItems,
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Bones"),
                onRemoveCallback = (list) => OnRemoveItem(list)
            };

            currentProperty = serializedObject.FindProperty("chains");
            reorderableChains = new ReorderableList(serializedObject, currentProperty, true, true, true, true)
            {
                drawElementCallback = DrawChainListItems,
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Chains"),
                onRemoveCallback = (list) => OnRemoveItem(list)
            };

            // Check whether there is already a serialized view state (state 
            // that survived assembly reloading)
            if (_treeViewState == null)
                _treeViewState = new TreeViewState();

            transformTreeView = new TransformTreeView(_builder.boneTransforms, _treeViewState);
        }

        private void OnRemoveItem(ReorderableList list)
        {
            selectedProperty = null;
            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
        }

        private void OnGUI()
        {
            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                if (GUILayout.Button("Create Rig"))
                {
                    Selection.activeGameObject = _builder.gameObject;
                    _builder.CreateIKRig();
                    _window.Close();
                    return;
                }
                if (GUILayout.Button("Create Source"))
                {
                    Selection.activeGameObject = _builder.gameObject;
                    _builder.CreateIKSource();
                    _window.Close();
                    return;
                }
            }
            

            Transform selection = Selection.activeTransform;


            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                if (GUILayout.Button("Add Bone"))
                {
                    AddBone(selection);

                    UpdateList();
                }
                if (GUILayout.Button("Add All Bones"))
                {
                    AddAllBones(selection);

                    UpdateList();
                }

                if (GUILayout.Button("Add Chain"))
                {
                    if (transformTreeView.HasSelection())
                    {
                        AddChain(transformTreeView.GetSelection());
                        return;
                    }
                    AddChain(Selection.GetTransforms(SelectionMode.Editable));
                    UpdateList();
                }
            }
            if (GUILayout.Button("Auto Polulate Humanoid Rig"))
            {
                PopulateArmature(selection);
                UpdateList();
            }

            if (GUILayout.Button("Clear All"))
            {
                _builder.boneTransforms.Clear();
                _builder.bones.Clear();
                _builder.chains.Clear();

                UpdateList();
            }

            serializedObject.Update();
            DrawHeaderToolBar();
            DrawBoneList();
            
            serializedObject.ApplyModifiedProperties();

        }

        private void UpdateList()
        {
            if (transformTreeView != null)
                transformTreeView.UpdateList(_builder.boneTransforms);
            Repaint();
        }

        private void DrawHeaderToolBar()
        {
            using (new GUILayout.HorizontalScope("box", GUILayout.ExpandWidth(true)))
            {
                if (GUILayout.Button("Transforms", EditorStyles.toolbarButton))
                {
                    _showTransforms = true;
                    _showBones = false;
                    _showChains = false;
                }
                if (GUILayout.Button("Bones", EditorStyles.toolbarButton))
                {
                    _showTransforms = false;
                    _showBones = true;
                    _showChains = false;
                }
                if (GUILayout.Button("Chains", EditorStyles.toolbarButton))
                {
                    _showTransforms = false;
                    _showBones = false;
                    _showChains = true;
                }
            }
        }

        Vector2 _scrollPosition;
        Rect treeViewRect;
        private void DrawBoneList()
        {
            using (var scrollViewScope = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollViewScope.scrollPosition;
                if (_showTransforms)
                {
                    if (GUILayout.Button("Expand All"))
                        transformTreeView.ExpandAll();
                    
                    if (GUILayout.Button("Collapse All"))
                        transformTreeView.CollapseAll();

                    treeViewRect = GUILayoutUtility.GetRect(_window.position.width, _window.position.height);
                    transformTreeView.OnGUI(treeViewRect);
                    return;
                }


                using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(180), GUILayout.ExpandHeight(true)))
                    {
                        if(_showBones)
                            reorderableBones.DoLayoutList(); 
                        if(_showChains)
                            reorderableChains.DoLayoutList();
                    }

                    using (new GUILayout.VerticalScope("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                    {
                        if (selectedProperty == null)
                            return;

                        EditorGUILayout.PropertyField(selectedProperty);
                    }
                }
            }
        }

        private void AddAllBones(Transform root)
        {
            if (AddTransform(root))
            {
                AddBone(root);
            }

            foreach (Transform transform in root)
            {
                AddAllBones(transform);
            }
        }

        private void AddBone(Transform transform)
        {
            AddTransform(transform);

            string name = transform.name.ToLower();
            Bone bone = new Bone(transform) 
            {
                side = GetSide(name),
                source = GetBone(name)
            };
            _builder.bones.Add(bone);
        }

        private void AddBone(Transform transform, SourceSide sourceSide, SourceBone sourceBone)
        {
            Bone bone = new Bone(transform)
            {
                side = sourceSide,
                source = sourceBone
            };
            _builder.bones.Add(bone);
        }

        private void AddChain(Transform root, int count)
        {
            Transform[] chainTransforms = new Transform[count];

            Bone[] chainBones = new Bone[count];

            for (int i = 0; i < count; i++)
            {
                chainTransforms[i] = root;
                chainBones[i] = new Bone(root);
                root = root.GetChild(0);
            }

            AddTransforms(chainTransforms);
            _builder.bones.AddRange(chainBones);

            Chain chain = new Chain(chainBones);
            chain.ComputeForwardAxis(chainTransforms[0], chainTransforms[1]);
            _builder.chains.Add(chain);
        }

        private void AddChain(Transform[] transforms)
        {
            int count = transforms.Length;

            Bone[] chainBones = new Bone[count];

            for (int i = 0; i < count; i++)
            {
                chainBones[i] = new Bone(transforms[i]);
            }

            AddTransforms(transforms);
            _builder.bones.AddRange(chainBones);

            Chain chain = new Chain(chainBones)
            {
                side = GetSide(name),
                source = GetChain(name)
            };
            chain.ComputeForwardAxis(transforms[0], transforms[1]);
            _builder.chains.Add(chain);
            SetChainBones(chainBones, chain);
        }

        private void AddChain(Transform[] transforms, SourceSide sourceSide, SourceChain sourceChain)
        {
            Bone[] chainBones = new Bone[transforms.Length];

            for (int i = 0; i < transforms.Length; i++)
            {
                chainBones[i] = new Bone(transforms[i]);
            }
            AddTransforms(transforms);
            _builder.bones.AddRange(chainBones);

            Chain chain = new Chain(chainBones)
            {
                side = sourceSide,
                source = sourceChain
            };
            chain.ComputeForwardAxis(transforms[0], transforms[1]);
            _builder.chains.Add(chain);

            SetChainBones(chainBones, chain);
        }

        private void AddChain(IList<int> indecies)
        {
            Bone[] chainBones = new Bone[indecies.Count];
            for (int i = 0; i < indecies.Count; i++)
            {
                chainBones[i] = _builder.bones[indecies[i]];
            }

            string name = chainBones.First().boneName.ToLower();

            Chain chain = new Chain(chainBones)
            {
                side = GetSide(name),
                source = GetChain(name)
            };
            chain.ComputeForwardAxis(_builder.bones[indecies[0]], _builder.bones[indecies[1]]);
            _builder.chains.Add(chain);

            SetChainBones(chainBones, chain);
            /*foreach (Bone bone in chainBones)
            {
                bone.side = chain.side;
                bone.source = SourceBone.CHAIN;
            }*/
        }

        private bool AddTransform(Transform transform)
        {
            if (_builder.boneTransforms == null)
                _builder.boneTransforms = new List<Transform>();

            if (_builder.boneTransforms.Contains(transform))
                return false;

            _builder.boneTransforms.Add(transform);
            return true;
        }

        private void AddTransforms(params Transform[] transforms)
        {
            foreach (Transform transform in transforms)
            {
                AddTransform(transform);
            }
        }

        #region Populate
        public void PopulateArmature(Transform root)
        {
            Transform hip = root;
            AddTransform(hip);
            AddBone(hip, SourceSide.MIDDLE, SourceBone.HIP);

            Transform[] spine = null;

            for (int i = 0; i < hip.childCount; i++)
            {
                Transform child = hip.GetChild(i);

                if (IsBone(child) == false)
                    continue;

                if (IsLeft(child.gameObject.name.ToLower()))
                {
                    Transform[] leftLeg = PopulateChain(child, IsFoot);
                    AddTransforms(leftLeg);
                    AddChain(leftLeg, SourceSide.LEFT, SourceChain.LEG);
                    continue;
                }
                if (IsRight(child.gameObject.name.ToLower()))
                {
                    Transform[] rightLeg = PopulateChain(child, IsFoot);
                    AddTransforms(rightLeg);
                    AddChain(rightLeg, SourceSide.RIGHT, SourceChain.LEG);
                    continue;
                }

                spine = PopulateSpine(child);
                AddTransforms(spine);
                AddChain(spine, SourceSide.MIDDLE, SourceChain.SPINE);
            }

            if (spine == null)
                return;

            Transform neck = null;

            for (int i = 0; i < spine.Last().childCount; i++)
            {
                Transform child = spine.Last().GetChild(i);
                if (IsBone(child) == false)
                    continue;

                string name = child.gameObject.name.ToLower();

                if (IsLeft(name))
                {
                    if (IsShoulder(name))
                    {
                        Transform leftShoulder = child;
                        AddTransforms(leftShoulder);
                        AddBone(leftShoulder, SourceSide.LEFT, SourceBone.SHOULDER);
                        child = leftShoulder.GetChild(0);
                    }
                    Transform[] leftArm = PopulateChain(child, IsHand);
                    AddChain(leftArm, SourceSide.LEFT, SourceChain.ARM);

                    //TODO: Add Fingers!
                    continue;
                }
                if (IsRight(name))
                {
                    if (IsShoulder(name))
                    {
                        Transform rightShoulder = child;
                        AddTransforms(rightShoulder);
                        AddBone(rightShoulder, SourceSide.RIGHT, SourceBone.SHOULDER);
                        child = rightShoulder.GetChild(0);
                    }
                    Transform[] rightArm = PopulateChain(child, IsHand);
                    AddTransforms(rightArm);
                    AddChain(rightArm, SourceSide.RIGHT, SourceChain.ARM);
                    continue;
                }
                neck = child;
            }

            if (neck == null)
                return;

            if (neck.childCount == 0)
            {
                AddTransform(neck);
                AddBone(neck, SourceSide.MIDDLE, SourceBone.HEAD);
            }
            else
            {
                AddTransform(neck);
                AddBone(neck, SourceSide.MIDDLE, SourceBone.NECK);
                AddTransform(neck.GetChild(0));
                AddBone(neck.GetChild(0), SourceSide.MIDDLE, SourceBone.HEAD);
            }

            //leftHand = new Hand(_leftArm.Last());
            //rightHand = new Hand(_rightArm.Last());

        }

        private Transform[] PopulateChain(Transform root, System.Func<string, bool> breakAction)
        {
            List<Transform> chain = new List<Transform>
            {
                root
            };

            for (int i = 0; i < 5; i++)
            {
                Transform child = chain[i].GetChild(0);
                chain.Add(child);
                if (breakAction(child.gameObject.name.ToLower()))
                    break;
            }

            return chain.ToArray();
        }

        private Transform[] PopulateSpine(Transform spineFirst)
        {
            List<Transform> spine = new List<Transform>();
            spine.Add(spineFirst);

            for (int i = 0; i < 5; i++)
            {
                if (spine[i].childCount > 1)
                    break;
                spine.Add(spine[i].GetChild(0));
            }

            return spine.ToArray();
        }

        private bool IsBone(Transform child) => child.gameObject.GetComponents<MonoBehaviour>().Length == 0;
        private bool IsBone(Transform child, string contains) => child.gameObject.name.ToLower().Contains(contains);

        
        private SourceSide GetSide(string name)
        {
            if (IsLeft(name))
                return SourceSide.LEFT;
            if (IsRight(name))
                return SourceSide.RIGHT;
            return SourceSide.MIDDLE;
        }

        private SourceBone GetBone(string name)
        {
            if (IsHead(name))
                return SourceBone.HEAD;
            if (IsNeck(name))
                return SourceBone.NECK;
            if (IsHip(name))
                return SourceBone.HIP;
            if (IsToe(name))
                return SourceBone.TOE;
            if (IsShoulder(name))
                return SourceBone.SHOULDER;

            return SourceBone.NONE;
        }

        private SourceChain GetChain(string name)
        {
            if (IsLeg(name))
                return SourceChain.LEG;
            if (IsArm(name))
                return SourceChain.ARM;
            if (IsSpine(name))
                return SourceChain.SPINE;
            if (IsHand(name))
                return GetFinger(name);

            return SourceChain.NONE;
        }

        private void SetChainBones(Bone[] chainBones, Chain chain)
        {
            foreach (Bone bone in chainBones)
            {
                bone.side = chain.side;
            }
            int start = 0;
            if (chain.source == SourceChain.LEG)
                start = (int)SourceBone.UPPER_LEG;

            if (chain.source == SourceChain.ARM)
                start = (int)SourceBone.UPPER_ARM;

            if (chain.source == SourceChain.SPINE)
                start = (int)SourceBone.SPINE_01;

            if (start == 0)
            {
                for (int i = 0; i < chainBones.Length; i++)
                {
                    Bone bone = chainBones[i];
                    bone.source = SourceBone.NONE;
                }
                return;
            }

            for (int i = 0; i < chainBones.Length; i++)
            {
                Bone bone = chainBones[i];
                bone.source = (SourceBone)(start + i);
            }
        }

        private SourceChain GetFinger(string name)
        {
            if (IsIndexFinger(name))
                return SourceChain.INDEX;
            if (IsMiddleFinger(name))
                return SourceChain.MIDDLE;
            if (IsRingFinger(name))
                return SourceChain.RING;
            if (IsPinkyFinger(name))
                return SourceChain.PINKY;
            if (IsThumb(name))
                return SourceChain.THUMB;

            return SourceChain.NONE;
        }
        private bool IsLeft(string name) => name.Contains("left") || name.Contains(".l");
        private bool IsRight(string name) => name.Contains("right") || name.Contains(".r");

        private bool IsTail(string name) => name.Contains("tail");
        private bool IsFoot(string name) => name.Contains("foot");
        private bool IsToe(string name) => name.Contains("toe");
        private bool IsLeg(string name) => name.Contains("leg");
        private bool IsArm(string name) => name.Contains("arm");
        private bool IsShoulder(string name) => name.Contains("shoulder") || name.Contains("collar");
        private bool IsSpine(string name) => name.Contains("spine") || name.Contains("abdomen");
        private bool IsHand(string name) => name.Contains("hand");
        private bool IsHead(string name) => name.Contains("head");
        private bool IsNeck(string name) => name.Contains("neck");
        private bool IsHip(string name) => name.Contains("hip") || name.Contains("pelvis");
        private bool IsIndexFinger(string name) => name.Contains("index");
        private bool IsMiddleFinger(string name) => name.Contains("middle");
        private bool IsRingFinger(string name) => name.Contains("ring");
        private bool IsPinkyFinger(string name) => name.Contains("pinky");
        private bool IsThumb(string name) => name.Contains("thumb");

        #endregion

        void DrawTransformListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderableTransform.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element);

            element.isExpanded = isActive;
            if (isActive == false)
                return;

            selectedProperty = element;

        }
        void DrawBoneListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderableBones.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty name = element.FindPropertyRelative("boneName");
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), name.stringValue);

            element.isExpanded = isActive;
            if (isActive == false)
                return;

            selectedProperty = element;
        }
        void DrawChainListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderableChains.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty side = element.FindPropertyRelative("side");
            SerializedProperty chain = element.FindPropertyRelative("source");

            string name = side.enumDisplayNames[side.enumValueIndex] + " " + chain.enumDisplayNames[chain.enumValueIndex];
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), name);

            element.isExpanded = isActive;
            if (isActive == false)
                return;

            selectedProperty = element;
        }
    }
}
