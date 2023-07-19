
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.Juice
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class JuicePourSync : UdonSharpBehaviour
    {
        public JuicePour pour;

        [UdonSynced, FieldChangeCallback(nameof(valve))]
        public float _syncedValve = 0;
        public float valve
        {
            get => pour.power;
            set
            {
                _syncedValve = value;
                loop = true;
                if (value > 0)
                {
                    pour.hasLiquid = true;
                    pour.TurnOn();
                    pour.MoveWaterDown();
                }
                if (Networking.LocalPlayer.IsOwner(gameObject))
                {
                    RequestSerialization();
                }
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
                    if (value && loop)
                    {
                        Loop();
                    }
                }
            }
        }

        public void Loop()
        {
            Debug.LogWarning("Loop");
            if (!loop)
            {
                return;
            }

            if (Mathf.Approximately(pour.power, _syncedValve) || pour.power <= 0.1f)
            {
                pour.power = _syncedValve;
                if (Mathf.Approximately(0f, _syncedValve))
                {
                    pour.hasLiquid = false;
                    pour.power = 0;
                    pour.TurnOff();
                }
                loop = false;
            }
            else
            {
                pour.power = Mathf.Lerp(pour.power, _syncedValve, 0.1f);
                SendCustomEventDelayedFrames(nameof(Loop), 1);
            }
            pour.MoveWaterDown();
        }

        public void TurnOn()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            valve = 1f;
        }
        public void TurnOff()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            valve = 0f;
        }
        public void Toggle()
        {
            if (_syncedValve > 0)
            {
                TurnOff();
            }
            else
            {
                TurnOn();
            }
        }
        public void SetValve(float value)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            valve = value;
        }

        public void Start()
        {
            valve = _syncedValve;
            pour.enabled = false;
        }
    }
}