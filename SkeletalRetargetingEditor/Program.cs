using UAssetAPI;
using UAssetAPI.Unversioned;

namespace SkeletalRetargetingEditor
{
    internal class Program
    {
        public static string skeletonPath = string.Empty;
        public static string skeletonName = string.Empty;
        public static string mappingsPath = string.Empty;
        public static UAsset skeleton;
        public static Usmap mappings;

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}