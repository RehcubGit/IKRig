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

        private Vector2 _scrollPosition;

        private int FPS = 30;
        private float _startTime;
        private bool _isPlaying;


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
                IKPose pose = animationData.animation.GetFrame(_currentKeyframe);
                _rig.ApplyIkPose(pose, false);

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

        private void StopPlayingAnimation()
        {
            EditorApplication.update -= PlayAnimation;
            _isPlaying = false;
        }

        private void PlayAnimation()
        {
            float time = (float)EditorApplication.timeSinceStartup - _startTime;
            int frame = Mathf.FloorToInt(time * FPS);

            Repaint();
            IKAnimationData animationData = (IKAnimationData)selectedProperty.objectReferenceValue;

            bool loop = animationData.loop;

            int size = animationData.animation.FrameCount - 1; //Last and First frame are the same
            if (frame >= size)
            {
                frame %= size;
                if(loop == false)
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
                        SerializedProperty nameProp = propObj.FindProperty("_animationName");
                        string name = nameProp.stringValue;
                        return name;
                    });

                    Drag();

                    if (GUILayout.Button("Clear"))
                        currentProperty.ClearArray();

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

                    IKAnimationData animationData = (IKAnimationData)selectedProperty.objectReferenceValue;

                    if (GUILayout.Button("Create Animation Clip"))
                        AnimationClipFactory.Create(_rig, animationData);


                    int keys = animationData.animation.FrameCount;

                    int previewsKeyframe = _currentKeyframe;
                    _currentKeyframe = EditorGUILayout.IntSlider(_currentKeyframe, 0, keys - 1);
                    if (previewsKeyframe != _currentKeyframe)
                        ApplyIkPose();

                    DrawPlayControls(keys);

                    if (settingsEditor.serializedObject.ApplyModifiedProperties())
                        ApplyPose();
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

        private void ApplyIkPose()
        {
            if (selectedProperty == null)
                return;
            if (selectedProperty.objectReferenceValue == null)
                return;

            IKAnimationData animationData = (IKAnimationData)selectedProperty.objectReferenceValue;

            _rig.ApplyIkPose(animationData, _currentKeyframe);
        }

        private void ApplyIkPose(int frame)
        {
            _currentKeyframe = frame;
            ApplyIkPose();
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

        private void Drag()
        {
            object[] objects = DropZone(100, 100, typeof(IKAnimationData));

            if (objects == null)
                return;

            if (objects[0].GetType() != typeof(IKAnimationData))
                return;

            int index = currentProperty.arraySize;
            currentProperty.arraySize++;

            SerializedProperty prop = currentProperty.GetArrayElementAtIndex(index);
            prop.objectReferenceValue = (Object)objects[0];
            selectedProperty = prop;
        }

        #endregion
    }
}
