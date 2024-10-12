
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
        public float pourCooldown = 0.5f;
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
            if (!Utilities.IsValid(newPour) || newPour.waterSource == waterSource || !Utilities.IsValid(waterSource))
            {
                return;
            }
            waterSource.ChangeJuiceColor(newPour.color);
            if (accuratePour)
            {
                if (lastParticle + pourCooldown > Time.timeSinceLevelLoad || !Utilities.IsValid(otherPour) || otherPour != newPour)
                {
                    //hasn't had a collision in a while. Treat this as a new pour
                    otherPour = newPour;
                    startAmount = waterSource.juiceAmount;
                    otherStartAmount = otherPour.waterSource.juiceAmount;
                    waterSource.ChangeJuiceAmount(newPour.pourRate * newPour.powerSquare);
                }
                else
                {
                    waterSource.juiceAmount = startAmount + (otherStartAmount - otherPour.waterSource.juiceAmount);
                }
            }
            else
            {
                waterSource.ChangeJuiceAmount(newPour.pourRate * newPour.powerSquare);
            }
            lastParticle = Time.timeSinceLevelLoad;
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

    }
}
