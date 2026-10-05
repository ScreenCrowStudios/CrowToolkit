using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CrowToolkitSettings : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Tools/CrowToolkit")]
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

#region Logger
        // Enable Logger
        var enableLogger = root.Q<Toggle>("enable-logger");

        enableLogger.value = PlayerPrefs.GetInt("Logger_Enabled", 1) == 1; // Fetch previous
        enableLogger.RegisterValueChangedCallback(evt =>
        {
            PlayerPrefs.SetInt("Logger_Enabled", evt.newValue ? 1 : 0);
            PlayerPrefs.Save();
        });

        // Max Log Amount
        var maxLogField = root.Q<IntegerField>("max-log-amount");

        // Clamp Max Log Amount input field to values greater than 0
        maxLogField?.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue <= 0) maxLogField.value = 1;
            });

        // Save Value to access from Logger.cs
        maxLogField.value = PlayerPrefs.GetInt("Logger_MaxLogs", 50); // Fetch previous
        maxLogField.RegisterValueChangedCallback(evt =>
        {
            PlayerPrefs.SetInt("Logger_MaxLogs", evt.newValue);
            PlayerPrefs.Save();
        });
#endregion
    }
}
