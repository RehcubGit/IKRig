using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.IMGUI.Controls;
using Bewildered.Editor;

namespace Rehcub 
{
    [System.Serializable]
    public class ArmatureBuilderWindow : ExtendedEditorWindow
    {
        private static ArmatureBuilderWindow _window;
        [SerializeField] private ArmatureBuilder _builder;

        private ReorderableList reorderableBones;
        private ReorderableList reorderableChains;

        // SerializeField is used to ensure the view state is written to the window 
        // layout file. This means that the state survives restarting Unity as long as the window
        // is not closed. If the attribute is omitted then the state is still serialized/deserialized.
        [SerializeField] TreeViewState _treeViewState;

        //The TreeView is not serializable, so it should be reconstructed from the tree data.
        TransformTreeView transformTreeView;

        NotificationBar notificationBar;

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
            if (_showChains)
            {
                foreach (Chain chain in _builder.chains)
                {
                    IKEditorDebug.DrawChain(chain);
                }
                return;
            }

            IKEditorDebug.DrawPose(_builder.bones);
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
            notificationBar = new NotificationBar();

            if (reorderableBones == null)
            {
                currentProperty = serializedObject.FindProperty("bones");
                reorderableBones = new ReorderableList(serializedObject, currentProperty, true, true, true, true)
                {
                    drawElementCallback = DrawBoneListItems,
                    drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Bones"),
                    onRemoveCallback = (list) => OnRemoveBoneItem(list)
                };
            }

            if(reorderableChains == null)
            {
                currentProperty = serializedObject.FindProperty("chains");
                reorderableChains = new ReorderableList(serializedObject, currentProperty, true, true, true, true)
                {
                    drawElementCallback = DrawChainListItems,
                    drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Chains"),
                    //onCanAddCallback = (list) => true,
                    onAddCallback = (list) => {
                        list.serializedProperty.arraySize++;
                        serializedObject.ApplyModifiedProperties();
                    },
                    onRemoveCallback = (list) => OnRemoveChainItem(list)
                };
            }


            // Check whether there is already a serialized view state (state 
            // that survived assembly reloading)
            if (_treeViewState == null)
                _treeViewState = new TreeViewState();

            transformTreeView = new TransformTreeView(_builder.boneTransforms, _treeViewState);
        }

