using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public static class MermaidRenderer
{
    // Opciones de calidad para la renderización
    public enum ImageQuality
    {
        Normal,
        HighDefinition,
        UltraHD
    }
    
    // Formato de salida para los diagramas
    public enum OutputFormat
    {
        PNG,
        SVG,
        Both
    }

    static void RenderMermaidToPng(string mermaidCode, string outputPngPath, ImageQuality quality = ImageQuality.HighDefinition)
    {
        string tempMmdPath = Path.Combine(Application.temporaryCachePath, "temp_diagram.mmd");
        File.WriteAllText(tempMmdPath, mermaidCode);

        // Parámetros adicionales basados en la calidad seleccionada
        string qualityParams = "";
        switch (quality)
        {
            case ImageQuality.HighDefinition:
                qualityParams = "--scale 2 --backgroundColor white";
                break;
            case ImageQuality.UltraHD:
                qualityParams = "--scale 3 --backgroundColor white --width 1920";
                break;
            default:
                qualityParams = "--backgroundColor white";
                break;
        }

        var psi = new ProcessStartInfo {
            FileName        = "mmdc.cmd", // o ruta absoluta
            Arguments       = $"-i \"{tempMmdPath}\" -o \"{outputPngPath}\" {qualityParams}",
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
    
    static string RenderMermaidToSvg(string mermaidCode, string outputSvgPath)
    {
        try
        {
            string tempMmdPath = Path.Combine(Application.temporaryCachePath, "temp_diagram_svg.mmd");
            File.WriteAllText(tempMmdPath, mermaidCode);

            var psi = new ProcessStartInfo {
                FileName        = "mmdc.cmd", // o ruta absoluta
                Arguments       = $"-i \"{tempMmdPath}\" -o \"{outputSvgPath}\" -e svg",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute       = false,
                CreateNoWindow        = true
            };

            // Capturar la salida para diagnósticos
            string output = "";
            string error = "";

            using (var process = Process.Start(psi)) 
            {
                output = process.StandardOutput.ReadToEnd();
                error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                {
                    UnityEngine.Debug.LogError($"Error al generar SVG: {error}");
                    UnityEngine.Debug.LogError($"Salida: {output}");
                }
            }

            File.Delete(tempMmdPath);

            if (!File.Exists(outputSvgPath))
            {
                UnityEngine.Debug.LogError($"El archivo SVG no se generó. Salida: {output}, Error: {error}");
                return "";
            }
                
            // Sanitizar el SVG para hacerlo compatible con Unity
            string sanitizedPath = SanitizeSvgForUnity(outputSvgPath);
            return !string.IsNullOrEmpty(sanitizedPath) ? sanitizedPath : outputSvgPath;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error generando SVG: {ex.Message}");
            return "";
        }
    }

    public static void RenderAndAddToUI(string mermaidCode, string ensayoName, ImageQuality quality = ImageQuality.HighDefinition, OutputFormat format = OutputFormat.Both)
    {
        string fileBaseName = "mermaid_" + Guid.NewGuid();
        string outputPngPath = Path.Combine(Application.persistentDataPath, fileBaseName + ".png");
        string outputSvgPath = Path.Combine(Application.persistentDataPath, fileBaseName + ".svg");
        string svgRelativePath = "";
        bool svgGenerado = false;
        
        UnityEngine.Debug.Log($"MermaidRenderer: generando diagrama en {Application.persistentDataPath}");
        
        // Generar PNG para visualización en UI (esto siempre se hace)
        try
        {
            RenderMermaidToPng(mermaidCode, outputPngPath, quality);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error generando PNG: {ex.Message}");
            return; // Si falla el PNG, no podemos continuar porque es necesario para la UI
        }
        
        // Generar SVG si se solicitó
        if (format == OutputFormat.SVG || format == OutputFormat.Both)
        {
            try
            {
                svgRelativePath = RenderMermaidToSvg(mermaidCode, outputSvgPath);
                svgGenerado = !string.IsNullOrEmpty(svgRelativePath);
                if (svgGenerado)
                {
                    UnityEngine.Debug.Log($"SVG generado: {svgRelativePath}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("No se pudo generar el SVG. Continuando solo con PNG.");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error en generación de SVG: {ex.Message}");
                UnityEngine.Debug.LogWarning("Continuando solo con PNG debido a error en SVG.");
                svgGenerado = false;
            }
        }
  
        byte[] bytes = File.ReadAllBytes(outputPngPath);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        
        // Configurar la textura para mejor calidad
        tex.filterMode = FilterMode.Bilinear;
        tex.anisoLevel = 9;
        
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
        
        // Ajustar descripción basada en lo que realmente se generó
        string formatoDesc = svgGenerado ? "PNG+SVG disponibles" : "Formato PNG";
      
        var rd = new ResourceUIManager.ResourceData {
            ensayo       = ensayoName,
            resourceType = "Docs",                  // debe coincidir con el Toggle "Docs"
            header       = "Diagrama HD",
            subHeader    = ensayoName,
            description  = $"Generado con Mermaid CLI en alta definición. {formatoDesc}",
            thumbnail    = spr,
            videoUrl     = svgGenerado ? svgRelativePath : ""  // Solo guardamos la ruta si el SVG realmente se generó
        };
        manager.allResources.Add(rd);
       
        manager.RefreshResources();
    }
    
    // Método auxiliar para exportar un diagrama como SVG
    public static string GenerateSvgOnly(string mermaidCode, string fileName = null)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = "mermaid_" + Guid.NewGuid() + ".svg";
            else if (!fileName.EndsWith(".svg"))
                fileName += ".svg";
                
            string outputPath = Path.Combine(Application.persistentDataPath, fileName);
            string result = RenderMermaidToSvg(mermaidCode, outputPath);
            
            if (string.IsNullOrEmpty(result))
            {
                UnityEngine.Debug.LogWarning("No se pudo generar el archivo SVG. Verifica que mmdc.cmd soporte el formato SVG.");
                return "";
            }
            
            return result;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error generando SVG: {ex.Message}");
            return "";
        }
    }

    // Ajusta el SVG para hacerlo compatible con Unity
    private static string SanitizeSvgForUnity(string svgPath)
    {
        try
        {
            string svgContent = File.ReadAllText(svgPath);
            
            // Cambios comunes para compatibilidad
            // 1. Reemplazar colores con formato incorrecto
            svgContent = System.Text.RegularExpressions.Regex.Replace(
                svgContent, 
                "fill=\"rgba\\([^)]+\\)\"", 
                "fill=\"#000000\"");
            
            // 2. Corregir otros elementos problemáticos
            svgContent = svgContent.Replace("fill=\"none\"", "fill=\"transparent\"");
            
            // Guardar el archivo sanitizado
            string sanitizedPath = Path.Combine(
                Path.GetDirectoryName(svgPath),
                Path.GetFileNameWithoutExtension(svgPath) + "_unity.svg");
            
            File.WriteAllText(sanitizedPath, svgContent);
            return sanitizedPath;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error sanitizando SVG: {ex.Message}");
            return "";
        }
    }
}
