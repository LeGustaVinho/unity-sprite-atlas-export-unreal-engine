using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.U2D;

public class UeSpriteExporterEditor : EditorWindow
{
    public SpriteAtlas SpriteAtlas;
    public RenderTextureReadWrite RenderTextureReadWrite = RenderTextureReadWrite.Linear;
    public RenderTextureFormat RenderTextureFormat = RenderTextureFormat.Default;
    public GraphicsFormat GraphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;

    [MenuItem("Tools/UnrealEngine/Unreal Engine Sprite Atlas Export")]
    public static void ShowWindow()
    {
        GetWindow<UeSpriteExporterEditor>("Unreal Engine Sprite Atlas Export", true);
    }

    public void OnGUI()
    {
        SerializedObject serialObj = new(this);
        SerializedProperty spriteAtlasProperty = serialObj.FindProperty(nameof(SpriteAtlas));
        SerializedProperty renderTextureReadWriteProperty = serialObj.FindProperty(nameof(RenderTextureReadWrite));
        SerializedProperty renderTextureFormatProperty = serialObj.FindProperty(nameof(RenderTextureFormat));
        SerializedProperty graphicsFormatProperty = serialObj.FindProperty(nameof(GraphicsFormat));

        EditorGUILayout.PropertyField(spriteAtlasProperty);
        EditorGUILayout.PropertyField(renderTextureReadWriteProperty);
        EditorGUILayout.PropertyField(renderTextureFormatProperty);
        EditorGUILayout.PropertyField(graphicsFormatProperty);

        if (GUILayout.Button("Export"))
        {
            ExportSpriteAtlasToUe();
        }
        
        GUILayout.Label("Note: Dont work with SpriteAtlas with TightPacking enabled, so keep it disabled.");

        serialObj.ApplyModifiedProperties();
    }

    public void ExportSpriteAtlasToUe()
    {
        Texture2D[] atlasTextures = ExportAtlasImage(out string folderPath);
        ExportSpriteData(atlasTextures, folderPath);
    }

    private void ExportSpriteData(Texture2D[] atlasTextures, string folderPath)
    {
        Sprite[] sprites = new Sprite[SpriteAtlas.spriteCount];
        SpriteAtlas.GetSprites(sprites);
        List<Sprite> allSprites = new List<Sprite>(sprites);

        for (int index = 0; index < atlasTextures.Length; index++)
        {
            Texture2D atlasTexture = atlasTextures[index];
            UeSpriteSheetSchema exportedSpriteData = new UeSpriteSheetSchema(
                "https://www.codeandweb.com/texturepacker",
                "1.0",
                "paper2D",
                $"{GetSpriteAtlasNameWithNoSpaces()}_{index}.png",
                "RGBA8888",
                new UeSpriteSize(atlasTexture.width, atlasTexture.height),
                "1");

            List<Sprite> spritesForThisTexture =
                allSprites.FindAll(item => item.texture.name.StartsWith($"sactx-{index}"));

            foreach (Sprite sprite in spritesForThisTexture)
            {
                UeSpriteFrame frame = new UeSpriteFrame()
                {
                    frame = new UeSpriteRect(
                        Mathf.RoundToInt(sprite.textureRect.width),
                        Mathf.RoundToInt(sprite.textureRect.height),
                        Mathf.RoundToInt(sprite.textureRect.x),
                        Mathf.RoundToInt(atlasTexture.height - (sprite.textureRect.y + sprite.textureRect.height))),
                    rotated = sprite.packingRotation != SpritePackingRotation.None,
                    trimmed = false,
                    spriteSourceSize = new UeSpriteRect(
                        Mathf.RoundToInt(sprite.rect.width),
                        Mathf.RoundToInt(sprite.rect.height),
                        Mathf.RoundToInt(sprite.rect.x),
                        Mathf.RoundToInt(sprite.rect.y)),
                    sourceSize = new UeSpriteSize(Mathf.RoundToInt(sprite.rect.width),
                        Mathf.RoundToInt(sprite.rect.height))
                };
                exportedSpriteData.AddFrame(RemoveSpaceFromName(sprite.name.Replace("(Clone)", ".png")), frame);
            }

            string json = JsonConvert.SerializeObject(exportedSpriteData);
            File.WriteAllText(Path.Combine(folderPath, $"{GetSpriteAtlasNameWithNoSpaces()}_{index}.paper2dsprites"), json);
        }
    }

    private Texture2D[] ExportAtlasImage(out string folderPath)
    {
        Texture2D[] textures = ReflectionSpriteAtlasGetPreviewTextures(SpriteAtlas);
        Texture2D[] result = new Texture2D[textures.Length];
        folderPath = EditorUtility.SaveFolderPanel("", "", "");
        
        for (int index = 0; index < textures.Length; index++)
        {
            result[index] = DuplicateTexture(textures[index], GraphicsFormat);
            File.WriteAllBytes(Path.Combine(folderPath, $"{GetSpriteAtlasNameWithNoSpaces()}_{index}.png"),
                result[index].EncodeToPNG());
        }

        return result;
    }

    private Texture2D[] ReflectionSpriteAtlasGetPreviewTextures(SpriteAtlas atlas)
    {
        Type type = typeof(SpriteAtlasExtensions);
        MethodInfo methodInfo = type.GetMethod("GetPreviewTextures", BindingFlags.Static | BindingFlags.NonPublic);

        if (methodInfo == null)
        {
            Debug.LogWarning("Failed to get UnityEditor.U2D.SpriteAtlasExtensions");
            return null;
        }

        return (Texture2D[])methodInfo.Invoke(null, new object[] { atlas });
    }
    
    private Texture2D DuplicateTexture(Texture2D source, GraphicsFormat format)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat,
            RenderTextureReadWrite);
 
        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height, format, TextureCreationFlags.None);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.name = $"{source.name}_{format.ToString()}";
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

    private string GetSpriteAtlasNameWithNoSpaces()
    {
        return RemoveSpaceFromName(SpriteAtlas.name);
    }
    
    private string RemoveSpaceFromName(string name)
    {
        return name.Replace(" ", "_");
    }
}