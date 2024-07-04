#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

namespace DialogueSystem.Utilities
{
    public static class FileSystemUtility
    {
        public static bool FolderExists(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return false;
            return Directory.Exists(folderPath);
        }

        public static bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            return File.Exists(filePath);
        }

        public static void CreateFolder(string folderPath)
        {
            Directory.CreateDirectory(folderPath);
        }

        public static string FindFolderPath(string folderName)
        {
            string[] guids = AssetDatabase.FindAssets(folderName);
            foreach (var guid in guids)
            {
                string folderPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Directory.Exists(folderPath)) return folderPath;
            }

            return string.Empty;
        }

        public static List<string> FindFolderNamesInPath(string folderPath)
        {
            string[] folderNames = folderPath.Split(new string[] { @"/", @"\" }, System.StringSplitOptions.RemoveEmptyEntries);
            return new List<string>(folderNames);
        }

        public static string FindLastFolderNameInPath(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return string.Empty;
            return new DirectoryInfo(folderPath).Name;
        }

        public static List<string> FindAllFilesInFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return new List<string>();
            return new List<string>(Directory.GetFiles(folderPath));
        }

        public static List<string> FindImmediateChildFolders(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return new List<string>();
            return new List<string>(Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly));
        }

        public static List<string> FindAllFolders(string folderPath, bool includeSelf)
        {
            if (!Directory.Exists(folderPath)) return new List<string>();

            var allFolders = new List<string>();
            if (includeSelf) allFolders.Add(folderPath);

            try
            {
                var folders = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);
                if (folders.Length != 0) allFolders.AddRange(folders);
            }
            catch (UnauthorizedAccessException)
            {
                return allFolders;
            }

            return allFolders;
        }

        public static bool IsFolder(string folderPath)
        {
            return Directory.Exists(folderPath);
        }

        public static List<string> FilterRootFolders(IEnumerable<string> folderPaths)
        {
            List<string> paths = new List<string>(folderPaths);
            paths.RemoveAll(item => !IsFolder(item));
            if (paths.Count == 0) return paths;

            for (int pathIndex = 0; pathIndex < paths.Count;)
            {
                string path = Path.GetFullPath(paths[pathIndex]) + Path.DirectorySeparatorChar;
                bool isChild = false;

                for (int otherPathIndex = 0; otherPathIndex < paths.Count; ++otherPathIndex)
                {
                    if (otherPathIndex != pathIndex)
                    {
                        string otherPath = Path.GetFullPath(paths[otherPathIndex]) + Path.DirectorySeparatorChar;
                        if (path.StartsWith(otherPath))
                        {
                            isChild = true;
                            break;
                        }
                    }
                }

                if (isChild)
                {
                    paths.RemoveAt(pathIndex);
                    continue;
                }
                else ++pathIndex;
            }

            return paths;
        }
    }
}
#endif