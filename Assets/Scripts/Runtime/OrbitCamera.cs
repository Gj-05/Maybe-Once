using UnityEngine;
using UnityEngine.InputSystem;

namespace OctoEarth
{
    [DefaultExecutionOrder(100)]
    public sealed class OrbitCamera : MonoBehaviour
    {
        public Vector3 focus;
        public float distance = 180, yaw = 0, pitch = 65;
        public bool localView;
        public bool easeMotion;
        public void FrameSolarSystem() { localView=false; focus=new Vector3(-34,0,0); distance=180; yaw=0; pitch=65; }
        public void FrameEarthMoon() { localView=true; focus=new Vector3(-4,0,0); distance=28; yaw=0; pitch=65; }
        void LateUpdate()
        {
            var mouse = Mouse.current;
            // Dashboard occupies the left edge; gestures there belong to UI.
            if (mouse != null && mouse.position.ReadValue().x > 430 && mouse.position.ReadValue().x < Screen.width-340)
            {
                var delta = mouse.delta.ReadValue();
                if (mouse.rightButton.isPressed) { yaw += delta.x*0.2f; pitch = Mathf.Clamp(pitch-delta.y*0.2f,5,89); }
                if (mouse.middleButton.isPressed) focus += (-transform.right*delta.x-transform.up*delta.y)*(distance*0.001f);
                distance = Mathf.Clamp(distance*Mathf.Exp(-mouse.scroll.ReadValue().y*0.001f),localView?2:8,localView?80:400);
            }
            var rotation = Quaternion.Euler(pitch,yaw,0);
            var camera=GetComponent<Camera>();
            camera.nearClipPlane=localView?0.005f:0.1f;
            camera.farClipPlane=localView?200:1000;
            var target=focus-rotation*Vector3.forward*distance;
            if(easeMotion) {float alpha=1-Mathf.Exp(-Time.unscaledDeltaTime*2); transform.SetPositionAndRotation(Vector3.Lerp(transform.position,target,alpha),Quaternion.Slerp(transform.rotation,rotation,alpha));}
            else transform.SetPositionAndRotation(target,rotation);
        }
    }
}
