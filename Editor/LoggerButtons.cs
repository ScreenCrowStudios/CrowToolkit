using UnityEngine;
using UnityEditor;
using System.IO;

public class LoggerButtons : MonoBehaviour
{

    [MenuItem("Debug/Delete Error Logs")]
    private static void DeleteErrorLogs()
    {
        var path = Path.Combine(Application.persistentDataPath, "Logs");
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            Debug.Log("Error Logs folder deleted successfully");
        }
        else
        {
            Debug.LogWarning("Error Logs folder not found");
        }
    }

    [MenuItem("Debug/Open Data Folder")]
    private static void OpenDataFolder()
    {
        string folderPath = Application.persistentDataPath;
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = folderPath,
            UseShellExecute = true
        });
    }

}
