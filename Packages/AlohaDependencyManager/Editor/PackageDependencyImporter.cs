using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Aloha.DependencyManager.Editor
{
    public static class PackageDependencyImporter
    {
        public static void UpdatePackageManifest(string[] dependencyFiles)
        {
            if (dependencyFiles.Length == 0) return;
            
            var packageManifest = File.ReadAllText("Packages/manifest.json");
            var jObject = JObject.Parse(packageManifest);
            var dependencies = jObject["dependencies"];
            var isDirty = false;
            var resultString = "";

            var packageDependencies = new List<JProperty>();
            foreach (var dependencyFile in dependencyFiles)
            {
                Debug.Log("Found dependency file: " + dependencyFile);
                var json = JObject.Parse(File.ReadAllText(dependencyFile));
                packageDependencies.AddRange(json.Properties());
            }
            
            foreach (JProperty package in packageDependencies)
            {
                if (dependencies[package.Name] == null
                    || CompareVersion(dependencies[package.Name].Value<JToken>().ToString(), package.Value.Value<string>()) < 0)
                {
                    dependencies[package.Name] = package.Value.Value<string>();
                    resultString += $"Updated {package.Name} to {package.Value<JToken>()}\n";
                    isDirty = true;
                }
            }

            if (isDirty)
            {
                Debug.Log("Updated package manifest\n" + resultString);
                File.WriteAllText("Packages/manifest.json", jObject.ToString());   
            }
        }

        private static int CompareVersion(string a, string b)
        {
            var aSplit = a.Split('.');
            var bSplit = b.Split('.');
        
            for (var i = 0; i < Math.Min(aSplit.Length, bSplit.Length); i++)
            {
                if (aSplit[i] == bSplit[i]) continue;
            
                if(int.TryParse(aSplit[i], out var aVer) && int.TryParse(bSplit[i], out var bVer))
                {
                    return aVer - bVer;
                }
                else
                {
                    return String.Compare(aSplit[i], bSplit[i], StringComparison.Ordinal);
                }
            }

            return aSplit.Length - bSplit.Length;
        }
    }
}