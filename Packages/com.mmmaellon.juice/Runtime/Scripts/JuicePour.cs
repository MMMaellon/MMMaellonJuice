
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MMMaellon.Juice
{
    public class JuicePour : UdonSharpBehaviour
    {
        public ParticleSystem particles;
        public ParticleSystem splashParticles;
        public AudioSource audioSource;
        [System.NonSerialized]
        ParticleSystem.MainModule main;
        [System.NonSerialized]
        ParticleSystem.TrailModule trail;
        [System.NonSerialized]
        ParticleSystem.ShapeModule shape;
        [System.NonSerialized]
        ParticleSystem.CollisionModule collision;
        Material pourMaterial;
        Material splashMaterial;
        public float _minAngle = 30f;
        public float _maxAngle = 120f;
        public float pourRate = 5f;
        public float minimumPourAmount = 0.1f;
        public float pourAngleShifting = 60f;
        public Collider[] receiverColliders;
        [System.NonSerialized]
        public float _power = 0;
        ParticleSystem.MinMaxCurve startSize;
        ParticleSystem.MinMaxCurve startSpeed;
        ParticleSystem.MinMaxCurve newStartSize;
        ParticleSystem.MinMaxCurve newStartSpeed;
        bool startRan = false;
        public JuiceContainer waterSource;
        Vector3 splashStartScale;

        public float minAngle
        {
            get
            {
                if (Utilities.IsValid(waterSource))
                {
                    return Mathf.Lerp(pourAngleShifting + _minAngle, _minAngle, waterSource.juiceAmount / waterSource.maxJuice);
                }
                return _minAngle;
            }
            set
            {
                _minAngle = value;
            }
        }
        public float maxAngle
        {
            get
            {
                if (Utilities.IsValid(waterSource))
                {
                    return Mathf.Lerp(pourAngleShifting + _maxAngle, _maxAngle, waterSource.juiceAmount / waterSource.maxJuice);
                }
                return _maxAngle;
            }
            set
            {
                _maxAngle = value;
            }
        }
        // float powerRoot;
        float powerSquare;
        public float power
        {
            get => _power;
            set
            {
                _power = value;
                if (!startRan)
                {
                    return;
                }
                powerSquare = Mathf.Max(minimumPourAmount, Mathf.Pow(value, 2));
                if (value < minimumPourAmount && value > 0f)
                {
                    newStartSize = main.startSize;
                    newStartSize.constant = startSize.constant * powerSquare;
                    newStartSize.constantMax = startSize.constantMax * powerSquare;
                    newStartSize.constantMin = startSize.constantMin * powerSquare;
                    main.startSize = newStartSize;
                    splashParticles.transform.localScale = Vector3.Lerp(splashStartScale * minimumPourAmount, splashStartScale, powerSquare);

                    newStartSpeed = main.startSpeed;
                    newStartSpeed.constant = startSpeed.constant * minimumPourAmount;
                    newStartSpeed.constantMax = startSpeed.constantMax * minimumPourAmount;
                    newStartSpeed.constantMin = startSpeed.constantMin * minimumPourAmount;
                    main.startSpeed = newStartSpeed;

                    audioSource.volume = minimumPourAmount;
                } else
                {
                    newStartSize = main.startSize;
                    newStartSize.constant = startSize.constant * powerSquare;
                    newStartSize.constantMax = startSize.constantMax * powerSquare;
                    newStartSize.constantMin = startSize.constantMin * powerSquare;
                    main.startSize = newStartSize;
                    splashParticles.transform.localScale = Vector3.Lerp(splashStartScale * minimumPourAmount, splashStartScale, powerSquare);

                    newStartSpeed = main.startSpeed;
                    newStartSpeed.constant = startSpeed.constant * value;
                    newStartSpeed.constantMax = startSpeed.constantMax * value;
                    newStartSpeed.constantMin = startSpeed.constantMin * value;
                    main.startSpeed = newStartSpeed;

                    audioSource.volume = value;
                }

            }
        }

        public Color _color = new Color(1,1,1,0);
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
                pourMaterial.color = value;
                splashMaterial.color = value;
            }
        }

        public bool hasLiquid = true;
        float newAngle;
        VRCPlayerApi localPlayer;
        ParticleSystemRenderer render;
        ParticleSystemRenderer splashRender;
        Vector3 startLocalPos;
        void Start()
        {
            main = particles.main;
            shape = particles.shape;
            trail = particles.trails;
            collision = particles.collision;
            startSize = main.startSize;
            startSpeed = main.startSpeed;
            splashStartScale = splashParticles.transform.localScale;
            localPlayer = Networking.LocalPlayer;
            render = particles.GetComponent<ParticleSystemRenderer>();
            splashRender = splashParticles.GetComponent<ParticleSystemRenderer>();
            render.material = render.trailMaterial;
            pourMaterial = render.material;
            render.trailMaterial = pourMaterial;
            splashMaterial = splashRender.material;
            startLocalPos = particles.transform.localPosition;
            prevKillSpeed = collision.minKillSpeed;

            foreach (Collider col in receiverColliders)
            {
                col.enabled = true;
            }

            startRan = true;
            power = power;
            color = color;
        }

        Vector3 projectedDown;
        bool overflow;
        bool tipped;
        public void Update()
        {
            Loop();
        }
        bool overflowChanged;
        bool tippedChanged;
        public void Loop()
        {
            if (!startRan)
            {
                return;
            }
            newAngle = Vector3.Angle(particles.transform.forward, Vector3.up);
            overflowChanged = overflow != (Utilities.IsValid(waterSource) && waterSource.juiceAmount > waterSource.maxJuice);
            tippedChanged = tipped != (newAngle > minAngle);
            if (overflowChanged)
            {
                overflow = !overflow;
            }
            if (tippedChanged)
            {
                tipped = !tipped;
                if (hasLiquid)
                {
                    foreach (Collider col in receiverColliders)
                    {
                        col.enabled = !tipped;
                    }
                }
            }
            if (hasLiquid && (tipped || overflow))
            {
                if (!particles.isPlaying)
                {
                    TurnOn();
                }
                if (newAngle < maxAngle)
                {
                    if (maxAngle <= minAngle)
                    {
                        power = 1;
                    }
                    else if (overflow)
                    {
                        power = Mathf.Max((newAngle - minAngle) / (maxAngle - minAngle), (waterSource.juiceAmount - waterSource.maxJuice) / waterSource.maxJuice);
                    }
                    else
                    {
                        power = (newAngle - minAngle) / (maxAngle - minAngle);
                    }
                }
                else
                {
                    power = 1;
                }
                MoveWaterDown();
                if (Utilities.IsValid(waterSource) && localPlayer.IsOwner(waterSource.gameObject))
                {
                    if (overflow)
                    {
                        waterSource.ChangeJuiceAmount(-pourRate * Mathf.Max(minimumPourAmount, powerSquare) * 2);
                    }
                    else
                    {
                        waterSource.ChangeJuiceAmount(-pourRate * Mathf.Max(minimumPourAmount, powerSquare));
                    }
                    waterSource.ChangeJuiceColor(color);
                }
            }
            else
            {
                if (particles.isPlaying)
                {
                    TurnOff();
                }
            }
        }

        public void MoveWaterDown()
        {
            projectedDown = particles.transform.InverseTransformDirection(Vector3.ProjectOnPlane(Vector3.down, particles.transform.forward));
            projectedDown = (particles.transform.localRotation * projectedDown.normalized * (1 - power) / 40);//hardcoded
            projectedDown.x = particles.transform.localScale.x != 0 ? projectedDown.x / particles.transform.localScale.x : projectedDown.x;
            projectedDown.y = particles.transform.localScale.y != 0 ? projectedDown.y / particles.transform.localScale.y : projectedDown.y;
            projectedDown.z = particles.transform.localScale.z != 0 ? projectedDown.z / particles.transform.localScale.z : projectedDown.z;
            particles.transform.localPosition = startLocalPos + projectedDown;
        }

        public void TurnOn()
        {
            if (!startRan)
            {
                return;
            }
            if (tipped)
            {
                foreach (Collider col in receiverColliders)
                {
                    col.enabled = false;
                }
            }
            trail.attachRibbonsToTransform = true;
            audioSource.SetScheduledEndTime(0);
            audioSource.Play();
            particles.Play();
        }

        public void TurnOff()
        {
            if (!startRan)
            {
                return;
            }
            trail.attachRibbonsToTransform = false;
            audioSource.SetScheduledEndTime(AudioSettings.dspTime + 0.2f);//let audio play for a little longer
            particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            foreach (Collider col in receiverColliders)
            {
                col.enabled = true;
            }
        }

        public bool _instakillParticles;
        float prevKillSpeed;
        public bool instakillParticles
        {
            get => _instakillParticles;
            set
            {
                if (_instakillParticles == value || !startRan)
                {
                    return;
                }
                _instakillParticles = value;
                if (value)
                {
                    collision.minKillSpeed = 1000;
                }
                else
                {
                    collision.minKillSpeed = prevKillSpeed;
                }
            }
        }

        JuiceReceiver otherReciever;


        public void OnParticleCollision(GameObject other)
        {
            if (!Utilities.IsValid(other))
            {
                return;
            }
            instakillParticles = other.layer == gameObject.layer;
        }
    }
}