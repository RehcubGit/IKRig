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
        [SerializeField] private Vector3 _rotationOffset;


        private Transform _transform;


        private void Start()
        {
            _transform = transform;
        }

        private void Update()
        {
        }

        private void LateUpdate()
        {
            ApplyTransform();
        }

        public void ApplyTransform()
        {
            Vector3 ownerPosition = _transform.position;
            Vector3 targetPosition = _target.position;

            Vector3 offset = _target.rotation * _offset;

            targetPosition.x = _x ? targetPosition.x + offset.x : ownerPosition.x;
            targetPosition.y = _y ? targetPosition.y + offset.y : ownerPosition.y;
            targetPosition.z = _z ? targetPosition.z + offset.z : ownerPosition.z;

            Quaternion targetRotation = _transform.rotation;
            if (_copyRotation)
            {
                targetRotation = _target.rotation * Quaternion.Euler(_rotationOffset);
            }


            _transform.SetPositionAndRotation(targetPosition, targetRotation);
        }
    }
}
