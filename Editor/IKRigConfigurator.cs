using Bewildered.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rehcub
{
    public class IKRigConfigurator : ExtendedEditorWindow
    {
        #region Fields

        private static IKRigConfigurator _window;
        private Editor settingsEditor;
        private IKRig _rig;
        Animation _animation;
        private bool _showArmatur = true;
        private bool _showAnimation;
        private bool _showAnimationDebug;
        private bool _editAnimation;

        private int _currentKeyframe;

        PopupExample popup;
        float scrollMod;
        private Rect popupRect;

        private Vector2 _scrollPosition;

        private int FPS = 30;
        private float _startTime;
        private bool _isPlaying;

        private IEnumerable<System.Type> _constraintTypes;


        #endregion

        #region Init

        [MenuItem("Window/IK Rig/Configurator")]
        public static void ShowConfigurator()
        {
            GetWindow<IKRigConfigurator>("IK Rig Configurator");
        }
        
        public static void ShowConfigurator(IKRig ikRig)
        {
            _window = GetWindow<IKRigConfigurator>("IK Rig Configurator");
            _window.serializedObject = new SerializedObject(ikRig);
            _window._rig = ikRig;
            ikRig.Init();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += DuringSceneGUI;
            Undo.undoRedoPerformed += UndoRepaint;

            onSelectProperty += (prop) =>
            {
                _rig.ResetToBindPose();
                _currentKeyframe = 0;
                
                IKAnimationData animationData = (IKAnimationData)prop.objectReferenceValue;
                if (animationData == null)
                    return;

                _animation = _rig.GetAnimation(animationData);
                _rig.ApplyIkPose(animationData, _currentKeyframe);

            };
        }
        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            Undo.undoRedoPerformed -= UndoRepaint;
            if (_isPlaying)
            {
                EditorApplication.update -= PlayAnimation;
            }
        }
        private void OnDestroy()
        {
            if(_rig != null)
                _rig.ResetToBindPose();
            SceneView.duringSceneGui -= DuringSceneGUI;
            Undo.undoRedoPerformed -= UndoRepaint;
            if (_isPlaying)
            {
                EditorApplication.update -= PlayAnimation;
            }
            AddModifierPopup.ClosePopup();
        }

        private void UndoRepaint()
        {
            if (settingsEditor == null)
                return;
            if (settingsEditor.serializedObject == null)
                return;
            settingsEditor.serializedObject.Update();
            ApplyIkPose();
            Repaint();
        }
        #endregion

        #region Debug
        private void DuringSceneGUI(SceneView sceneView)
        {
            if (_window == null)
            {
                if(_rig != null)
                {
                    ShowConfigurator(_rig); 
                    return;
                }
                SceneView.duringSceneGui -= DuringSceneGUI;
                if (_isPlaying)
                    EditorApplication.update -= PlayAnimation;
                return;
            }

            if (_rig.Armature == null)
                return;

            if (_showArmatur)
            {
                foreach (Chain chain in _rig.Armature.GetAllChains())
                {
                    IKEditorDebug.DrawChain(chain);
                }
                Apply();
                return;
            }

            if (selectedProperty == null)
                return;
            if (selectedProperty.objectReferenceValue == null)
                return;

            IKAnimationData animationData = (IKAnimationData)selectedProperty.objectReferenceValue;
            if (_showAnimationDebug)
            {
                IKEditorDebug.DrawPose(_rig.Armature, _rig.Armature.currentPose);
                IKEditorDebug.DrawBone(_rig.Armature.currentPose, _rig.Armature.GetChains(SourceChain.ARM, SourceSide.LEFT)[0].Last(), animationData.animation.GetFrame(_currentKeyframe).leftArm.endEffector);
            }

            if (_editAnimation == false)
                return;
        }

        #endregion

        #region Play Animation

        private void StartPlayingAnimation()
        {
            EditorApplication.update += PlayAnimation;
            _isPlaying = true;
            _startTime = (float)EditorApplication.timeSinceStartup;
        }

        private void PlayAnimation()
        {
            float time = (float)EditorApplication.timeSinceStartup - _startTime;
            int frame = Mathf.FloorToInt(time * FPS);

            //_rig.transform.Rotate(0, 1f, 0);

            Repaint();
            IKAnimationData animationData = (IKAnimationData)selectedProperty.objectReferenceValue;


            SerializedProperty animation = settingsEditor.serializedObject.FindProperty("animation");
            SerializedProperty loop = settingsEditor.serializedObject.FindProperty("loop");
            SerializedProperty keyframes = animation.FindPropertyRelative("_keyframes");

            int size = keyframes.arraySize - 1; //Last and First frame are the same
            if (frame >= size)
            {
                frame %= size;
                if(loop.boolValue == false)
                {
                    EditorApplication.update -= PlayAnimation;
                    _isPlaying = false;
                }
            }

            if (frame == _currentKeyframe)
                return;

            _currentKeyframe = frame;
            _rig.ApplyIkPose(animationData, _currentKeyframe);
        }
        #endregion

        #region Draw Functions
        private void OnGUI()
        {
            if (serializedObject == null)
            {
                EditorGUILayout.LabelField("No Rig selected!");
                return;
            }

            //TODO: if Armature is not build jet draw the armature builder panel!!

            HandleInput();

            DrawHeaderToolBar();

            if (_showArmatur)
            {
                //serializedObject.Update();
                using (var scrollViewScope = new GUILayout.ScrollViewScope(_scrollPosition))
                {
                    _scrollPosition = scrollViewScope.scrollPosition;
                    currentProperty = serializedObject.FindProperty("_armature");
                    currentProperty = currentProperty.FindPropertyRelative("_chains");

                    EditorGUILayout.PropertyField(currentProperty);
                }
            }

            if (_showAnimation)
            {
                serializedObject.Update();
                DrawAnimationEditor();
                Apply();
                return;
            }

            if (GUI.changed) 
                Repaint();

            Apply();
        }

        private void DrawHeaderToolBar()
        {
            using (new GUILayout.HorizontalScope("box", GUILayout.ExpandWidth(true)))
            {
                if (GUILayout.Button("Armature", EditorStyles.toolbarButton))
                {
                    _showArmatur = true;
                    _showAnimation = false;
                    _rig.ResetToBindPose();
                }
                if (GUILayout.Button("Animation", EditorStyles.toolbarButton))
                {
                    _showArmatur = false;
                    _showAnimation = true;
                    _rig.ResetToBindPose();
                }
            }
        }

        private void DrawAnimationEditor()
        {
            currentProperty = serializedObject.FindProperty("_animations");

            /*if(settingsEditor != null)
                settingsEditor.serializedObject.Update();*/

            using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                using (new GUILayout.VerticalScope("box", GUILayout.MaxWidth(180), GUILayout.ExpandHeight(true)))
                {
                    DrawSideBar(currentProperty, (prop) =>
                    {
                        if (prop.objectReferenceValue == null)
                            return "NULL";
                        SerializedObject propObj = new SerializedObject(prop.objectReferenceValue);
                        SerializedProperty nameProp = propObj.FindProperty("animationName");
                        string name = nameProp.stringValue;
                        return name;
                    });

                    Drag();
                }

                using (new GUILayout.VerticalScope("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
                {
                    if (selectedProperty == null)
                    {
                        GUILayout.Label("Select a Animation to Edit!");
                        return;
                    }

                    EditorGUILayout.PropertyField(selectedProperty);

                    if (selectedProperty.objectReferenceValue == null)
                        return;

                    DrawSettingsEditor(selectedProperty.objectReferenceValue, ref settingsEditor);
                    //GUILayout.Space(15);
                    Separator(1, 30);

                    if(GUILayout.Button("Create Animation Clip"))
                    {
                        CreateAnimationClip();
                    }

                    SerializedProperty animation = settingsEditor.serializedObject.FindProperty("animation");
                    SerializedProperty keyframes = animation.FindPropertyRelative("_keyframes");

                    int previewsKeyframe = _currentKeyframe;
                    _currentKeyframe = EditorGUILayout.IntSlider(_currentKeyframe, 0, keyframes.arraySize - 1);
                    /*if (previewsKeyframe != _currentKeyframe)
                        ApplyIkPose();*/
                    if (previewsKeyframe != _currentKeyframe)
                        ApplyPose();

                    DrawPlayControls(keyframes.arraySize);

                    //Undo.RecordObject(settingsEditor.serializedObject.targetObject, "Generic Animation Apply");
                    //Undo.RegisterCompleteObjectUndo(settingsEditor.serializedObject.targetObject, "Test");
                    if (settingsEditor.serializedObject.ApplyModifiedProperties())
                    {
                        //ApplyIkPose();
                        ApplyPose();
                    }
                }
            }

        }

        private void DrawPlayControls(int keyframeCount)
        {
            using (new GUILayout.HorizontalScope("box", GUILayout.ExpandWidth(true)))
            {
                GUILayoutOption[] options =
                {
                            GUILayout.MaxWidth(32f),
                            GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight)
                        };

                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Animation.PrevKey@2x"), options))
                {
                    _currentKeyframe--;
                    if (_currentKeyframe < 0)
                        _currentKeyframe = keyframeCount - 1;
                    IKAnimationData animationData = (IKAnimationData)selectedProperty.objectReferenceValue;
                    ApplyIkPose();
                    //ApplyPose();
                }
                if (_isPlaying == false)
                {
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_PlayButton On@2x"), options))
                    {
                        StartPlayingAnimation();
                    }
                }
                else
                {
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_PauseButton On@2x"), options))
                    {
                        EditorApplication.update -= PlayAnimation;
                        _isPlaying = false;
                    }
                }
                if (GUILayout.Button(EditorGUIUtility.IconContent("d_Animation.NextKey@2x"), options))
                {
                    _currentKeyframe++;
                    if (_currentKeyframe >= keyframeCount - 1)
                        _currentKeyframe = 0;
                    IKAnimationData animationData = (IKAnimationData)selectedProperty.objectReferenceValue;
                    ApplyIkPose();
                    //ApplyPose();
                }

                FPS = EditorGUILayout.IntField("Samples", FPS);
                _showAnimationDebug = GUILayout.Toggle(_showAnimationDebug, EditorGUIUtility.TrTextContent("Debug", "Enable/disable scene debug view."), EditorStyles.toolbarButton);
                _editAnimation = GUILayout.Toggle(_editAnimation, EditorGUIUtility.TrTextContent("Edit", "Edit mode for the animation curves."), EditorStyles.toolbarButton);

                if (_editAnimation)
                    _showAnimationDebug = false;
            }
        }
        
        #endregion

        #region Input

        private void HandleInput()
        {
            HandleKeyboard();
        }

        private void HandleKeyboard()
        {
            switch (Event.current.type)
            {
                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.Space)
                    {
                        if (_isPlaying)
                        {
                            EditorApplication.update -= PlayAnimation;
                            _isPlaying = false;
                            return;
                        }
                        if (_showAnimation)
                            StartPlayingAnimation();

                        _isPlaying = true;
                        Event.current.Use();
                    }
                    break;
            }
        }
        #endregion

        #region Helper Functions

        private void AddModifier(System.Type type)
        {
            SerializedProperty modifierProp = GetModifierProperty();
            modifierProp.arraySize++;
            SerializedProperty modProp = modifierProp.GetArrayElementAtIndex(modifierProp.arraySize - 1);
            modProp.managedReferenceValue = System.Activator.CreateInstance(type);
        }

        private void RemoveModifier(int index)
        {
            SerializedProperty modifierProp = GetModifierProperty();
            modifierProp.DeleteArrayElementAtIndex(index);
        }

        private void ApplyIkPose()
        {
            if (selectedProperty == null)
                return;
            if (selectedProperty.objectReferenceValue == null)
                return;

            IKAnimationData animationData = (IKAnimationData)selectedProperty.objectReferenceValue;

            _rig.ApplyIkPose(animationData, _currentKeyframe);
        }

        private void ApplyIkPose(IKPose pose)
        {
            _rig.ApplyIkPose(pose);
            if (settingsEditor == null)
                return;
            settingsEditor.serializedObject.Update();
        }

        private void ApplyPose()
        {
            Pose pose = _animation.GetFrame(_currentKeyframe);
            ApplyPose(pose);
        }

        private void ApplyPose(Pose pose)
        {
            _rig.ApplyPose(pose);
        }

        private void CreateAnimationClip()
        {
            SerializedProperty animation = settingsEditor.serializedObject.FindProperty("animation");
            string animationName = animation.FindPropertyRelative("name").stringValue;
            SerializedProperty keyframes = animation.FindPropertyRelative("_keyframes");
            AnimationClip clip = new AnimationClip();
            clip.frameRate = 30;
            Dictionary<string, AnimationCurve[]> curves = new Dictionary<string, AnimationCurve[]>();
            AnimationCurve[] hipPositionCurves = new AnimationCurve[3];
            hipPositionCurves[0] = new AnimationCurve();
            hipPositionCurves[1] = new AnimationCurve();
            hipPositionCurves[2] = new AnimationCurve();

            float frameTime = 1.0f / 30f;

            string[] boneNames = _rig.Armature.currentPose.GetNames().ToArray();

            for (int j = 0; j < boneNames.Length; j++)
            {
                string boneName = boneNames[j];
                curves[boneName] = new AnimationCurve[4];
                curves[boneName][0] = new AnimationCurve();
                curves[boneName][1] = new AnimationCurve();
                curves[boneName][2] = new AnimationCurve();
                curves[boneName][3] = new AnimationCurve();
            }

            for (int i = 0; i < keyframes.arraySize; i++)
            {
                IKAnimationData animationData = (IKAnimationData)selectedProperty.objectReferenceValue;
                _rig.ApplyIkPose(animationData, i);

                for (int j = 0; j < boneNames.Length; j++)
                {
                    string boneName = boneNames[j];
                    //_rig.Armature.SetKey(curves[boneName], i, boneName);

                    Bone hip = _rig.Armature.currentPose[_rig.Armature.root.boneName];
                    Vector3 position = hip.local.position;
                    hipPositionCurves[0].AddKey(i * frameTime, position.x);
                    hipPositionCurves[1].AddKey(i * frameTime, position.y);
                    hipPositionCurves[2].AddKey(i * frameTime, position.z);
                }
            }

            //https://forum.unity.com/threads/new-animationclip-property-names.367288/
            for (int i = 0; i < boneNames.Length; i++)
            {
                //string path = _rig.Armature.GetPath(boneNames[i]);
                string path = ""; 
                AnimationCurve[] rotationCurves = curves[boneNames[i]];

                clip.SetCurve(path, typeof(Transform), "localRotation.x", rotationCurves[0]);
                clip.SetCurve(path, typeof(Transform), "localRotation.y", rotationCurves[1]);
                clip.SetCurve(path, typeof(Transform), "localRotation.z", rotationCurves[2]);
                clip.SetCurve(path, typeof(Transform), "localRotation.w", rotationCurves[3]);
            }

            clip.SetCurve(_rig.Armature.root.boneName, typeof(Transform), "localPosition.x", hipPositionCurves[0]);
            clip.SetCurve(_rig.Armature.root.boneName, typeof(Transform), "localPosition.y", hipPositionCurves[1]);
            clip.SetCurve(_rig.Armature.root.boneName, typeof(Transform), "localPosition.z", hipPositionCurves[2]);

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"Assets/AnimationClips/{animationName}.anim");
            AssetDatabase.CreateAsset(clip, assetPath);
            AssetDatabase.SaveAssets();
        }

        private IEnumerable<System.Type> GetTypes(System.Type type)
        {
            var types = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
            return types.Where(p => type.IsAssignableFrom(p) && !p.IsAbstract);
        }

        private void Drag()
        {
            //object[] objects = DropZone(typeof(IKAnimationData));
            object[] objects = DropZone(100, 100, typeof(IKAnimationData));

            if (objects == null)
                return;

            /*if (objects[0].GetType() != typeof(IKAnimationData))
                return;*/

            int index = currentProperty.arraySize;
            currentProperty.arraySize++;

            SerializedProperty prop = currentProperty.GetArrayElementAtIndex(index);
            prop.objectReferenceValue = (Object)objects[0];
            selectedProperty = prop;
        }

        private SerializedProperty GetModifierProperty() => settingsEditor.serializedObject.FindProperty("_modifiers");

        #endregion
    }
}
