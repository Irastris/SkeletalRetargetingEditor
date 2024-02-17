using UAssetAPI;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;

namespace SkeletalRetargetingEditor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void AttachContextMenuToNode(TreeNode node)
        {
            ContextMenuStrip ctxMenu = new ContextMenuStrip();
            EventHandler clickHandler = new EventHandler(ctxMenuItem_Click);
            ToolStripMenuItem item1 = new ToolStripMenuItem("Set Retargeting Mode: Animation", null, clickHandler, "Animation");
            ToolStripMenuItem item2 = new ToolStripMenuItem("Set Retargeting Mode: AnimationRelative", null, clickHandler, "AnimationRelative");
            ToolStripMenuItem item3 = new ToolStripMenuItem("Set Retargeting Mode: AnimationScaled", null, clickHandler, "AnimationScaled");
            ToolStripMenuItem item4 = new ToolStripMenuItem("Set Retargeting Mode: Skeleton", null, clickHandler, "Skeleton");
            ToolStripSeparator separator = new ToolStripSeparator();
            ToolStripMenuItem item5 = new ToolStripMenuItem("Recursively Set Retargeting Mode: Animation", null, clickHandler, "RecursiveAnimation");
            ToolStripMenuItem item6 = new ToolStripMenuItem("Recursively Set Retargeting Mode: AnimationRelative", null, clickHandler, "RecursiveAnimationRelative");
            ToolStripMenuItem item7 = new ToolStripMenuItem("Recursively Set Retargeting Mode: AnimationScaled", null, clickHandler, "RecursiveAnimationScaled");
            ToolStripMenuItem item8 = new ToolStripMenuItem("Recursively Set Retargeting Mode: Skeleton", null, clickHandler, "RecursiveSkeleton");
            ctxMenu.Items.AddRange(new ToolStripItem[]{ item1, item2, item3, item4, separator, item5, item6, item7, item8 });
            node.ContextMenuStrip = ctxMenu;
        }

        private void InitializeTreeView(TreeView treeView)
        {
            Dictionary<string, int> bones = Utilities.GetBoneNameIndexPairs();
            Dictionary<string, int> parents = Utilities.GetBoneParentIndexes();

            foreach (KeyValuePair<string, int> kvp in parents)
            {
                TreeNode node = new TreeNode(kvp.Key);
                node.Name = kvp.Key;
                node.Tag = Utilities.GetBoneRetargetingMode(bones[kvp.Key]);
                if (kvp.Value == -1)
                {
                    treeView1.Nodes.Add(node);
                    continue;
                }
                AttachContextMenuToNode(node);
                TreeNode[] parentNode = treeView1.Nodes.Find(bones.ElementAt(kvp.Value).Key, true);
                parentNode[0].Nodes.Add(node);
            }

            foreach(KeyValuePair<string, int> kvp in bones)
            {
                if (kvp.Value != 0)
                {
                    TreeNode[] node = treeView1.Nodes.Find(kvp.Key, true);
                    node[0].Tag = Utilities.GetBoneRetargetingMode(kvp.Value);
                    node[0].Text = $"({node[0].Tag}) {node[0].Name}";
                }
            }

            treeView1.ExpandAll();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (OpenFileDialog skeletonOpenDialog = new OpenFileDialog())
            {
                skeletonOpenDialog.Title = "Open Cooked Skeleton .uasset";
                skeletonOpenDialog.Filter = ".uasset (*.uasset)|*.uasset";
                skeletonOpenDialog.RestoreDirectory = true;

                if (skeletonOpenDialog.ShowDialog() == DialogResult.OK)
                {
                    Program.skeletonPath = Path.GetDirectoryName(skeletonOpenDialog.FileName);
                    Program.skeletonName = Path.GetFileNameWithoutExtension(skeletonOpenDialog.FileName);
                }
                else
                {
                    Application.Exit();
                }
            }

            using (OpenFileDialog mappingsOpenDialog = new OpenFileDialog())
            {
                mappingsOpenDialog.Title = "Open Mappings .usmap";
                mappingsOpenDialog.Filter = ".usmap (*.usmap)|*.usmap";
                mappingsOpenDialog.RestoreDirectory = true;

                if (mappingsOpenDialog.ShowDialog() == DialogResult.OK)
                {
                    Program.mappingsPath = mappingsOpenDialog.FileName;
                }
                else
                {
                    Application.Exit();
                }
            }

            Program.mappings = new Usmap(Program.mappingsPath);
            Program.skeleton = new UAsset(Path.Combine(Program.skeletonPath, $"{Program.skeletonName}.uasset"), EngineVersion.VER_UE5_1, Program.mappings);

            InitializeTreeView(treeView1);
            treeView1.Nodes[0].EnsureVisible();
            treeView1.NodeMouseClick += (sender, args) => treeView1.SelectedNode = args.Node;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog skeletonSaveDialog = new SaveFileDialog())
            {
                skeletonSaveDialog.Title = "Save Modified Skeleton .uasset";
                skeletonSaveDialog.Filter = ".uasset (*.uasset)|*.uasset";
                skeletonSaveDialog.RestoreDirectory = true;

                if (skeletonSaveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (skeletonSaveDialog.FileName != Path.Combine(Program.skeletonPath, $"{Program.skeletonName}.uasset"))
                    {
                        Program.skeleton.Write(skeletonSaveDialog.FileName);
                    }
                    else
                    {
                        MessageBox.Show("Skeletal Retargeting Editor does not support overwriting the original .uasset, saving has been canceled.\n\nPlease try again and specify a new path.", "Error While Saving", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ctxMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            switch (clickedItem.Name)
            {
                case "Animation":
                case "AnimationRelative":
                case "AnimationScaled":
                case "Skeleton":
                    // Console.WriteLine($"Setting {treeView1.SelectedNode.Name} to {clickedItem.Name}");
                    node.Tag = clickedItem.Name;
                    node.Text = $"({node.Tag}) {node.Name}";
                    break;

                case "RecursiveAnimation":
                case "RecursiveAnimationRelative":
                case "RecursiveAnimationScaled":
                case "RecursiveSkeleton":
                    string mode = clickedItem.Name.Split("Recursive")[1];
                    // Console.WriteLine($"Setting {treeView1.SelectedNode.Name} to {mode} recursively");
                    node.Tag = mode;
                    node.Text = $"({node.Tag}) {node.Name}";
                    Utilities.SetRetargetingModeRecursively(node, mode);
                    break;

                default:
                    break;
            }
        }
    }
}
