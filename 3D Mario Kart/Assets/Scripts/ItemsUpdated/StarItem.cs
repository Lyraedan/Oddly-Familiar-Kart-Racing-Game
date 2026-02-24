using System.Collections;
using UnityEngine;

public class StarItem : ItemBase
{
    [Header("Star Settings")]
    public float duration = 7.5f;
    public Material starMaterial;
    public GameObject starParticles;
    public AudioSource starMusic;

    private Renderer[] playerRenderers;
    private Material[] originalMaterials;
    private AudioSource courseMusic;
    private AudioSource courseMusicParent;

    public override void Initialize(Player p, ItemManager manager)
    {
        base.Initialize(p, manager);

        // TODO WIRE IN
        //playerRenderers = manager.playerRenderers;
        //originalMaterials = manager.normalMaterials;

        // Cache references ONCE (no GameObject.Find every time)
        GameObject courseMusicObj = GameObject.FindGameObjectWithTag("CourseMusic");
        courseMusic = courseMusicObj.GetComponent<AudioSource>();
        courseMusicParent = courseMusicObj.transform.parent.GetComponent<AudioSource>();
    }

    public override void Use(bool forward)
    {
        itemManager.StartCoroutine(StarRoutine());
    }

    private IEnumerator StarRoutine()
    {
        itemManager.ConsumeItem();

        float originalVol1 = courseMusic.volume;
        float originalVol2 = courseMusicParent.volume;

        //player.StarPowerUp = true;

        // Apply star material
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            playerRenderers[i].material = starMaterial;
        }

        // Mute course music
        courseMusic.volume = 0;
        courseMusicParent.volume = 0;

        // Play star music
        starMusic.Play();

        // Play particles
        for (int i = 0; i < starParticles.transform.childCount; i++)
        {
            starParticles.transform.GetChild(i)
                .GetComponent<ParticleSystem>()
                .Play();
        }

        if (player.GetComponent<PlayerSounds>().CanPlayCharacterSound())
        {
            player.GetComponent<PlayerSounds>().PlayStar();
        }

        yield return new WaitForSeconds(duration);

        // Restore music
        courseMusic.volume = originalVol1;
        courseMusicParent.volume = originalVol2;
        starMusic.Stop();

        // Restore materials
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            playerRenderers[i].material = originalMaterials[i];
        }

        // Stop particles
        for (int i = 0; i < starParticles.transform.childCount; i++)
        {
            starParticles.transform.GetChild(i)
                .GetComponent<ParticleSystem>()
                .Stop();
        }

        //player.StarPowerUp = false;
    }
}