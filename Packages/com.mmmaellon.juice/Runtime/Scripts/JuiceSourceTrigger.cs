
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.Juice
{
    public class JuiceSourceTrigger : UdonSharpBehaviour
    {
        public MeshRenderer mesh;
        public Color _color = new Color(1,1,1,0);
        Material mat;
        public Color color
        {
            get => _color;
            set
            {
                _color = value;
                if (!startRan)
                {
                    return;
                }
                mat.color = value;
                mat.SetFloat("_Metallic", value.a);
            }
        }

        bool startRan = false;
        void Start()
        {
            mat = mesh.material;
            startRan = true;
            color = color;
        }
    }
}