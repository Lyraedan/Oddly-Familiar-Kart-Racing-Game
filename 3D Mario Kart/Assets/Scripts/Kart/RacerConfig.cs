using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class RacerConfig : MonoBehaviour
{
    public Animator Driver;
    public Sprite Emblem;
    public Sprite MinimapIcon;
    public string CharacterName = "Racer";
    public Sprite CharacterIcon;
    public Camera FirstPersonCamera;

    [System.Serializable]
    public class FaceConfig
    {
        public Material DefaultFace;
        public Renderer faceRenderer;
        public List<Material> Faces = new();
    }
    public FaceConfig Face;

    public Transform HeadBone;

    [System.Serializable]
    public class RacerSoundConfig
    {
        public List<AudioSource> Boost = new();
        public List<AudioSource> Star = new();
        public List<AudioSource> JumpTrick = new();
        public AudioSource Glider;
        public AudioSource FirstPlace;
        public AudioSource Lose;
        public List<AudioSource> HurtSounds = new();
    }

    public RacerSoundConfig RacerSounds;

    [Header("Used for Star power up")]
    public List<Renderer> CharacterRenderers = new();

    // This object should only contain a model and nothing else really so this should be fine
    [ContextMenu("Get Character Renderers")]
    public void GetCharacterRenderers()
    {
        CharacterRenderers = new List<Renderer>(
            GetComponentsInChildren<Renderer>().Where(r => r != null && r.enabled && !(r is ParticleSystemRenderer))
        );
    }

}
