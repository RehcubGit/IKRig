using UnityEngine;

namespace Rehcub 
{
    public class PercentSolver : Solver
    {
        [SerializeField] private float minLimit = -90;
        [SerializeField] private float maxLimit;

        [Range(0, 1)]
        [SerializeField] private float open;


        public PercentSolver(Chain chain) : base(chain)
        {
        }

        public override void Solve(BindPose bindPose, Pose pose, IKChain ikChain, BoneTransform parentTransform)
        {
            BoneTransform bindLocal = bindPose.GetLocalTransform(_chain.First());
            BoneTransform childTransform = parentTransform + bindLocal;
            //Axis axis = GetAxis(ikChain);

            //Debug.Log(_chain.First().boneName);
            //Debug.Log(_chain.alternativeUp);

            Vector3 worldForward = childTransform.rotation * _chain.alternativeForward;
            Vector3 worldUp = childTransform.rotation * _chain.alternativeUp;

            Axis axis = new Axis(worldForward, worldUp);

            //float percent = Vector3.Dot(worldForward, axis.forward) * 0.5f + 0.5f;
            float percent = open;

            float angle = (maxLimit - minLimit) * percent;
            Quaternion q = Quaternion.AngleAxis(angle, axis.left);

            for (int i = 0; i < _chain.count; i++)
            {
                //Bone bind = bindPose[_chain[i].boneName]; 
                bindLocal = bindPose.GetLocalTransform(_chain[i]);

                childTransform = parentTransform + bindLocal;

                Quaternion rot = q * childTransform.rotation;
                rot = Quaternion.Inverse(parentTransform.rotation) * rot;

                pose.SetBoneLocal(_chain[i].boneName, rot);
                parentTransform += new BoneTransform(bindLocal.position, rot);
            }

            /*Vector3 bindDirection = _chain.Last().model.position - _chain.First().model.position;
            bindDirection.Normalize();
            float angle = Vector3.Angle(bindDirection, axis.forward);


            for (int i = 0; i <= count; i++)
			{
				Bone bind = bindPose[_chain[i].boneName];

                BoneTransform childTransform = parentTransform + bind.local;

                Quaternion rot = Quaternion.AngleAxis(angle, axis.left) * childTransform.rotation;
				rot = Quaternion.Inverse(parentTransform.rotation) * rot;

				pose.SetBoneLocal(_chain[i].boneName, rot);
				parentTransform += new BoneTransform(bind.local.position, rot);
			}*/
		}
    }
}
