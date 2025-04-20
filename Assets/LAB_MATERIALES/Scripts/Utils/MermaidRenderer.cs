using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class MermaidRenderer
{

    static void RenderMermaidToPng(string mermaidCode, string outputPngPath)
    {
        string tempMmdPath = Path.Combine(Application.temporaryCachePath, "temp_diagram.mmd");
        File.WriteAllText(tempMmdPath, mermaidCode);

        var psi = new ProcessStartInfo {
            FileName        = "mmdc.cmd", // o ruta absoluta
            Arguments       = $"-i \"{tempMmdPath}\" -o \"{outputPngPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute       = false,
            CreateNoWindow        = true
        };

        using (var process = Process.Start(psi)) {
            process.WaitForExit();
        }

        File.Delete(tempMmdPath);

        if (!File.Exists(outputPngPath))
            throw new IOException("No se pudo generar el PNG desde Mermaid.");
    }

    public static void RenderAndAddToUI(string mermaidCode, string ensayoName)
    {
 
        string fileName   = "mermaid_" + Guid.NewGuid() + ".png";
        string outputPath = Path.Combine(Application.persistentDataPath, fileName);
        UnityEngine.Debug.Log($"MermaidRenderer: guardando PNG en {outputPath}");
        RenderMermaidToPng(mermaidCode, outputPath);

  
        byte[] bytes = File.ReadAllBytes(outputPath);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        var spr = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );


        var manager = UnityEngine.Object.FindFirstObjectByType<ResourceUIManager>();
        if (manager == null) {
            UnityEngine.Debug.LogError("MermaidRenderer: no se encontró ningún ResourceUIManager en la escena.");
            return;
        }

      
        var rd = new ResourceUIManager.ResourceData {
            ensayo       = ensayoName,
            resourceType = "Docs",                  // debe coincidir con el Toggle "Docs"
            header       = "Diagrama",
            subHeader    = ensayoName,
            description  = "Generado con Mermaid CLI",
            thumbnail    = spr
        };
        manager.allResources.Add(rd);

       
        manager.RefreshResources();
    }
}
