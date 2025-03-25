using UnityEngine;
using UnityEngine.AddressableAssets;

public class Managers : MonoBehaviour
{
    public static bool Initialized { get; set; } = false;
    public static Managers s_instance;
    public static Managers Instance { get { return s_instance; } }

    private PoolManager _pool = new PoolManager();
    private ResourceManager _resource = new ResourceManager();
    private SoundManager _sound = new SoundManager();
    private DataManager _data = new DataManager();
    public static PoolManager Pool { get { return Instance?._pool; } }
    public static ResourceManager Resource { get { return Instance?._resource; } }
    public static SoundManager Sound { get { return Instance?._sound; } }
    public static DataManager Data { get { return Instance?._data; } }


    private void Awake()
    {
        Application.targetFrameRate = 60;
        Init();
    }

    public static void Init()
    {
        Initialized = true;

        GameObject go = GameObject.Find("@Managers");

        if (go == null)
        {
            go = new GameObject { name = "@Managers" };
            go.AddComponent<Managers>();
        }

        DontDestroyOnLoad(go);

        s_instance = go.GetComponent<Managers>();
        Addressables.InitializeAsync();
        Resource.Init();
        Pool.Init();
        //s_instance._sound.Init();
    }
}