        private void OnGUI()
        {
            serializedObject.Update();

            DrawHeaderToolBar();

            GUILayout.Space(5f);

            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)))
            {
                DrawCreateRigOrSourcePopup();

                if (_showTransforms)
                {
                    DrawAddPopup();

                    /*if (GUILayout.Button("Auto Polulate Humanoid Rig"))
                    {
                        PopulateArmature(selection);
                        UpdateList();
                    }*/

                }

                if (_showBones)
                {
                    BoneControls();
                }
                if (_showChains)
                {
                    ChainControls();
                }
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Clear All", EditorStyles.toolbarButton))
                {
                    _builder.boneTransforms.Clear();
                    _builder.bones.Clear();
                    _builder.chains.Clear();

                    selectedProperty = null;
                    UpdateList();
                }
            }

            GUILayout.Space(5f);

            DrawBoneList();

            notificationBar.Draw();
        }

        private void DrawCreateRigOrSourcePopup()
        {
            bool buttonResult = GUILayout.Button(EditorGUIUtility.IconContent("d_Avatar Icon"), 
                EditorStyles.toolbarPopup, 
                GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));

            if (Event.current.type == EventType.Repaint)
                popupRect2 = GUILayoutUtility.GetLastRect();

            popupRect2.width = 200f;

            if (buttonResult)
            {
                string[] options = { "Create Rig", "Create Source" };
                void callback(int i)
                {
                    if (reorderableBones.count <= 0)
                    {
                        Debug.Log("There are no bones in the Armature.");
                        notificationBar.Push("There are no bones in the Armature.");
                        return;
                    }
                    switch (i)
                    {
                        case 0:
                            Selection.activeGameObject = _builder.gameObject;
                            _builder.CreateIKRig();
                            _window.Close();
                            break;

                        case 1:
                            Selection.activeGameObject = _builder.gameObject;
                            _builder.CreateIKSource();
                            _window.Close();
                            break;

                        default:
                            break;
                    }
                }
                PopupWindow.Show(popupRect2, Popup.Create(popupRect2, options, callback));
            }
        }

        private bool DrawAddPopup()
        {
            bool buttonResult;
            Transform selection = Selection.activeTransform;

            //buttonResult = GUILayout.Button(EditorGUIUtility.IconContent("d_Toolbar Plus More@2x"), GUILayout.MaxWidth(EditorGUIUtility.singleLineHeight * 1.5f));
            buttonResult = GUILayout.Button(EditorGUIUtility.TrTextContent("Add", "Bones are added based on the Selection in the Hierarchy.\nChains are added based on the Selection in the Treeview below."), EditorStyles.toolbarPopup);
            if (Event.current.type == EventType.Repaint)
                popupRect = GUILayoutUtility.GetLastRect();

            popupRect.width = 200f;

            if (buttonResult)
            {
                string[] options = { "Add Bone", "Add Bone and Childrean", "Add Chain" };
                void callback(int i)
                {
                    switch (i)
                    {
                        case 0:
                            if (selection == null)
                            {
                                Debug.Log("Please Select a bone in the Hierarchy.");
                                notificationBar.Push("Please Select a bone in the Hierarchy.");
                                return;
                            }
                            if (selection == _builder.transform || selection.IsChildOf(_builder.transform) == false)
                            {
                                Debug.Log("Selected bone must be a child of the root transform.");
                                notificationBar.Push("Selected bone must be a child of the root transform.");
                                return;
                            }

                            AddBone(selection);
                            notificationBar.Push($"Added bone '{selection.name}'.");
                            UpdateList();
                            break;

                        case 1:
                            if (selection == null)
                            {
                                Debug.Log("Please Select a bone in the Hierarchy.");
                                notificationBar.Push("Please Select a bone in the Hierarchy.");
                                return;
                            }
                            if (selection == _builder.transform || selection.IsChildOf(_builder.transform) == false)
                            {
                                Debug.Log("Selected bone must be a child of the root transform.");
                                notificationBar.Push("Selected bone must be a child of the root transform.");
                                return;
                            }
                            int count = _builder.bones.Count;
                            AddAllBones(selection);
                            count = _builder.bones.Count - count;
                            notificationBar.Push($"Added {count} bones.");

                            UpdateList();
                            break;

                        case 2:
                            if (transformTreeView.HasSelection() == false)
                            {
                                Debug.Log("Please Select bones in the Treeview.");
                                notificationBar.Push("Please Select bones in the Treeview.");
                                return;
                            }
                            AddChain(transformTreeView.GetSelection());
                            /*else
                                AddChain(Selection.GetTransforms(SelectionMode.Editable));*/
                            UpdateList();
                            break;

                        default:
                            break;
                    }
                }
                PopupWindow.Show(popupRect, Popup.Create(popupRect, options, callback));
            }

            return buttonResult;
        }

        private void UpdateList()
        {
            if (transformTreeView != null)
                transformTreeView.UpdateList(_builder.boneTransforms);
            serializedObject.SetIsDifferentCacheDirty();
            serializedObject.Update();
            Repaint();
        }

        private void ForceTPose(Chain chain)
        {
            Vector3 forward = Vector3.forward;
            Vector3 up = Vector3.up;
            Vector3 upLast = Vector3.up;
            bool ignoreLast = false;

            if(chain.source == SourceChain.LEG)
            {
                forward = Vector3.down;
                up = Vector3.forward;
                ignoreLast = true;
            }
            if(chain.source == SourceChain.ARM)
            {
                if(chain.side == SourceSide.LEFT)
                    forward = Vector3.left;
                if(chain.side == SourceSide.RIGHT)
                    forward = Vector3.right;
                up = Vector3.back;
                upLast = Vector3.up;
            }


            for (int i = 0; i < chain.count; i++)
            {
                if (i == chain.count - 1 && ignoreLast)
                    continue;

                Bone bone = FindBone(chain[i].boneName);
                Transform transform = FindTransform(chain[i].boneName);
                Transform parent = transform.parent;

                Axis axis = bone.axis;

                Quaternion rot = Quaternion.FromToRotation(transform.rotation * axis.forward, forward) * transform.rotation;
                //rot = parent.rotation * rot;

                if (i == chain.count - 1)
                    up = upLast;

                rot = Quaternion.FromToRotation(rot * axis.up, up) * rot;

                Undo.RecordObject(transform, $"Force TPose {chain.side} {chain.source}");
                Undo.RecordObject(_builder, $"Force TPose {chain.side} {chain.source}");
                transform.rotation = rot;

                UpdateBone(bone);
            }

            //TODO: Update the children ob the chains last bone!

            serializedObject.Update();
        }

        private void UpdateBone(Bone bone)
        {
            Transform transform = FindTransform(bone.boneName);
            bone.Update(transform);
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
                    if (reorderableBones.index == -1 && reorderableBones.count > 0)
                    {
                        reorderableBones.GrabKeyboardFocus();
                        selectedProperty = reorderableBones.serializedProperty.GetArrayElementAtIndex(0);
                        selectedProperty.isExpanded = true;
                    }

                    if (reorderableBones.count <= 0)
                        selectedProperty = null;

                    _showTransforms = false;
                    _showBones = true;
                    _showChains = false;
                }
                if (GUILayout.Button("Chains", EditorStyles.toolbarButton))
                {
                    if (reorderableChains.index == -1 && reorderableChains.count > 0)
                    {
                        selectedProperty = reorderableChains.serializedProperty.GetArrayElementAtIndex(0);
                        selectedProperty.isExpanded = true;
                    }

                    if (reorderableChains.count <= 0)
                        selectedProperty = null;

                    _showTransforms = false;
                    _showBones = false;
                    _showChains = true;
                }
            }
        }

        Vector2 _scrollPosition;
        Rect treeViewRect;
        float sideBarWidth = 180;
        bool changeLayout;
        private Rect popupRect;
        private Rect popupRect2;

        private void DrawBoneList()
        {
            if (_showTransforms)
            {
                using (new GUILayout.VerticalScope("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    if (GUILayout.Button("Expand All"))
                        transformTreeView.ExpandAll();

                    if (GUILayout.Button("Collapse All"))
                        transformTreeView.CollapseAll();

                    treeViewRect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    transformTreeView.OnGUI(treeViewRect);
                }
                return;
            }


            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(_scrollPosition, GUILayout.MaxWidth(sideBarWidth)))
                {
                    using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(sideBarWidth), GUILayout.ExpandHeight(true)))
                    {
                        _scrollPosition = scrollViewScope.scrollPosition;
                        if (_showBones)
                            reorderableBones.DoLayoutList();
                        if (_showChains)
                            reorderableChains.DoLayoutList();
                    
                    }
                }

                Rect rect = GUILayoutUtility.GetRect(4f, 0f, GUILayout.MaxWidth(4f), GUILayout.ExpandHeight(true));
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);

                Event evt = Event.current;
                EventType eventType = evt.type;

                if (rect.Contains(evt.mousePosition))
                {
                    if (eventType == EventType.MouseDown)
                        changeLayout = true;
                }

                if (eventType == EventType.MouseUp)
                    changeLayout = false;

                if (changeLayout)
                {
                    Rect mouseRect = new Rect(0f, 0f, _window.position.width, _window.position.height);
                    EditorGUIUtility.AddCursorRect(mouseRect, MouseCursor.ResizeHorizontal);
                    if (eventType == EventType.MouseDrag)
                    {
                        sideBarWidth = evt.mousePosition.x;
                        GUI.changed = true;
                        evt.Use();
                    }
                }

                using (new GUILayout.VerticalScope("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    if (selectedProperty == null)
                        return;

                    EditorGUILayout.PropertyField(selectedProperty, true, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                    bool test = serializedObject.ApplyModifiedProperties();

                }
            }
        }

        private void BoneControls()
        {
            if (selectedProperty == null)
                return;

            if (GUILayout.Button(EditorGUIUtility.TrTextContent("Recalculate Forward", "Recalculates the forward axis of the bone based on the first child."), EditorStyles.toolbarButton))
            {
                Bone bone = _builder.bones.Find((b) => b.boneName.Equals(selectedProperty.FindPropertyRelative("boneName").stringValue));
                Bone child = _builder.bones.Find((b) => b.boneName.Equals(selectedProperty.FindPropertyRelative("childNames").GetArrayElementAtIndex(0).stringValue));
                bone.ComputeForwardAxis(child);
            }

            if (GUILayout.Button(EditorGUIUtility.TrTextContent("Recalculate Length", "Recalculates the length of the bone based on the first child."), EditorStyles.toolbarButton))
            {
                Bone bone = _builder.bones.Find((b) => b.boneName.Equals(selectedProperty.FindPropertyRelative("boneName").stringValue));
                Bone child = _builder.bones.Find((b) => b.boneName.Equals(selectedProperty.FindPropertyRelative("childNames").GetArrayElementAtIndex(0).stringValue));
                bone.ComputeLength(child);
            }

            if (GUILayout.Button(EditorGUIUtility.TrTextContent("Shift Childs", "Shift the childrean in the array one up. Make sure you recalculate the forward axis and length afterwards."), EditorStyles.toolbarButton))
            {
                Bone bone = _builder.bones.Find((b) => b.boneName.Equals(selectedProperty.FindPropertyRelative("boneName").stringValue));

                string first = bone.childNames.First();

                for (int i = 1; i < bone.childNames.Count; i++)
                {
                    bone.childNames[i - 1] = bone.childNames[i];
                }
                bone.childNames[bone.childNames.Count - 1] = first;
            }
        }

        private void ChainControls()
        {
            if (selectedProperty == null)
                return;

            if (GUILayout.Button(EditorGUIUtility.TrTextContent("Calculate short Length", "Calculates the length of the chain from the start to the end based on the positions in the bind pose."), EditorStyles.toolbarButton))
            {
                Chain chain = selectedProperty.GetValue<Chain>();
                float length = Vector3.Distance(chain.First().model.position, chain.Last().model.position);
                chain.length = length;
            }
            if (GUILayout.Button(EditorGUIUtility.TrTextContent("Calculate Length", "Calculates the length of the chain as a sum of all bone lengths."), EditorStyles.toolbarButton))
            {
                Chain chain = selectedProperty.GetValue<Chain>();
                chain.ComputeLength();
            }
            if (GUILayout.Button(EditorGUIUtility.TrTextContent("Force TPose", "Brings the chain bones into a tpose position. Make sure you adjust the bone up axis of the chain bones first."), EditorStyles.toolbarButton))
            {
                Chain chain = selectedProperty.GetValue<Chain>();
                ForceTPose(chain);
            }
        }

        #region Add Bone/Chain/Transform

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

        private Bone AddBone(Transform transform)
        {
            string name = transform.name.ToLower();
            return AddBone(transform, GetSide(name), GetBone(name));
        }

        private Bone AddBone(Transform transform, SourceSide sourceSide, SourceBone sourceBone)
        {
            AddTransform(transform);

            Bone bone = new Bone(transform)
            {
                side = sourceSide,
                source = sourceBone
            }; 
            
            if (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                Vector3 alternativeForward = transform.InverseTransformPoint(child.position);
                Axis axis = new Axis(alternativeForward, Vector3.forward);
                
                bone.alternativeForward = axis.forward;
                bone.alternativeUp = axis.up;
                bone.ComputeLength(child);
            }

            AddBone(bone);
            return bone;
        }

        private void AddBone(Bone bone)
        {
            _builder.bones.Add(bone);

            Bone parent = _builder.bones.Find((b) => b.boneName.Equals(bone.parentName));
            if (parent == null)
                return;
            parent.childNames.Add(bone.boneName);
        }

        private void AddChain(Transform[] transforms)
        {
            string name = transforms.First().name;
            AddChain(transforms, GetSide(name), GetChain(name));
        }

        private void AddChain(Transform[] transforms, SourceSide sourceSide, SourceChain sourceChain)
        {
            Bone[] chainBones = new Bone[transforms.Length];

            for (int i = 0; i < transforms.Length; i++)
            {
                Bone bone = FindBone(transforms[i].name);
                if(bone == null)
                {
                    bone = AddBone(transforms[i]);
                }
                chainBones[i] = bone;
            }
            AddTransforms(transforms);

            Chain chain = new Chain(chainBones)
            {
                side = sourceSide,
                source = sourceChain
            };
            _builder.chains.Add(chain);

            notificationBar.Push($"Added Chain {chain.side} {chain.source}.");

            SetChainBones(chainBones, chain);
        }

        private void AddChain(IList<int> indecies)
        {
            Bone[] chainBones = new Bone[indecies.Count];
            for (int i = 0; i < indecies.Count; i++)
            {
                chainBones[i] = new Bone(_builder.bones[indecies[i]]);
            }

            string name = chainBones.First().boneName.ToLower();

            Chain chain = new Chain(chainBones)
            {
                side = GetSide(name),
                source = GetChain(name)
            };
            //_builder.chains.Add(chain);

            SerializedProperty chainList = serializedObject.FindProperty("chains");
            int index = chainList.arraySize;

            chainList.InsertArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();

            chainList.GetLastArrayElement().SetValue(chain);
            serializedObject.Update();

            notificationBar.Push($"Added Chain {chain.side} {chain.source}.");
            SetChainBones(chainBones, chain);
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

        #endregion

        #region Remove Bone/Chain
        private void OnRemoveBoneItem(ReorderableList list)
        {
            selectedProperty = null;

            Bone bone = _builder.bones[list.index];
            Transform transform = _builder.boneTransforms.Find((t) => t.name.Equals(bone.boneName));



            Bone parent = _builder.bones.Find((b) => b.boneName.Equals(bone.parentName));
            if (parent != null)
                parent.childNames.Remove(bone.boneName);

            for (int i = 0; i < bone.childNames.Count; i++)
            {
                Bone child = _builder.bones.Find((b) => b.boneName.Equals(bone.childNames[i]));
                child.parentName = bone.parentName;

                BoneTransform parentTransform = BoneTransform.zero;
                if (parent != null)
                    parentTransform = parent.model;

                child.UpdateLocal(parentTransform);
            }


            _builder.boneTransforms.Remove(transform);
            _builder.bones.RemoveAt(list.index);
            UpdateList();
            //list.serializedProperty.DeleteArrayElementAtIndex(list.index); 
        }

        private void OnRemoveChainItem(ReorderableList list)
        {
            selectedProperty = null;
            _builder.chains.RemoveAt(list.index);
            UpdateList();
            //list.serializedProperty.DeleteArrayElementAtIndex(list.index);
        }

        #endregion

        #region Find Bone/Chain/Transform

        private Transform FindTransform(string boneName)
        {
            return _builder.boneTransforms.Find((t) => t.name.Equals(boneName));
        }

        private Bone FindBone(string boneName)
        {
            return _builder.bones.Find((b) => b.boneName.Equals(boneName));
        }

        private SerializedProperty FindBoneProperty(string boneName)
        {
            for (int i = 0; i < _builder.bones.Count; i++)
            {
                if (_builder.bones[i].boneName.Equals(boneName))
                {
                    return reorderableBones.serializedProperty.GetArrayElementAtIndex(i);
                    /*SerializedProperty boneProp = serializedObject.FindProperty("bones").GetArrayElementAtIndex(i);
                    Debug.Log(boneProp.FindPropertyRelative("boneName").stringValue);
                    return boneProp;*/
                }
            }
            return null;
        }

        private SerializedProperty FindBoneProperty(SerializedProperty bone)
        {
            //SerializedProperty bonesArray = serializedObject.FindProperty("bones");
            SerializedProperty bonesArray = reorderableBones.serializedProperty;

            for (int i = 0; i < bonesArray.arraySize; i++)
            {
                SerializedProperty boneProp = bonesArray.GetArrayElementAtIndex(i);
                if (SerializedProperty.EqualContents(bone, boneProp))
                    return boneProp;
            }
            return null;
        }

        private Chain FindChain(SourceChain sourceChain, SourceSide sourceSide)
        {
            return _builder.chains.Find((c) => c.source == sourceChain && c.side == sourceSide);
        }
        private SerializedProperty FindChainProperty(SourceChain sourceChain, SourceSide sourceSide)
        {

            SerializedProperty chainArray = serializedObject.FindProperty("chains");
            for (int i = 0; i < chainArray.arraySize; i++)
            {
                SerializedProperty chain = chainArray.GetArrayElementAtIndex(i);

                if (chain.FindPropertyRelative("source").enumValueIndex == ((int)sourceChain) && chain.FindPropertyRelative("side").enumValueIndex == ((int)sourceSide))
                {
                    return chain;
                }
            }
            Debug.Log("Chain null");
            return null;
        }

        #endregion

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
            name = name.ToLower();
            if (IsLeft(name))
                return SourceSide.LEFT;
            if (IsRight(name))
                return SourceSide.RIGHT;
            return SourceSide.MIDDLE;
        }

        private SourceBone GetBone(string name)
        {
            name = name.ToLower();
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
            name = name.ToLower();
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
                    SerializedProperty bone = FindBoneProperty(chainBones[i].boneName);
                    bone.FindPropertyRelative("source").enumValueIndex = start + i;
                    bone.FindPropertyRelative("side").enumValueIndex = (int)chain.side;
                }
                return;
            }

            for (int i = 0; i < chainBones.Length; i++)
            {
                SerializedProperty bone = FindBoneProperty(chainBones[i].boneName);
                bone.FindPropertyRelative("source").enumValueIndex = start + i;
                bone.FindPropertyRelative("side").enumValueIndex = (int)chain.side;
            }

            serializedObject.ApplyModifiedProperties();
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

        void DrawBoneListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderableBones.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty name = element.FindPropertyRelative("boneName");
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), name.stringValue);

            if (reorderableBones.index != index)
                return;

            element.isExpanded = true;
            selectedProperty = element;
        }
        void DrawChainListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = reorderableChains.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty side = element.FindPropertyRelative("side");
            SerializedProperty chain = element.FindPropertyRelative("source");

            string name = side.enumDisplayNames[side.enumValueIndex] + " " + chain.enumDisplayNames[chain.enumValueIndex];
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), name);

            if (reorderableChains.index != index)
                return;

            element.isExpanded = true;
            selectedProperty = element;
        }
    }
}
