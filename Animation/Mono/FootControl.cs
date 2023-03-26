using UnityEngine;

namespace Rehcub 
{
    public class FootControl : MonoBehaviour, IEndEffector
    {
        [SerializeField] private IKRig _rig;
        [SerializeField] private SourceSide _targetSide;

        [HideInInspector]
        [SerializeField] private Chain _chain;

        private void OnEnable()
        {
            if (_rig == null)
                return;

            _chain = GetChain();
        }

        private void OnValidate()
        {
            if (_rig == null)
                return;

            _chain = GetChain();
        }


        public void Apply()
        {
            if (enabled == false)
                return;

            Bone foot = _chain.Last();
            Bone toe = GetChildBone(foot.boneName);
            Bone toeEnd = GetChildBone(toe.boneName);

            //CalculateFoot(_rig.Armature.currentPose, foot, toe);
            CalculateToes(_rig.Armature.currentPose, toe, toeEnd);
        }

        public BoneTransform AqustTarget(Vector3 start, BoneTransform target)
        {
            Bone foot = _chain.Last();
            BoneTransform bind = _rig.Armature.bindPose.GetModelTransform(foot);
            float footGroundHeight = bind.position.y;

            float footLength = _rig.Armature.bindPose.GetLength(foot);
            Axis axis = _rig.Armature.bindPose.GetAxis(foot);
            axis.Rotate(bind.rotation);

            Vector3 direction = target.position - start;
            float distance = direction.magnitude + footGroundHeight;
            direction.Normalize();

            bool gotHit = Physics.Raycast(start, direction, out RaycastHit hit, distance * 2f);

            if (gotHit == false)
                return target;

            float angle = Vector3.SignedAngle(hit.normal, -direction, target.left);
            angle = Mathf.PI * 0.5f - angle * Mathf.Deg2Rad;
            float hypotenuseLength = footGroundHeight / Mathf.Sin(angle);
            Vector3 pointOnRay = hit.point - direction * hypotenuseLength;

            //Get the Vec3 from the hit point to the chain target and check if it is underneath the surface
            if (Vector3.Dot(start + direction * distance - hit.point, hit.normal) < 0f)
            {
                target.position = pointOnRay;

                Quaternion q = Quaternion.FromToRotation(Vector3.up, hit.normal);
                target.rotation = q * axis.GetRotation();
                return target;
            }

            Vector3 forward = target.forward;
            Vector3 toePosition = target.position + forward * footLength;

            Physics.Raycast(toePosition + Vector3.up, Vector3.down, out hit, 2f);
            
            if (Vector3.Dot(toePosition - hit.point, hit.normal) > 0f)
                return target;

            float a = Vector3.Project(target.position - hit.point, hit.normal).magnitude;

            //float a = target.position.y - groundHeight;
            float c = footLength;
            float b = Mathf.Sqrt(c * c - a * a);

            Vector3 toeLocal = toePosition - target.position;
            Vector3 newToeLocal = Quaternion.FromToRotation(Vector3.up, hit.normal) * new Vector3(toeLocal.x, -a, b);

            Quaternion rot = Quaternion.FromToRotation(toeLocal.normalized, newToeLocal.normalized);
            target.rotation = rot * target.rotation;
            return target;
        }

        private void CalculateToes(Pose pose, Bone toe, Bone toeEnd)
        {
            if (toe == null)
                return;

            BoneTransform toeModel = pose.GetModelTransform(toe);
            float length = pose.GetLength(toe.boneName);

            Vector3 target = toeModel.TransformPoint(toe.axis.forward * length);
            if (toeEnd != null)
            {
                target = pose.GetModelTransform(toeEnd).position;
            }

            Physics.Raycast(target + Vector3.up, Vector3.down, out RaycastHit hit, 1f);
            float groundHeight = hit.point.y;

            if (target.y >= groundHeight)
                return;

            Axis axis = new Axis(hit.point - toeModel.position, toeModel.rotation * toe.axis.up);
            toe.Solve(_rig.Armature.bindPose, _rig.Armature.currentPose, axis);
        }

        private Chain GetChain()
        {
            Chain[] chains = _rig.Armature.GetChains(SourceChain.LEG, _targetSide);
            if (chains.Length == 0)
            {
                Debug.LogError($"No {_targetSide} Leg found in armature");
                enabled = false;
                return null;
            }

            Debug.Log(chains.Length);
            Debug.Log(chains.First().count);
            return chains.First();
        }

        private Bone GetChildBone(string boneName)
        {
            Bone[] bones = _rig.Armature.GetChildren(boneName);
            if (bones.Length == 0)
                return null;

            return bones.First();
        }
    }
}
