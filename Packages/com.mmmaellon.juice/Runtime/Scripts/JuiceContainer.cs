
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.Juice
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class JuiceContainer : UdonSharpBehaviour
    {
        public MeshRenderer juiceMesh;
        Material juiceMat;
        [HideInInspector]
        public JuicePour[] pours;
        public float syncCooldown = 0.25f;
        public Animator containerAnimator;
        public string animatorParameter = "juice";
        public bool optimizeAnimator = true;

        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(color))]
        public Color _color = new Color(1, 1, 1, 0);
        public Color color
        {
            get => _color;
            set
            {
                if (_color == value)
                {
                    return;
                }
                _color = value;
                colorloop = true;
                if (Networking.LocalPlayer.IsOwner(gameObject))
                {
                    RequestSerialization();
                }
            }
        }
        [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(juiceAmount))]
        public float _juiceAmount = 0f;
        public float juiceAmount
        {
            get => _juiceAmount;
            set
            {
                if (value <= 0 && _juiceAmount > 0)
                {
                    foreach (JuicePour pour in pours)
                    {
                        pour.hasLiquid = false;
                    }
                }
                else if (value > 0 && _juiceAmount <= 0)
                {
                    foreach (JuicePour pour in pours)
                    {
                        pour.hasLiquid = true;
                    }
                }
                if (Networking.LocalPlayer.IsOwner(gameObject) && ((value <= 0 && _juiceAmount > 0) || (value >= maxJuice && _juiceAmount < maxJuice) || lastSerialize + syncCooldown < Time.timeSinceLevelLoad))
                {
                    RequestSerialization();
                }
                _juiceAmount = value;
                loop = true;
                if (value <= 0)
                {
                    _juiceAmount = 0;
                }
                if (Utilities.IsValid(juiceMesh))
                {
                    juiceMesh.enabled = _juiceAmount > 0;
                }
            }
        }

        float lastSerialize = -1001f;
        public override void OnPreSerialization()
        {
            lastSerialize = Time.timeSinceLevelLoad;
        }


        public float maxJuice = 500f;
        float startingJuice;
        void Start()
        {
            if (Utilities.IsValid(containerAnimator))
            {
                bool parameterMatch = false;
                foreach (var parameter in containerAnimator.parameters)
                {
                    if (parameter.name == animatorParameter && parameter.type == AnimatorControllerParameterType.Float)
                    {
                        parameterMatch = true;
                        break;
                    }
                }
                if (!parameterMatch)
                {
                    containerAnimator = null;
                }
                if (optimizeAnimator)
                {
                    containerAnimator.enabled = false;
                }
            }
            if (Utilities.IsValid(juiceMesh))
            {
                juiceMat = juiceMesh.material;
            }
            color = color;
            colorValue = color;
            //jank fix
            startingJuice = juiceAmount;
            juiceAmount = maxJuice;
            juiceAmount = startingJuice;
        }

        public void ChangeJuiceAmount(float amount)
        {
            juiceAmount += amount;
        }
        public void ChangeJuiceColor(Color newColor)
        {
            if (juiceAmount <= 1f)
            {
                color = newColor;
            }
            else if (Mathf.Approximately(color.r, newColor.r) && Mathf.Approximately(color.g, newColor.g) && Mathf.Approximately(color.b, newColor.b))
            {
                color = newColor;
            }
            else if (lastSerialize + syncCooldown < Time.timeSinceLevelLoad)
            {
                color = Color.Lerp(color, newColor, Mathf.Lerp(0.25f, 0.05f, juiceAmount / maxJuice));
            }
        }

        [FieldChangeCallback(nameof(loop))]
        bool _loop;
        public bool loop
        {
            get => _loop;
            set
            {
                if (_loop != value)
                {
                    _loop = value;
                    if (value)
                    {
                        Loop();
                    }

                    if (optimizeAnimator && Utilities.IsValid(containerAnimator))
                    {
                        containerAnimator.enabled = value;
                    }
                }
            }
        }

        float animatorValue;
        public void Loop()
        {
            if (!loop)
            {
                return;
            }
            if (!Utilities.IsValid(containerAnimator))
            {
                loop = false;
                return;
            }
            animatorValue = juiceAmount / maxJuice;
            if (Mathf.Approximately(containerAnimator.GetFloat(animatorParameter), animatorValue))
            {
                containerAnimator.SetFloat(animatorParameter, animatorValue);
                loop = false;
            }
            else
            {
                containerAnimator.SetFloat(animatorParameter, Mathf.Lerp(containerAnimator.GetFloat(animatorParameter), animatorValue, 0.1f));
                SendCustomEventDelayedFrames(nameof(Loop), 1);
            }
        }


        [FieldChangeCallback(nameof(colorloop))]
        bool _colorloop;
        public bool colorloop
        {
            get => _colorloop;
            set
            {
                if (_colorloop != value)
                {
                    _colorloop = value;
                    if (value)
                    {
                        colorLoop();
                    }
                }
            }
        }

        Color colorValue;
        public void colorLoop()
        {
            if (!colorloop)
            {
                return;
            }
            colorValue = Color.Lerp(colorValue, color, 0.1f);

            if (Mathf.Approximately(color.r, colorValue.r) && Mathf.Approximately(color.g, colorValue.g) && Mathf.Approximately(color.b, colorValue.b))
            {
                foreach (JuicePour pour in pours)
                {
                    pour.color = color;
                }
                if (Utilities.IsValid(juiceMat))
                {
                    juiceMat.color = color;
                }
                colorloop = false;
            }
            else
            {
                foreach (JuicePour pour in pours)
                {
                    pour.color = colorValue;
                }
                if (Utilities.IsValid(juiceMat))
                {
                    juiceMat.color = colorValue;
                }
                SendCustomEventDelayedFrames(nameof(colorLoop), 1);
            }
        }
    }
}
