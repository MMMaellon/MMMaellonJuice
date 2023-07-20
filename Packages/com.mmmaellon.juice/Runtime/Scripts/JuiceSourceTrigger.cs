
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
            get => Utilities.IsValid(altColorSource) ? altColorSource.color : _color;
            set
            {
                if (Utilities.IsValid(altColorSource))
                {
                    altColorSource.color = value;
                } else
                {
                    _color = value;
                    if (!startRan)
                    {
                        return;
                    }
                    mat.color = value;
                }
            }
        }

        public JuiceContainer altColorSource;

        bool startRan = false;
        void Start()
        {
            mat = mesh.material;
            startRan = true;
            color = color;
        }
    }
}