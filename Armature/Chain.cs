using UnityEngine;

namespace Rehcub 
{
    [System.Serializable]
    public class Chain
    {
        [SerializeField] protected Bone[] _bones;
        [ReadOnly]
        [Tooltip("The number of bones in the chain")]
        public int count;

        public SourceSide side;
        public SourceChain source;

        [Tooltip("The direction in local space which points from the first bone to the second bone")]
        public Vector3 alternativeForward = Vector3.forward;
        [Tooltip("The direction in local space in which the chain will bend (pole target)")]
        public Vector3 alternativeUp = Vector3.up;

        [ReadOnly]
        [Tooltip("The length of all bones combined")]
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

            ComputeLength();
            ComputeForwardAxis();

            switch (count)
            {
                case 1:
                case 2:
                    solver = new BoneSolver(this);
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

        public void Solve(Armature armature, IKChain ikChain) => Solve(armature, armature.currentPose, ikChain);

        public void Solve(Armature armature, Pose pose, IKChain ikChain) => Solve(armature, pose, ikChain, pose.GetParentModelTransform(First()));
        public void Solve(Armature armature, IKChain ikChain, BoneTransform parentTransform) => Solve(armature, armature.currentPose, ikChain, parentTransform);
        public void Solve(Armature armature, Pose pose, IKChain ikChain, BoneTransform parentTransform)
        {
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

        public void ComputeForwardAxis()
        {
            if (_bones.Length <= 1)
                return;

            Bone a = _bones[0];
            Bone b = _bones[1];

            alternativeForward = a.model.InverseTransformDirection(b.model.position - a.model.position).normalized; 
            Vector3 left = alternativeForward.OrthogonalVector();
            alternativeUp = left;
        }

        public Bone First() => _bones.First();
        public Bone Last() => _bones.Last();
        private void ComputeLength()
        {
            if(_bones.Length == 1)
            {
                length = _bones[0].length;
                return;
            }

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
