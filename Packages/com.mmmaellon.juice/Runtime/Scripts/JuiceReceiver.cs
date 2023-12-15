
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.Juice
{
    public class JuiceReceiver : UdonSharpBehaviour
    {
        public JuiceContainer waterSource;
        public bool accuratePour = true;
        public float loopDelay = 0.5f;
        JuicePour otherPour;
        float startAmount;
        float otherStartAmount;
        JuicePour newPour;
        float lastParticle = -1001f;
        public void OnParticleCollision(GameObject other)
        {
            if (!Utilities.IsValid(other) || !Networking.LocalPlayer.IsOwner(waterSource.gameObject))
            {
                return;
            }
            newPour = other.GetComponentInParent<JuicePour>();
            if (!Utilities.IsValid(newPour) || newPour.waterSource == waterSource)
            {
                return;
            }
            lastParticle = Time.timeSinceLevelLoad;
            if (waterSource)
            {
                waterSource.ChangeJuiceColor(newPour.color);
                if (newPour.waterSource && accuratePour)
                {
                    if (!loop && newPour != otherPour)
                    {
                        otherPour = newPour;
                        startAmount = waterSource.juiceAmount;
                        otherStartAmount = otherPour.waterSource.juiceAmount;
                    }
                    loop = true;
                }
                else
                {
                    waterSource.ChangeJuiceAmount(newPour.pourRate * newPour.powerSquare);
                }
            }
        }

        JuiceSourceTrigger otherWater;
        public virtual void OnTriggerEnter(Collider other)
        {
            if (!Utilities.IsValid(other) || !Networking.LocalPlayer.IsOwner(waterSource.gameObject))
            {
                return;
            }

            otherWater = other.GetComponent<JuiceSourceTrigger>();
            if (!Utilities.IsValid(otherWater))
            {
                return;
            }

            waterSource.juiceAmount = waterSource.maxJuice;
            waterSource.color = otherWater.color;
        }

        int last_loop = -1001;
        [System.NonSerialized]
        public bool _loop = false;
        public bool loop
        {
            get => _loop;
            set
            {
                if (value != _loop)
                {
                    _loop = value;
                    if (value && last_loop < Time.renderedFrameCount)
                    {
                        last_loop = Time.renderedFrameCount;
                        SendCustomEventDelayedFrames(nameof(Loop), 1);
                    }
                }
            }
        }
        public void Loop()
        {
            if (!otherPour || !otherPour.waterSource || lastParticle + loopDelay < Time.timeSinceLevelLoad)
            {
                loop = false;
                return;
            }
            if (loop && last_loop < Time.timeSinceLevelLoad)
            {
                last_loop = Time.renderedFrameCount;
                SendCustomEventDelayedFrames(nameof(Loop), 1);
            }

            waterSource.juiceAmount = startAmount + (otherStartAmount - otherPour.waterSource.juiceAmount);
        }
    }
}
