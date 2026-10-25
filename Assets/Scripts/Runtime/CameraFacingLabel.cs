using UnityEngine;

namespace OctoEarth
{
    public sealed class CameraFacingLabel : MonoBehaviour
    {
        void LateUpdate() { if (Camera.main != null) transform.rotation = Camera.main.transform.rotation; }
    }
}
