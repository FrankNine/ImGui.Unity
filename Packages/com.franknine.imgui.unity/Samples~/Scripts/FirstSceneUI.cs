using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FirstSceneUI : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private ImGuiDemo _imGuiDemo;
    [SerializeField] private Button _nextSceneButton;

    private void Start()
    {
        DontDestroyOnLoad(_camera);
        DontDestroyOnLoad(_imGuiDemo);
        _nextSceneButton.onClick.AddListener(() => { SceneManager.LoadScene("Sample2"); });
    }
}