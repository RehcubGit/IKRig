using UnityEngine;

namespace Rehcub 
{
    [ExecuteInEditMode]
    public class CopyTransform : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private bool _x = true;
        [SerializeField] private bool _y = true;
        [SerializeField] private bool _z = true;

        [SerializeField] private bool _copyRotation;

        [SerializeField] private Vector3 _offset;


        private Transform _transform;


        private void Start()
        {
            _transform = transform;
        }

        private void Update()
        {
        }

        public void ApplyTransform()
        {
            Vector3 ownerPosition = _transform.position;
            Vector3 targetPosition = _target.position;

            targetPosition.x = _x ? targetPosition.x + _offset.x : ownerPosition.x;
            targetPosition.y = _y ? targetPosition.y + _offset.y : ownerPosition.y;
            targetPosition.z = _z ? targetPosition.z + _offset.z : ownerPosition.z;

            Quaternion targetRotation = _copyRotation ? _target.rotation : _transform.rotation;


            _transform.SetPositionAndRotation(targetPosition, targetRotation);
        }
    }
}
