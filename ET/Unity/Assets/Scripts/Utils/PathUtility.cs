using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameMain.Utils
{
    public static class PathUtility
    {
        /// <summary>
        /// 确保存在目录
        /// </summary>
        /// <param name="dir"></param>
        public static void EnsureExistDirectory(string dir)
        {
            var directoryInfo = new DirectoryInfo(dir);
            if (directoryInfo.Exists)
            {
                return;
            }

            directoryInfo.Create();
        }

        /// <summary>
        /// 确保存在文件目录
        /// </summary>
        /// <param name="file"></param>
        public static void EnsureExistFileDirectory(string file)
        {
            try
            {
                var dirName = Path.GetDirectoryName(file);
                if (string.IsNullOrEmpty(dirName))
                {
                    SimpleLog.Log($"[PathUtility]EnsureExistFileDirectory:{file} dirname is null");
                    return;
                }

                EnsureExistDirectory(dirName);
            }
            catch (Exception ex)
            {
                SimpleLog.Log($"[PathUtility]EnsureExistFileDirectory:{file}");
                SimpleLog.LogException(ex);
            }
        }

        /// <summary>
        /// 确保不存在目录
        /// </summary>
        /// <param name="dir"></param>
        public static void EnsureNotExistDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return;
            }

            Directory.Delete(dir, true);
        }

        /// <summary>
        /// 确保不存在文件目录
        /// </summary>
        /// <param name="file"></param>
        public static void EnsureNotExistFileDirectory(string file)
        {
            var dirName = Path.GetDirectoryName(file);
            if (string.IsNullOrEmpty(dirName))
            {
                SimpleLog.Log($"[PathUtility]EnsureNotExistFileDirectory:{file} dirname is null");
                return;
            }

            EnsureNotExistDirectory(dirName);
        }

        /// <summary>
        /// 确保不存在文件
        /// </summary>
        /// <param name="file"></param>
        public static void EnsureNotExistFile(string file)
        {
            var fileInfo = new FileInfo(Path.GetFullPath(file));
            if (!fileInfo.Exists)
            {
                return;
            }

            fileInfo.Delete();
        }

        /// <summary>
        /// 复制文件夹及文件（不包含原文件根目录名称）
        /// </summary>
        /// <param name="sourceFolder">原文件路径</param>
        /// <param name="destFolder">目标文件路径</param>
        /// <returns></returns>
        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            try
            {
                //如果目标路径不存在,则创建目标路径
                EnsureExistDirectory(destFolder);

                //得到原文件根目录下的所有文件
                var files = Directory.GetFiles(sourceFolder);
                foreach (var file in files)
                {
                    var name = Path.GetFileName(file);
                    var dest = Path.GetFullPath(Path.Combine(destFolder, name));
                    File.Copy(file, dest, true); //复制文件
                }

                //得到原文件根目录下的所有文件夹
                var folders = Directory.GetDirectories(sourceFolder);
                foreach (var folder in folders)
                {
                    var name = Path.GetFileName(folder);
                    var dest = Path.Combine(destFolder, name);
                    CopyFolder(folder, dest); //构建目标路径,递归复制文件
                }
            }
            catch (Exception e)
            {
                SimpleLog.LogException(e);
            }
        }
    }
}
