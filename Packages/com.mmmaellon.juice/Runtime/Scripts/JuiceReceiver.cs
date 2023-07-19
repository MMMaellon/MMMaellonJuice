
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.Juice
{
    public class JuiceReceiver : UdonSharpBehaviour
    {
        public JuiceContainer waterSource;

        JuicePour otherPour;
        public void OnParticleCollision(GameObject other)
        {
            if (!Utilities.IsValid(other) || !Networking.LocalPlayer.IsOwner(waterSource.gameObject))
            {
                return;
            }
            otherPour = other.GetComponentInParent<JuicePour>();
            if (!Utilities.IsValid(otherPour) || otherPour.waterSource == waterSource)
            {
                return;
            }
            waterSource.ChangeJuiceAmount(Mathf.Max(0.1f, otherPour.pourRate * otherPour.power));
            waterSource.ChangeJuiceColor(otherPour.color);
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