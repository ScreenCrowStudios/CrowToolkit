// ===========================================================================
//
//  FILE:    AtlasExporter.cs
//  DESC:    Adds a Menu Item that exports the Texture Atlas of a selected 
//           .psd file. The Texture Atlas is used to create secondary
//           textures (usually in different software like SpriteIlluminator).
//
// ===========================================================================

using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.U2D.PSD;

public static class PSDAtlasExporter
{
    [MenuItem("Assets/Export Atlas from PSD", true)]
    private static bool ValidateExport()
    {
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return !string.IsNullOrEmpty(path) && path.EndsWith(".psd", System.StringComparison.OrdinalIgnoreCase);
    }

    [MenuItem("Assets/Export Atlas from PSD")]
    private static void ExportAtlasFromPSD()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("No PSD file selected.");
            return;
        }

        PSDImporter importer = AssetImporter.GetAtPath(path) as PSDImporter;
        if (importer == null)
        {
            Debug.LogError("Selected asset is not a valid PSD file managed by PSDImporter: " + path);
            return;
        }

        // Cache initial state
        bool originalIsReadable = importer.isReadable;
        TextureImporterPlatformSettings defaultSettings = importer.GetImporterPlatformSettings(BuildTarget.NoTarget);
        TextureImporterCompression originalCompression = defaultSettings.textureCompression;

        bool needsReimport = !originalIsReadable || originalCompression != TextureImporterCompression.Uncompressed;

        try
        {
            if (needsReimport)
            {
                importer.isReadable = true;
                
                // Modify compression settings and apply
                defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetImporterPlatformSettings(defaultSettings);

                importer.SaveAndReimport();
            }

            // Load generated texture atlas
            Texture2D sourceTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (sourceTex == null)
            {
                Debug.LogError("Could not load Texture2D from PSD path: " + path);
                return;
            }

            // Encode to PNG format
            byte[] pngData = sourceTex.EncodeToPNG();
            if (pngData != null)
            {
                string savePath = EditorUtility.SaveFilePanel(
                    "Save Atlas as PNG",
                    "",
                    Path.GetFileNameWithoutExtension(path) + "_atlas.png",
                    "png"
                );

                if (!string.IsNullOrEmpty(savePath))
                {
                    File.WriteAllBytes(savePath, pngData);
                    Debug.Log("Atlas successfully exported to: " + savePath);
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                Debug.LogError("Failed to encode texture to PNG.");
            }
        }
        finally
        {
            // Revert settings
            if (needsReimport)
            {
                PSDImporter revertImporter = AssetImporter.GetAtPath(path) as PSDImporter;
                if (revertImporter != null)
                {
                    revertImporter.isReadable = originalIsReadable;
                    
                    TextureImporterPlatformSettings revertSettings = revertImporter.GetImporterPlatformSettings(BuildTarget.NoTarget);
                    revertSettings.textureCompression = originalCompression;
                    revertImporter.SetImporterPlatformSettings(revertSettings);

                    revertImporter.SaveAndReimport();
                }
            }
        }
    }
}