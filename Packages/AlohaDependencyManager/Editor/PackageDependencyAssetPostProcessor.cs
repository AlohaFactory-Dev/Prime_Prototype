using System.IO;
using System.Linq;
using UnityEditor;

namespace Aloha.DependencyManager.Editor
{
    public class PackageDependencyAssetPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var dependencies = importedAssets.Where(a => Path.GetFileName(a) == "aloha-package-dependency.json").ToArray();
            PackageDependencyImporter.UpdatePackageManifest(dependencies);
        }
    }
}