using UnityEngine;

public class InputDebug : MonoBehaviour
{
    private PlayerInput input;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (Time.frameCount % 20 == 0)
        {
            Debug.Log($"Move: {input.Move} | MouseScreen: {input.MouseScreen}");
        }
    }
}
