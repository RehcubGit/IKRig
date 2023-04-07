using UnityEditor;
using UnityEngine;

namespace Rehcub 
{
    public class IKSourceConfigurator : ExtendedEditorWindow
    {
        private static IKSourceConfigurator _window;
        private IKSource _iKSource;
        private AnimationClip _clip;

        private IKAnimationData _animationData;

        private bool _isPlaying;
        private float _startTime;

        private bool _showAnimationDebug;
        private int _currentKeyframe;

        private float _rootAngle; 
        Vector3 _rootAxis;


        private bool _hasRootCurve;
        private bool _hasBakedRootMotion;
        private bool _hasBakedRootRotation;

        public static void ShowConfigurator(IKSource iKSource)
        {
            IKSourceConfigurator window = GetWindow<IKSourceConfigurator>("IK Source Configurator");
            window.serializedObject = new SerializedObject(iKSource);
            window._iKSource = iKSource;
            window._animationData = iKSource.AnimationData;
            _window = window;
            _window.position.Set(0f, _window.position.y, 550f, 140f);

            _window.Init();
        }

        private void Init()
        {
            currentProperty = serializedObject.FindProperty("_clip");

            if (currentProperty.objectReferenceValue == null)
                return;

            GetClipData();
        }

        private void GetClipData()
        {
            _hasRootCurve = _iKSource.HasRootMotionCurve();
            _hasBakedRootMotion = _iKSource.HasBakedRootMotion();
            _hasBakedRootRotation = _iKSource.HasBakedRootRotation();

            _rootAngle = _iKSource.GetRootRotationAngle();
            _rootAxis = _iKSource.GetRootMotionAxis();
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
                _iKSource.ResetToBindPose();
        }

        private void OnGUI()
        {
            currentProperty = serializedObject.FindProperty("_clip");

            _clip = (AnimationClip) currentProperty.objectReferenceValue;
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                int frames = (int)(_clip.length * _clip.frameRate);
                _currentKeyframe = EditorGUILayout.IntSlider(_currentKeyframe, 0, frames);

                if (check.changed)
                {
                    _iKSource.SampleAnimation(_currentKeyframe);
                }
            }
            
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(currentProperty);

                if (check.changed)
                {
                    if (currentProperty.objectReferenceValue == null)
                        return;

                    Apply();

                    GetClipData();

                    _currentKeyframe = 0;
                    _iKSource.SampleAnimation(_currentKeyframe);
                }
            }

            if (_clip == null)
                return;

            DrawPlayControls();

            if (GUILayout.Button("Create IK Animation Object"))
            {
                //TODO: Make a toggle with has root motion
                _animationData = _iKSource.CreateIKAnimation(_rootAxis, _rootAngle);

                if (AssetDatabase.IsValidFolder("Assets/IKAnimations") == false)
                    AssetDatabase.CreateFolder("Assets", "IKAnimations");

                string name = AssetDatabase.GenerateUniqueAssetPath($"Assets/IKAnimations/{_animationData.animationName}.asset");
                AssetDatabase.CreateAsset(_animationData, name);
                AssetDatabase.SaveAssets();

                _iKSource.ResetToBindPose();
            }

            _showAnimationDebug = GUILayout.Toggle(_showAnimationDebug, EditorGUIUtility.TrTextContent("Debug", "Enable/disable scene debug view."), EditorStyles.toolbarButton);
            if (GUILayout.Button(EditorGUIUtility.TrTextContent("Reset Pose", "Reset To bind pose."), EditorStyles.toolbarButton))
                _iKSource.ResetToBindPose();

            Apply();

            if (_hasRootCurve)
            {
                using (new GUILayout.VerticalScope("box", GUILayout.ExpandWidth(true)))
                {
                    EditorGUILayout.LabelField("The animation has root motion curves");

                    _clip.GetAnimationRoot(_clip.length, out Vector3 motion, out Quaternion rotation);

                    Vector3 euler = rotation.eulerAngles;
                    EditorGUILayout.LabelField($"Motion: ({motion.x}, {motion.y}, {motion.z})");
                    EditorGUILayout.LabelField($"Rotation: ({euler.x}, {euler.y}, {euler.z})");
                }
                return;
            }

            if (_hasBakedRootMotion)
            {
                using (new GUILayout.HorizontalScope("box", GUILayout.ExpandWidth(true)))
                {
                    EditorGUILayout.LabelField($"Baked root motion axis: {_rootAxis.x}, {_rootAxis.y}, {_rootAxis.z}", GUILayout.MaxWidth(200f));

                    EditorGUILayout.LabelField("X:", GUILayout.MaxWidth(15f));
                    _rootAxis.x = EditorGUILayout.Toggle(_rootAxis.x == 1, GUILayout.MaxWidth(30f)).ToFloat();

                    EditorGUILayout.LabelField("Y:", GUILayout.MaxWidth(15f));
                    _rootAxis.y = EditorGUILayout.Toggle(_rootAxis.y == 1, GUILayout.MaxWidth(30f)).ToFloat();

                    EditorGUILayout.LabelField("Z:", GUILayout.MaxWidth(15f));
                    _rootAxis.z = EditorGUILayout.Toggle(_rootAxis.z == 1, GUILayout.MaxWidth(30f)).ToFloat();

                    if (GUILayout.Button("Reset", GUILayout.MaxWidth(50f)))
                    {
                        _rootAxis = _iKSource.GetRootMotionAxis();
                    }
                }

            }

            if (_hasBakedRootRotation)
            {
                using (new GUILayout.HorizontalScope("box", GUILayout.ExpandWidth(true)))
                {
                    EditorGUILayout.LabelField("Baked root rotation: ", GUILayout.MaxWidth(200f));

                    EditorGUILayout.LabelField(EditorGUIUtility.TrTextContent("Angle: ",
                        "When the Rotation of the hip first hits that value the rotation of the hip will no longer be extracted!"), 
                        GUILayout.MaxWidth(40f)
                        );
                    _rootAngle = EditorGUILayout.FloatField(_rootAngle, GUILayout.MaxWidth(107f));

                    if (GUILayout.Button("Reset", GUILayout.MaxWidth(50f)))
                    {
                        _rootAngle = _iKSource.GetRootRotationAngle();
                    }
                }
            }
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
            _iKSource.SampleAnimation(_currentKeyframe);
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
                    _iKSource.SampleAnimation(_currentKeyframe);
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
                    _iKSource.SampleAnimation(_currentKeyframe);
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
        }
    }
}
