using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;

namespace RollicGames.Advertisements.Editor
{
    public static class CodeControlUtils
    {
        private static string HASHED_CLASS_CODE = "ac630c19bb1539290d92909453f720812ecab29820a4a97a87ae437bc83627ad";

        private const string AssetsPathPrefix = "Assets/";
        private const string Directory = AssetsPathPrefix + "RollicGames";
        private const string FileName = "RLAdvertisementManager.cs";

        public const string Warning =
            "Please, do not change RLAdvertisementManager.cs content. Rollback all changes in RLAdvertisementManager.cs. If the issue persists, please delete the package and re-install from [Elephant -> Manage Core SDKs]";


        [UnityEditor.Callbacks.DidReloadScripts]
        public static void OnReloadScripts()
        {
            if (!CheckCode())
            {
                EditorUtility.DisplayDialog("Error", Warning, "OK");
            }
        }

        public static bool CheckCode()
        {
            var originalFileHash = CodeControlUtils.GetOriginalClassFile();
            var implementedFileHash = CodeControlUtils.GetImplementedClassFile();
            
            if (!originalFileHash.Equals(implementedFileHash))
            {
                return false;
            }

            return true;
        }

        private static string GetOriginalClassFile()
        {
            return HASHED_CLASS_CODE;
        }

        private static string GetImplementedClassFile()
        {
            var path = System.IO.Path.Combine(Directory, FileName);
            StreamReader streamReader = new StreamReader(path, Encoding.UTF8);
            var implementedClassFile = streamReader.ReadToEnd();
            var removedBreaks = implementedClassFile.Replace("\r\n", "\n");
            var hashedCode = ComputeSha256Hash(removedBreaks);
            return hashedCode;
        }

        private static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                foreach (var t in bytes)
                {
                    builder.Append(t.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }


    [InitializeOnLoad]
    public class CodeControlOnBuild
    {
        static CodeControlOnBuild()
        {
            if (!CodeControlUtils.CheckCode())
            {
                BuildPlayerWindow.RegisterBuildPlayerHandler(ThrowException);
            }
        }

        static void ThrowException(BuildPlayerOptions obj)
        {
            throw new BuildPlayerWindow.BuildMethodException(CodeControlUtils
                .Warning);
        }
    }
}