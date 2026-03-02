using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneToLoad = "RoomRuntime";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Loads runtime scene immediately when the game starts.
        SceneManager.LoadScene(firstSceneToLoad);
    }
}