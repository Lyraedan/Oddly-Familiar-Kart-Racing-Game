using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class RacerConfig : MonoBehaviour
{
    public Animator Driver;

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

}
