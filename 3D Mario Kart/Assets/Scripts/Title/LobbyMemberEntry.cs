using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMemberEntry : MonoBehaviour
{
    public string PlayerName;
    public bool IsHost;
    public ulong SteamId;

    [Header("UI Element")]
    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private Image avatar;

    public void UpdateEntry(string playerName, ulong steamId, bool isHost)
    {
        PlayerName = playerName;
        SteamId = steamId;
        IsHost = isHost;

        PlayerNameText.text = isHost ? $"{playerName} (Host)" : playerName;
    }

    public void SetAvatar(Sprite avatarSprite)
    {
        avatar.sprite = avatarSprite;
    }

    public void RequestAvatar(CSteamID steamId)
    {
        Sprite avatarSprite = GetSteamAvatar(steamId);
        if (avatarSprite != null)
        {
            SetAvatar(avatarSprite);
        }
    }

    private Sprite GetSteamAvatar(CSteamID steamId)
    {
        int imageId = SteamFriends.GetLargeFriendAvatar(steamId);

        if (imageId == -1)
            return null;

        uint width, height;
        if (!SteamUtils.GetImageSize(imageId, out width, out height))
            return null;

        byte[] image = new byte[width * height * 4];

        if (!SteamUtils.GetImageRGBA(imageId, image, image.Length))
            return null;

        Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);

        // Flip vertically
        byte[] flipped = new byte[image.Length];
        int rowSize = (int)width * 4;

        for (int y = 0; y < height; y++)
        {
            Buffer.BlockCopy(
                image,
                y * rowSize,
                flipped,
                ((int)height - y - 1) * rowSize,
                rowSize
            );
        }

        texture.LoadRawTextureData(flipped);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }
}
