using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public class Chain
    {
        [SerializeField] protected Bone[] _bones;
        [ReadOnly]
        public int count;

        public SourceSide side;
        public SourceChain source;

        [Tooltip("The direction in local space which points from the first bone to the second bone")]
        public Vector3 alternativeForward = Vector3.forward;
        [Tooltip("The direction in local space in which the chain will bend (pole target)")]
        public Vector3 alternativeUp = Vector3.up;

        [ReadOnly]
        public float length;

        [SerializeReference] public Solver solver;


        public Bone this[int i]
        {
            get { return _bones[i]; }
        }

        public Chain(params Bone[] bones)
        {
            _bones = bones;
            count = bones.Length;
            Debug.Log(count);

            ComputeLength();
            ComputeForwardAxis(bones[0], bones[1]);

            switch (count)
            {
                case 1:
                case 2:
                    solver = null;
                    break;
                case 3:
                    solver = new LimbSolver(this);
                    break;
                case 4:
                    solver = new ZigZagSolver(this);
                    break;
                default:
                    solver = new SpringSolver(this);
                    break;
            }
        }

        public Chain(Chain chain)
        {
            _bones = chain._bones;
            count = _bones.Length;
            length = chain.length;
            alternativeForward = chain.alternativeForward;
            alternativeUp = chain.alternativeUp; 
        }

        public void Solve(Armature armature, IKChain ikChain)
        {
            BoneTransform parentTransform = armature.currentPose.GetParentModelTransform(First());
            solver.Check(this);
            solver.Solve(armature.bindPose, armature.currentPose, ikChain, parentTransform);
        }

        public void Solve(Armature armature, IKChain ikChain, Pose pose)
        {
            BoneTransform parentTransform = pose.GetParentModelTransform(First());
            solver.Check(this);
            solver.Solve(armature.bindPose, pose, ikChain, parentTransform);
        }

        /// <summary>
        /// Setting the axis for the ik computation.
        /// </summary>
        /// <param name="forward">The direction in local space which points from the first bone to the second bone</param>
        /// <param name="up">The direction in local space in which the chain is bending (pole target direction)</param>
        public void SetAxis(Vector3 forward, Vector3 up)
        {
            alternativeForward = forward;
            alternativeUp = up;
        }

        public void ComputeForwardAxis(Transform a, Transform b)
        {
            alternativeForward = a.InverseTransformDirection((b.position - a.position).normalized);
            Vector3 left = alternativeForward.OrthogonalVector();
            alternativeUp = left;
        }

        public void ComputeForwardAxis(Bone a, Bone b)
        {
            Vector3 forward = b.model.position - a.model.position;
            alternativeForward = (a.model.rotation * forward).normalized;
            alternativeUp = (a.model.rotation * Vector3.forward).normalized;
        }

        public Bone First() => _bones.First();
        public Bone Last() => _bones.Last();
        private void ComputeLength()
        {
            int end = _bones.Length - 1;
            float sum = 0;

            for (int i = 0; i < end; i++)
            {
                float length = Vector3.Distance(this._bones[i].model.position, this._bones[i + 1].model.position);
                _bones[i].length = length;

                sum += length;
            }

            length = sum;
        }
    }
}
