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

}
