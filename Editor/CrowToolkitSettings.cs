using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CrowToolkitSettings : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Tools/CrowToolkit/Settings")]
    public static void ShowExample()
    {
        CrowToolkitSettings wnd = GetWindow<CrowToolkitSettings>();
        wnd.titleContent = new GUIContent("CrowToolkitSettings");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        // Clamp Max Log Amount input field to values greater than 0
        var maxLogField = root.Q<IntegerField>("max-log-amount");
        maxLogField?.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue <= 0) maxLogField.value = 1;
            });
    }
}
