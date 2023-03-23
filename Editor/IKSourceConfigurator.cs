using UnityEditor;
using UnityEngine;

namespace Rehcub 
{
    public class IKSourceConfigurator : ExtendedEditorWindow
    {
        private static IKSourceConfigurator _window;
        private IKSource _iKSource;
        private AnimationClip _clip;

        private IKPose _ikPose;
        private IKAnimationData _animationData;

        private bool _isPlaying;
        private float _startTime;

        private bool _showAnimationDebug;
        private int _currentKeyframe;

        public static void ShowConfigurator(IKSource iKSource)
        {
            IKSourceConfigurator window = GetWindow<IKSourceConfigurator>("IK Source Configurator");
            window.serializedObject = new SerializedObject(iKSource);
            window._iKSource = iKSource;
            window._animationData = iKSource.AnimationData;
            _window = window;
            _window.position.Set(0f, _window.position.y, 550f, 140f);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += DuringSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
        }
        private void OnDestroy()
        {
            SceneView.duringSceneGui -= DuringSceneGUI;
            if (_iKSource != null)
                _iKSource.ResetToTPose();
        }

        private void OnGUI()
        {
            currentProperty = serializedObject.FindProperty("_clip");

            _clip = (AnimationClip) currentProperty.objectReferenceValue;
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                int frames = (int)(_clip.length * _clip.frameRate);
                _currentKeyframe = EditorGUILayout.IntSlider(_currentKeyframe, 0, frames);
                EditorGUILayout.PropertyField(currentProperty);

                if (check.changed)
                    _ikPose = _iKSource.EditorDebug(_currentKeyframe);
            }

            if (_clip == null)
                return;

            DrawPlayControls();

            if (GUILayout.Button("Create IK Animation Object"))
            {
                //TODO: Make a toggle with has root motion
                _animationData = _iKSource.CreateIKAnimation();

                if (AssetDatabase.IsValidFolder("Assets/IKAnimations") == false)
                    AssetDatabase.CreateFolder("Assets", "IKAnimations");

                string name = AssetDatabase.GenerateUniqueAssetPath($"Assets/IKAnimations/{_animationData.animationName}.asset");
                AssetDatabase.CreateAsset(_animationData, name);
                AssetDatabase.SaveAssets();

                _iKSource.ResetToTPose();
            }

            _showAnimationDebug = GUILayout.Toggle(_showAnimationDebug, EditorGUIUtility.TrTextContent("Debug", "Enable/disable scene debug view."), EditorStyles.toolbarButton);
            if (GUILayout.Button(EditorGUIUtility.TrTextContent("Reset Pose", "Reset To bind pose."), EditorStyles.toolbarButton))
                _iKSource.ResetToTPose();

            Apply();
        }

        private void StartPlayingAnimation()
        {
            _startTime = (float) EditorApplication.timeSinceStartup;
            EditorApplication.update += PlayAnimation;
            _isPlaying = true;
        }

        private void PlayAnimation()
        {
            float time = (float)EditorApplication.timeSinceStartup - _startTime;
            int frame = Mathf.FloorToInt(time * _clip.frameRate);

            if (frame == _currentKeyframe)
                return;

            int keyframeCount = (int)(_clip.length * _clip.frameRate);

            _currentKeyframe = frame;

            if (_currentKeyframe >= keyframeCount)
            {
                _currentKeyframe %= keyframeCount;
                if (_clip.isLooping == false)
                {
                    EditorApplication.update -= PlayAnimation;
                    _isPlaying = false;
                }
            }
            _ikPose = _iKSource.EditorDebug(_currentKeyframe);
        }

        private void DrawPlayControls()
        {
            int keyframeCount = (int)(_clip.length * _clip.frameRate);

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
                    if (_currentKeyframe >= keyframeCount)
                        _currentKeyframe = 0;
                }
            }
        }

        private void DuringSceneGUI(SceneView sceneView)
        {
            if (_window == null)
            {
                if (_iKSource != null)
                {
                    ShowConfigurator(_iKSource);
                    return;
                }
                SceneView.duringSceneGui -= DuringSceneGUI;
                return;
            }

            if (_showAnimationDebug == false)
                return;

            IKEditorDebug.DrawPose(_iKSource.Armature, _iKSource.Armature.currentPose);

            if (_animationData == null)
                return;

            if(_ikPose == null)
                _ikPose = _animationData.animation.GetFrame(0);

            /*IKEditorDebug.DrawHip(_iKSource.Armature, _ikPose);
            IKEditorDebug.DrawIKChain(_iKSource.Armature.currentPose, _iKSource.Armature.spine, _ikPose.spine);
            IKEditorDebug.DrawIKChain(_iKSource.Armature.currentPose, _iKSource.Armature.leftArm, _ikPose.leftArm);
            IKEditorDebug.DrawIKChain(_iKSource.Armature.currentPose, _iKSource.Armature.rightArm, _ikPose.rightArm);
            IKEditorDebug.DrawIKChain(_iKSource.Armature.currentPose, _iKSource.Armature.leftLeg, _ikPose.leftLeg);
            IKEditorDebug.DrawIKChain(_iKSource.Armature.currentPose, _iKSource.Armature.rightLeg, _ikPose.rightLeg);

            IKEditorDebug.DrawBone(_iKSource.Armature.currentPose, _iKSource.Armature.head, _ikPose.head);*/
        }
    }
}
