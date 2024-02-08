using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using GameMain.Utils;
using HybridCLR.Editor;
using HybridCLR.Editor.Meta;
using UnityEditor;
using UnityEngine;

namespace GameMain.Editor.BuildPipeline
{
    public partial class BuildPipeline
    {
        private static Helper _keyHelper;

        private static void PrepareMetadata()
        {
            RevertMetadataLoaderCppFile();

            _keyHelper = new Helper();

            OnPrepareDecryptCode();
        }

        private static void OnEncryptMetadataProcess(string rootPath = null)
        {
            var srcPath = rootPath;
            if (string.IsNullOrEmpty(rootPath))
            {
                srcPath = $"{SettingsUtil.ProjectDir}/Temp/StagingArea/Data/Managed/Metadata/global-metadata.dat";
            }

            SimpleLog.Log($"[BuildPipeline:EncryptMetadata] {srcPath}");

            Encrypt(srcPath);
        }

        private static void Encrypt(string strFileName)
        {
            if (!File.Exists(strFileName))
            {
                throw new Exception($"[BuildPipeline::Encrypt] {strFileName}找不到!!!");
            }

            var sw = Stopwatch.StartNew();
            var fileBytes = File.ReadAllBytes(strFileName);

            // 关闭 sanity == 0xFAB11BAF &&&& version == 29 
            var rand = new System.Random();
            for (var i = 0; i < 8; ++i)
            {
                fileBytes[i] = (byte)rand.Next(byte.MinValue + 5, byte.MaxValue - 5);
            }

            using var msIn = new MemoryStream(fileBytes);
            using var msOut = new MemoryStream();

            Utility.Compress(msIn, msOut);

            fileBytes = msOut.ToArray();

            _keyHelper.Xor(fileBytes);

            File.WriteAllBytes(strFileName, fileBytes);

            sw.Stop();

            SimpleLog.Log($"[BuildPipeline:Encrypt] finished: {sw.ElapsedMilliseconds} ms!!!");
        }

        private static void OnPrepareDecryptCode()
        {
            var strMetadataLoaderFile = "MetadataLoader2020.cpp";
             var assets = AssetDatabase.FindAssets(strMetadataLoaderFile);
            if (assets.Length == 0)
            {
                throw new Exception($"[BuildPipeline::OnPrepareDecryptCode] MetadataLoader.cpp找不到!!!");
            }

            var filePath = AssetDatabase.GUIDToAssetPath(assets[0]);
            var codeTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            if (codeTemplate == null || string.IsNullOrEmpty(codeTemplate.text))
            {
                throw new Exception($"[BuildPipeline::OnPrepareDecryptCode] codeTemplate is empty!!");
            }

            var codeTxt = codeTemplate.text;

            strMetadataLoaderFile = "MetadataLoaderHeader.cpp";
            assets = AssetDatabase.FindAssets(strMetadataLoaderFile);
            if (assets.Length == 0)
            {
                throw new Exception($"[BuildPipeline::OnPrepareDecryptCode] MetadataLoaderHeader.cpp找不到!!!");
            }

            filePath = AssetDatabase.GUIDToAssetPath(assets[0]);
            codeTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            if (codeTemplate == null || string.IsNullOrEmpty(codeTemplate.text))
            {
                throw new Exception($"[BuildPipeline::OnPrepareDecryptCode] MetadataLoaderHeader is empty!!");
            }

            var codeHeader = codeTemplate.text;

            var src = Path.Combine(SettingsUtil.LocalIl2CppDir, "libil2cpp", "vm", "MetadataLoader.cpp");
            src = Path.GetFullPath(src).Replace('\\', '/');
            var tmpFile = src + ".DISABLED";
            if (!File.Exists(tmpFile))
            {
                File.Copy(src, tmpFile, true);
            }

            var password = _keyHelper.Password;
            var paramsStr = new StringBuilder();
            var startPos = 0;
            paramsStr.AppendLine();
            foreach (var pass in password)
            {
                paramsStr.AppendLine($"\t\t\tfileOutput[{startPos++}] ^= 0x{pass:x};");
            }

            codeHeader = codeHeader.Replace("//!{{ProcessCode}}//", paramsStr.ToString());
            codeTxt = codeTxt.Replace("//{{HEADER}}//", codeHeader);

            File.WriteAllText(src, codeTxt);
        }

        private static void RevertMetadataLoaderCppFile()
        {
            var src = Path.Combine(SettingsUtil.LocalIl2CppDir, "libil2cpp", "vm", "MetadataLoader.cpp");
            src = Path.GetFullPath(src).Replace('\\', '/');
            var tmpFile = src + ".DISABLED";

            if (File.Exists(tmpFile))
            {
                File.Copy(tmpFile, src, true);
                File.Delete(tmpFile);

                SimpleLog.Log($"[BuildPipeline:RevertMetadataLoaderCppFile] MetadataLoader.cpp ok!!!");
            }
            else
            {
                SimpleLog.Log($"[BuildPipeline:RevertMetadataLoaderCppFile] can't find {tmpFile} file!!!");
            }
        }
    }
}
