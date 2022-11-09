#if UNITY_IOS
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace RollicGames.Editor
{
    public class PostProcess
    {
#if !UNITY_2019_3_OR_NEWER
        private const string UnityMainTargetName = "Unity-iPhone";
#endif

        [PostProcessBuild(46)]
        public static void OnPostprocessBuildForFramework(BuildTarget target, string pathToBuiltProject) {
            if (target == BuildTarget.iOS)
            {
                string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                PBXProject project = new PBXProject();
                project.ReadFromString(File.ReadAllText(projectPath));
                
                EditPodFile(pathToBuiltProject);

#if UNITY_2019_3_OR_NEWER
                var unityMainTargetGuid = project.GetUnityMainTargetGuid();
                var unityFrameworkTargetGuid = project.GetUnityFrameworkTargetGuid();
#else
                var unityMainTargetGuid = project.TargetGuidByName(UnityMainTargetName);
                var unityFrameworkTargetGuid = project.TargetGuidByName(UnityMainTargetName);
#endif

                AddSwiftSupportIfNeeded(pathToBuiltProject, project, unityFrameworkTargetGuid);
                AddCalendarSupport(pathToBuiltProject);
                
                // Write.
                File.WriteAllText(projectPath, project.WriteToString());
            }
        }
        
        [PostProcessBuild(102)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
            if (target == BuildTarget.iOS)
            {
                string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
                PBXProject project = new PBXProject();
                project.ReadFromString(File.ReadAllText(projectPath));

                EditInfoPlist(pathToBuiltProject);

                // Write.
                File.WriteAllText(projectPath, project.WriteToString());
            }
        }

        private static void EditPodFile(string pathToBuiltProject)
        {
            var podPath = System.IO.Path.Combine(pathToBuiltProject, "Podfile");
            var content = File.ReadAllText(podPath);
            content = content.Replace("use_frameworks!", "#use_frameworks!");
            File.WriteAllText(podPath, content);
        }

        private static void EditInfoPlist(string pathToBuiltProject)
        {
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            
            PlistElementDict rootDict = plist.root;
            var rootValues = rootDict.values;
            
            // Fyber
            rootValues.Remove("NSAllowsArbitraryLoadsInWebContent");
            rootValues.Remove("NSAllowsArbitraryLoadsForMedia");
            rootValues.Remove("NSAllowsLocalNetworking");
            rootDict.SetBoolean("NSAllowsArbitraryLoads", true);
            

            // SKAdNetwork
            var array = rootDict.CreateArray("SKAdNetworkItems");
            XElement[] elements = XDocument.Load(@"Assets/RollicGames/Resources/SkanIds.xml").Descendants("dict").ToArray();
            
            foreach (var element in elements)
            {
                var dict = array.AddDict();
                
                var key = "SKAdNetworkIdentifier";
                var networkId = element.Value.Replace("SKAdNetworkIdentifier", "");
                
                dict.SetString(key, networkId);
            }
            
            File.WriteAllText(plistPath, plist.WriteToString());
        }

        private static void AddSwiftSupportIfNeeded(string buildPath, PBXProject project, string targetGuid)
        {
            var swiftFileRelativePath = "Classes/RollicAdsSwiftSupport.swift";
            var swiftFilePath = Path.Combine(buildPath, swiftFileRelativePath);

            // Add Swift file
            CreateSwiftFile(swiftFilePath);
            var swiftFileGuid = project.AddFile(swiftFilePath, swiftFileRelativePath, PBXSourceTree.Source);
            project.AddFileToBuild(targetGuid, swiftFileGuid);

            // Add Swift version property if needed
#if UNITY_2018_2_OR_NEWER
            var swiftVersion = project.GetBuildPropertyForAnyConfig(targetGuid, "SWIFT_VERSION");
#else
            // Assume that swift version is not set on older versions of Unity.
            const string swiftVersion = "";
#endif
            if (string.IsNullOrEmpty(swiftVersion))
            {
                project.SetBuildProperty(targetGuid, "SWIFT_VERSION", "5.0");
            }

            // Enable Swift modules
            project.AddBuildProperty(targetGuid, "CLANG_ENABLE_MODULES", "YES");
        }

        private static void CreateSwiftFile(string swiftFilePath)
        {
            if (File.Exists(swiftFilePath))
            {
                return;
            }
            // Create a file to write to.
            using (var writer = File.CreateText(swiftFilePath))
            {
                writer.WriteLine("//\n//  RollicAdsSwiftSupport.swift\n//");
                writer.WriteLine("\nimport Foundation\n");
                writer.WriteLine("// This file ensures the project includes Swift support.");
                writer.WriteLine("// It is automatically generated by the RollicAds Unity Plugin.");
                writer.Close();
            }
        }

        private static void AddCalendarSupport(string pathToBuiltProject)
        {
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;

            // Enable calendar
            rootDict.SetString("NSCalendarsUsageDescription", "${PRODUCT_NAME} requests access to the Calendar");

            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif