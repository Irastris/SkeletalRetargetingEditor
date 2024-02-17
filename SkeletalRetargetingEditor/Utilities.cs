using CUE4Parse;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Assets;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Animation;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using System.Windows.Forms;
using UAssetAPI;
using UAssetAPI.ExportTypes;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.PropertyTypes.Structs;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace SkeletalRetargetingEditor
{
    internal class Utilities
    {
        public static UObject LoadExportFromLoose(string skeletonPath, string skeletonName, string mappingsPath)
        {
            TypeMappings? cue4mappings = new FileUsmapTypeMappingsProvider(Program.mappingsPath).MappingsForGame;
            DirectoryInfo workingDirectory = new DirectoryInfo(Program.skeletonPath);
            FileInfo uassetFile = new FileInfo(Path.Combine(Program.skeletonPath, $"{Program.skeletonName}.uasset"));
            FileInfo uexpFile = new FileInfo(Path.Combine(Program.skeletonPath, $"{Program.skeletonName}.uexp"));
            FArchive uasset = new OsGameFile(workingDirectory, uassetFile, Program.skeletonPath, new VersionContainer(EGame.GAME_UE5_1)).CreateReader();
            FArchive uexp = new OsGameFile(workingDirectory, uexpFile, Program.skeletonPath, new VersionContainer(EGame.GAME_UE5_1)).CreateReader();
            Package pkg = new Package(uasset, uexp, (FArchive)null, mappings: cue4mappings);

            return pkg.GetExport(Program.skeletonName, StringComparison.OrdinalIgnoreCase);
        }

        public static Dictionary<string, int> GetBoneNameIndexPairs()
        {
            dynamic obj = LoadExportFromLoose(Program.skeletonPath, Program.skeletonName, Program.mappingsPath);
            Dictionary<string, int> nameMap = obj.ReferenceSkeleton.FinalNameToIndexMap;

            return nameMap.ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<string, int> GetBoneParentIndexes()
        {
            dynamic obj = LoadExportFromLoose(Program.skeletonPath, Program.skeletonName, Program.mappingsPath);
            FMeshBoneInfo[] boneInfoArray = obj.ReferenceSkeleton.FinalRefBoneInfo;

            return boneInfoArray.ToDictionary(x => x.Name.ToString(), x => x.ParentIndex);
        }

        public static string GetBoneRetargetingMode(int boneIndex)
        {
            foreach (NormalExport normalExport in Program.skeleton.Exports)
            {
                if (normalExport.ObjectName.ToString() != Program.skeletonName) continue;
                foreach (ArrayPropertyData arrayPropertyData in normalExport.Data)
                {
                    if (arrayPropertyData.Name.ToString() != "BoneTree") continue;
                    foreach (StructPropertyData structPropertyData in arrayPropertyData.Value)
                    {
                        if (structPropertyData.StructType.ToString() != "BoneNode") continue;
                        EnumPropertyData enumPropertyData = (EnumPropertyData)structPropertyData.Value[0];
                        if (enumPropertyData.Name.ToString() != "TranslationRetargetingMode") continue;
                        return enumPropertyData.Value == null ? "Animation" : enumPropertyData.Value.ToString();
                    }
                }
            }
            return null;
        }

        public static void SetRetargetingModeRecursively(TreeNode node, string mode)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Tag = mode;
                child.Text = $"({child.Tag}) {child.Name}";
                if (child.Nodes.Count > 0)
                {
                    SetRetargetingModeRecursively(child, mode);
                }
            }
        }

        public static string UnusedFunction()
        {
            foreach (NormalExport normalExport in Program.skeleton.Exports)
            {
                if (normalExport.ObjectName.ToString() != Program.skeletonName) continue;
                foreach (ArrayPropertyData arrayPropertyData in normalExport.Data)
                {
                    if (arrayPropertyData.Name.ToString() != "BoneTree") continue;
                    foreach (StructPropertyData structPropertyData in arrayPropertyData.Value)
                    {
                        if (structPropertyData.StructType.ToString() != "BoneNode") continue;
                        foreach (EnumPropertyData enumPropertyData in structPropertyData.Value)
                        {
                            if (enumPropertyData.Name.ToString() != "TranslationRetargetingMode") continue;
                            return enumPropertyData.Value == null ? "Animation" : enumPropertyData.Value.ToString();
                        }
                    }
                }
            }
            return null;
        }
    }
}
