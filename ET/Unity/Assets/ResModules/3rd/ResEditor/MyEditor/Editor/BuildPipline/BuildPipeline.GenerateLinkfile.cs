using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using HybridCLR.Editor;
using UnityEditor;
using UnityEngine;

namespace Custom.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        private static string _il2cppManagedPath = string.Empty;

        private static string il2cppManagedPath
        {
            get
            {
                if (string.IsNullOrEmpty(_il2cppManagedPath))
                {
                    var contentsPath = EditorApplication.applicationContentsPath;
                    var extendPath = "";

                    var buildTarget = EditorUserBuildSettings.activeBuildTarget;
#if UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
                    switch (buildTarget)
                    {
                        case BuildTarget.StandaloneWindows64:
                        case BuildTarget.StandaloneWindows:
                            extendPath = "PlaybackEngines/windowsstandalonesupport/Variations/il2cpp/Managed/";
                            break;
                        case BuildTarget.iOS:
                            extendPath = "PlaybackEngines/iOSSupport/Variations/il2cpp/Managed/";
                            break;
                        case BuildTarget.Android:
                            extendPath = "PlaybackEngines/AndroidPlayer/Variations/il2cpp/Managed/";
                            break;
                        default:
                            throw new Exception($"[BuildPipeline::GenerateLinkfile] 请选择合适的平台, 目前是:{buildTarget}");
                    }
#elif UNITY_EDITOR_OSX
                switch (buildTarget)
                {
                    case BuildTarget.StandaloneOSX:
                        extendPath = "PlaybackEngines/MacStandaloneSupport/Variations/il2cpp/Managed/";
                        break;
                    case BuildTarget.iOS:
                        extendPath = "../../PlaybackEngines/iOSSupport/Variations/il2cpp/Managed/";
                        break;
                    case BuildTarget.Android:
                        extendPath = "../../PlaybackEngines/AndroidPlayer/Variations/il2cpp/Managed/";
                        break;
                    default:
                        throw new Exception($"[BuildPipeline::GenerateLinkfile] 请选择合适的平台, 目前是:{buildTarget}");
                }
#endif
                    if (string.IsNullOrEmpty(extendPath))
                    {
                        throw new Exception($"[BuildPipeline::GenerateLinkfile] 请选择合适的平台, 目前是:{buildTarget}");
                    }

                    _il2cppManagedPath = Path.Combine(contentsPath, extendPath).Replace('\\', '/');
                }

                return _il2cppManagedPath;
            }
        }

        private static List<string> IgnoreClass = new()
        {
            "editor", "netstandard", "Bee.", "dnlib", ".framework", "Test", "plastic", "Gradle", "log4net", "Analytics", "System.Drawing",
            "NVIDIA", "VisualScripting", "UIElements", "IMGUIModule", ".Cecil", "GIModule", "GridModule", "HotReloadModule", "StreamingModule", 
            "TLSModule", "XRModule", "WindModule", "VRModule", "VirtualTexturingModule", "compiler", "BuildProgram", "NiceIO", "ClothModule",
            "VFXModule", "ExCSS", "GeneratedCode", "mscorlib", "System", "SyncToolsDef"
        };
        private static bool IsIngoreClass(string classFullName)
        {
            var tmpName = classFullName.ToLower();
            foreach (var ic in IgnoreClass)
            {
                if (tmpName.Contains(ic.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<string> IgnoreType = new()
        {
            "jetbrain", "editor", "PrivateImplementationDetails", "experimental", "microsoft.", "compiler"
        };
        private static bool IsIgnoreType(string typeFullName)
        {
            var tmpName = typeFullName.ToLower();
            foreach (var ic in IgnoreType)
            {
                if (tmpName.Contains(ic.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        public static void GenerateLinkfile(string outPath)
        {
            var basePath = il2cppManagedPath;
            if (!Directory.Exists(basePath))
            {
                Debug.LogWarning($"[BuildPipeline::GenerateLinkfile] can't find il2cpp's dlls [{basePath}]");
                basePath = basePath.Replace("/il2cpp/", "/mono/");
            }

            if (!Directory.Exists(basePath))
            {
                Debug.LogWarning($"[BuildPipeline::GenerateLinkfile] can't find il2cpp's dlls [{basePath}]");
                return;
            }

            LinkedList<string> Assemblies;
            Dictionary<string, Assembly> AllAssemblies = new();

            var hashAss = new HashSet<string>();
            var files = new List<string>(Directory.GetFiles(basePath, "*.dll"));
            foreach (var file in files)
            {
                var ass = Assembly.LoadFile(file);
                if (ass != null)
                {
                    var name = ass.GetName().Name;
                    if (IsIngoreClass(name))
                    {
                        continue;
                    }

                    if (!hashAss.Contains(name))
                    {
                        hashAss.Add(name);

                        AllAssemblies[name] = ass;
                    }
                }
            }

            var names = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;
            var localAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var localPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..")).Replace('\\', '/');
            foreach (var ass in localAssemblies)
            {
                if (ass.IsDynamic)
                {
                    Debug.LogWarning($"[BuildPipeline::GenerateLinkfile] {ass.FullName} is dynamic!!!");
                    continue;
                }

                var assPath = Path.GetFullPath(ass.Location).Replace('\\', '/');
                if (assPath.Contains(localPath) && assPath.ToLower().Contains("/editor/"))
                {
                    continue;
                }

                var name = ass.GetName().Name;
                if (hashAss.Contains(name))
                {
                    continue;
                }
                
                var ignore = false;
                foreach (var n in names)
                {
                    if (name.Contains(n))
                    {
                        ignore = true;
                        break;
                    }
                }

                if (ignore)
                {
                    continue;
                }

                hashAss.Add(name);
                AllAssemblies[name] = ass;
            }

            var fullPreserve = new List<string>();
            var otherAss = new List<string>();
            var otherAssemblies = new Dictionary<string, List<string>>();

            foreach (var ass in AllAssemblies)
            {
                if (IsIngoreClass(ass.Key))
                {
                    continue;
                }

                var allTypes = ass.Value.GetTypes();
                var stripTypes = new List<string>();
                foreach (var type in allTypes)
                {
                    if (IsIgnoreType(type.FullName))
                    {
                        continue;
                    }

                    stripTypes.Add(type.FullName);
                }

                if (stripTypes.Count == 0)
                {
                    continue;
                }
                else if (allTypes.Length < 5)
                {
                    fullPreserve.Add(ass.Key);
                }
                else if (allTypes.Length - stripTypes.Count > allTypes.Length * 0.1f)
                {
                    otherAssemblies.Add(ass.Key, stripTypes);
                    otherAss.Add(ass.Key);
                }
                else
                {
                    fullPreserve.Add(ass.Key);
                }
            }

            fullPreserve.Sort();
            otherAss.Sort();

            var fileName = outPath;
            var writer = System.Xml.XmlWriter.Create(fileName, new()
            {
                Encoding = new UTF8Encoding(false),
                Indent = true
            });

            writer.WriteStartDocument();
            writer.WriteStartElement("linker");

            foreach (var fp in fullPreserve)
            {
                writer.WriteStartElement("assembly");
                writer.WriteAttributeString("fullname", fp);
                writer.WriteAttributeString("preserve", "all");
                writer.WriteEndElement();
            }

            foreach (var fp in otherAss)
            {
                writer.WriteStartElement("assembly");
                writer.WriteAttributeString("fullname", fp);

                foreach (var type in otherAssemblies[fp])
                {
                    writer.WriteStartElement("type");
                    writer.WriteAttributeString("fullname", type);
                    writer.WriteAttributeString("preserve", "all");
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }
    }
}
