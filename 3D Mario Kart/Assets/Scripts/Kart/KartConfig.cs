using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class KartConfig : MonoBehaviour
{

    public ChassisConfig Chassis;

    [System.Serializable]
    public class KartSoundConfig
    {
        public AudioSource KartSound;
        public AudioSource KartIdle;
    }

    public KartSoundConfig Sounds;

    public GameObject boostParticles;
    public GameObject boostBurstParticles;
    public GameObject WheelParticles;

    [System.Serializable]
    public class DustConfig
    {
        public Color Drift1 = new Color(0f, 0.8f, 1f, 1f);
        public Color Drift2 = new Color(1f, 0.72f, 0.05f, 1f);
        public Color Drift3 = new Color(1f, 0.12f, 0.89f, 1f);
        public Transform DustParticles;
        public Transform DriftDustLeft;
        public Transform DriftDustRight;
        public Transform ExhaustDust;
        public Transform AccelBeforeStartDust;
    }

    public DustConfig dust;

    public GameObject Trails;

    [System.Serializable]
    public struct TireConfig
    {
        public GameObject Container;
        public List<GameObject> Mains;
        public List<GameObject> Parents;
        [Header("Used for Anti-Gravity")]
        public List<Renderer> Renderers;
    }
    public TireConfig Tires;

    [System.Serializable]
    public class AntiGravityConfig
    {
        public Color TireColor = new Color(0.27f, 0.6f, 0.9f, 1f);
        public Color LightDecalColor = new Color(0f, 0.68f, 1f, 0.5f);
        public List<Vector3> TirePositions = new List<Vector3>()
        {
            new Vector3(-0.58f, -0.06f, 0.715138f),
            new Vector3(-0.651f, 0.075f, -0.37f),
            new Vector3(0.58f, -0.065f, 0.72f),
            new Vector3(0.65f, 0.07f, -0.37f)
        };
        public List<ParticleSystem> Spin = new();
    }
    public AntiGravityConfig antiGravityConfig;

    [Space(10)]
    public ParticleSystem GroundLandParticles;
    public GameObject SteeringWheel;
    public GameObject Glider;
    public GameObject Propeller;
}
