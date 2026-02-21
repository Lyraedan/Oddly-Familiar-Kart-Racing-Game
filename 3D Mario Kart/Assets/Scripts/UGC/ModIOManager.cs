using UnityEngine;
using Modio;
using Modio.Authentication;
using Modio.Users;
using System.Threading.Tasks;

public class ModIOManager : MonoBehaviour
{
    public static ModIOManager Instance;

    // These are set by your UI system externally
    public string Email;
    public string Code;

    bool codeSubmitted;

    private ModioEmailAuthService modioEmailAuthService;

    void Awake()
    {
        Instance = this;

        // Initialize ModioEmailAuthService instance
        modioEmailAuthService = new ModioEmailAuthService();

        // Register code prompter
        modioEmailAuthService.SetCodePrompter(GetAuthCode);
    }

    public async Task<bool> Initialize()
    {
        Error error = await ModioClient.Init();

        if (error)
        {
            Debug.LogError("Mod.io init failed: " + error);
            return false;
        }

        Debug.Log("Mod.io initialized");
        return true;
    }

    public async Task<bool> Authenticate()
    {
        if (string.IsNullOrEmpty(Email))
        {
            Debug.LogError("Email is empty");
            return false;
        }

        Error error = await ModioClient.AuthService.Authenticate(true, Email);

        if (error)
        {
            Debug.LogError("Authentication failed: " + error);
            return false;
        }

        Debug.Log("Authenticated as: " + User.Current.Profile.Username);
        return true;
    }

    async Task<string> GetAuthCode()
    {
        Debug.Log("Waiting for auth code...");

        codeSubmitted = false;

        while (!codeSubmitted)
            await Task.Yield();

        return Code;
    }

    public void SubmitCode(string code)
    {
        Code = code;
        codeSubmitted = true;
    }
}
