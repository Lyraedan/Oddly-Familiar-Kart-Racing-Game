using UnityEngine;
using Modio;
using Modio.Mods;
using Modio.Users;
using System.Threading.Tasks;
using System.Linq;

public class ModIOManager : MonoBehaviour
{
    public static ModIOManager Instance;

    Mod[] availableMods;
    Mod currentDownload;

    float progressTimer;

    public string MODIO_EMAIL = "user@email.com";

    void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        await Initialize();
        await Authenticate();
        await FetchMods();

        // Start installation management
        ModInstallationManagement.ManagementEvents += OnModManagementEvent;
    }

    async Task Initialize()
    {
        Debug.Log("Initializing Mod.io...");

        Error error = await ModioClient.Init();

        if (error)
        {
            Debug.LogError("Failed to initialize Mod.io: " + error);
            return;
        }

        Debug.Log("Mod.io initialized");
    }

    async Task Authenticate()
    {
        if (User.Current.IsAuthenticated)
        {
            Debug.Log("Already authenticated: " + User.Current.Profile.Username);
            return;
        }

        Debug.Log("Authenticating via email...");

        // Replace with your UI input email
        string email = MODIO_EMAIL;

        Error error = await ModioClient.AuthService.Authenticate(true, email);

        if (error)
        {
            Debug.LogError("Auth failed: " + error);
            return;
        }

        Debug.Log("Authentication successful: " + User.Current.Profile.Username);
    }

    async Task FetchMods()
    {
        Debug.Log("Fetching mods...");

        var (error, page) = await Mod.GetMods(new ModSearchFilter());

        if (error)
        {
            Debug.LogError("Failed to fetch mods: " + error);
            return;
        }

        availableMods = page.Data;

        Debug.Log($"Found {availableMods.Length} mods");

        foreach (var mod in availableMods)
        {
            Debug.Log($"Mod: {mod.Name} (ID: {mod.Id})");
        }
    }

    public async Task SubscribeToMod(Mod mod)
    {
        Debug.Log("Subscribing to: " + mod.Name);

        Error error = await mod.Subscribe();

        if (error)
        {
            Debug.LogError("Subscribe failed: " + error);
            return;
        }

        Debug.Log("Subscribed successfully");
    }

    void OnModManagementEvent(
        Mod mod,
        Modfile modfile,
        ModInstallationManagement.OperationType type,
        ModInstallationManagement.OperationPhase phase)
    {
        Debug.Log($"{type} {phase}: {mod.Name}");

        if (type == ModInstallationManagement.OperationType.Install)
        {
            if (phase == ModInstallationManagement.OperationPhase.Started)
            {
                currentDownload = mod;
            }

            if (phase == ModInstallationManagement.OperationPhase.Completed)
            {
                Debug.Log("Installed at: " + mod.File.InstallLocation);

                LoadMod(mod);

                currentDownload = null;
            }
        }
    }

    void Update()
    {
        if (currentDownload == null)
            return;

        progressTimer -= Time.deltaTime;

        if (progressTimer > 0)
            return;

        progressTimer = 1f;

        float progress = currentDownload.File.FileStateProgress * 100f;

        Debug.Log($"Downloading {currentDownload.Name}: {progress:0}%");
    }

    void LoadMod(Mod mod)
    {
        Debug.Log("Loading mod from: " + mod.File.InstallLocation);

        var bundle = AssetBundle.LoadFromFile(mod.File.InstallLocation);

        if (bundle == null)
        {
            Debug.LogError("Failed to load AssetBundle");
            return;
        }

        Debug.Log("AssetBundle loaded: " + bundle.name);

        // Example: load prefab
        var prefab = bundle.LoadAsset<GameObject>("ModPrefab");

        if (prefab != null)
        {
            Instantiate(prefab);
        }
    }

    // Example helper
    public Mod GetMod(int index)
    {
        if (availableMods == null || index >= availableMods.Length)
            return null;

        return availableMods[index];
    }

    public Mod[] GetAllMods()
    {
        return availableMods;
    }
}