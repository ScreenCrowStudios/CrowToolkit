using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.U2D.PSD;

public static class PSDAtlasExporter
{
    [MenuItem("Assets/Export Atlas from PSD", true)]
    private static bool ValidateExport()
    {
        // Only enable if a PSD is selected
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return path.EndsWith(".psd", System.StringComparison.OrdinalIgnoreCase);
    }

    [MenuItem("Assets/Export Texture Atlas")]
    private static void ExportAtlasFromPSD()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("No PSD file selected.");
            return;
        }

        // Load the texture Unity generated from this PSD
        Texture2D sourceTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (sourceTex == null)
        {
            Debug.LogError("Could not load texture from PSD: " + path);
            return;
        }

        // Copy texture into readable format
        string tempPath = AssetDatabase.GetAssetPath(sourceTex);
        PSDImporter importer = AssetImporter.GetAtPath(tempPath) as PSDImporter;
        bool wasReadable = importer.isReadable;
        if (!wasReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        // Encode to PNG
        byte[] pngData = sourceTex.EncodeToPNG();
        if (pngData != null)
        {
            string savePath = EditorUtility.SaveFilePanel("Save Atlas as PNG", "", Path.GetFileNameWithoutExtension(path) + "_atlas.png", "png");
            if (!string.IsNullOrEmpty(savePath))
            {
                File.WriteAllBytes(savePath, pngData);
                Debug.Log("Atlas exported to: " + savePath);
            }
        }
        else
        {
            Debug.LogError("Failed to encode texture to PNG.");
        }

        // Restore original import settings
        if (!wasReadable)
        {
            importer.isReadable = false;
            importer.SaveAndReimport();
        }
    }
}
