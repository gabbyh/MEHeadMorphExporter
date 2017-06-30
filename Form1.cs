using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEMeshMorphExporter
{
    public partial class Form1 : Form
    {
        private FormData data;

        public Form1()
        {
            InitializeComponent();
            data = new FormData();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void openPccToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Title = "Select game package on disk";
            opf.Filter = "Mass Effect Package|*.pcc;*.sfm;*.upk";

            if (opf.ShowDialog() == DialogResult.OK)
            {
                string pccPath = opf.FileName;
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    TreeNode node = data.BuildPccTree(pccPath);
                    if (node != null)
                    {
                        LeftTree.Nodes.Add(node);
                        LeftTree.Refresh();
                    }
                    else
                    {
                        MessageBox.Show("No morph found in this pcc file");
                    }
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    this.Cursor = Cursors.Default;    
                }                        
            }
        }

        private void exportMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MeshTreeNode meshNode = LeftTree.SelectedNode as MeshTreeNode;
            if (meshNode != null)
            {
                var pccNode = meshNode.GetParentNode();
                var exporter = new MEMeshMorphExporter.Exporters.MeshExporter(pccNode.pcc);
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Save mesh as fbx...";
                sfd.Filter = "*.fbx|.fbx";
                sfd.FileName = meshNode.Text + ".fbx";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    exporter.ExportSkeletalMeshToFbx(meshNode.mesh, meshNode.Text, sfd.FileName);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Done.");
                }
            }          
        }

        // on select in the list tree
        private void LeftTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            exportMeshToolStripMenuItem.Enabled = false;
            exportMorphToolStripMenuItem.Enabled = false;
            exportAllPccMeshesToolStripMenuItem.Enabled = false;
            exportAllPccMorphsToolStripMenuItem.Enabled = false;
            LeftTreeContextMenuStrip.Visible = false;
            exportContextMenuItem.Visible = false;

            IDisplayNode nodeToDisplay = LeftTree.SelectedNode as IDisplayNode;
            if (nodeToDisplay != null)
            {
                nodeToDisplay.DisplayDetails(DetailsTreeView);

                switch (nodeToDisplay.GetType())
                {
                    case "pcc":
                        exportMeshToolStripMenuItem.Enabled = false;
                        exportMorphToolStripMenuItem.Enabled = false;
                        exportAllPccMeshesToolStripMenuItem.Enabled = true;
                        exportAllPccMorphsToolStripMenuItem.Enabled = true;
                        break;
                    case "morph":
                        exportMeshToolStripMenuItem.Enabled = false;
                        exportMorphToolStripMenuItem.Enabled = true;
                        exportAllPccMeshesToolStripMenuItem.Enabled = false;
                        exportAllPccMorphsToolStripMenuItem.Enabled = false;
                        LeftTreeContextMenuStrip.Visible = true;
                        exportContextMenuItem.Visible = true;
                        exportContextMenuItem.Enabled = true;
                        break;
                    case "mesh":
                        exportMeshToolStripMenuItem.Enabled = true;
                        exportMorphToolStripMenuItem.Enabled = false;
                        exportAllPccMeshesToolStripMenuItem.Enabled = false;
                        exportAllPccMorphsToolStripMenuItem.Enabled = false;
                        LeftTreeContextMenuStrip.Visible = true;
                        exportContextMenuItem.Visible = true;
                        exportContextMenuItem.Enabled = true;
                        break;
                }
            }
        }

        private void exportMorphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MorphTreeNode morphNode = LeftTree.SelectedNode as MorphTreeNode;
            if (morphNode != null)
            {
                var pccNode = morphNode.GetParentNode();
                
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Save morph as...";
                sfd.Filter = "*.fbx|.fbx|*.json|*.json";
                sfd.FileName = morphNode.Text + ".fbx";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    string ext = System.IO.Path.GetExtension(sfd.FileName);
                    if (ext.EndsWith("fbx"))
                    {
                        var exporter = new MEMeshMorphExporter.Exporters.MeshExporter(pccNode.pcc);
                        exporter.ExportMeshWithMorph(morphNode.morph, 0, sfd.FileName);
                    } 
                    else if (ext.EndsWith("json"))
                    {
                        morphNode.morph.ExportToJson(sfd.FileName);
                    }
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Done.");
                }
            }
        }

        private void exportAllPccMeshesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PccTreeNode pccNode = LeftTree.SelectedNode as PccTreeNode;
            if (pccNode != null)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select folder where to save the meshes";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    var exporter = new MEMeshMorphExporter.Exporters.MeshExporter(pccNode.pcc);
                    exporter.ExportMeshesToFbx(fbd.SelectedPath);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Done.");
                }
            }
        }

        // export all morphs as fbx
        private void aSFbxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PccTreeNode pccNode = LeftTree.SelectedNode as PccTreeNode;
            if (pccNode != null)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select folder where to save the morphs";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    var exporter = new MEMeshMorphExporter.Exporters.MeshExporter(pccNode.pcc);
                    exporter.ExportMorphsToFbx(fbd.SelectedPath);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Done.");
                }
            }
        }

        // Export all morphs as json
        private void aSJsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PccTreeNode pccNode = LeftTree.SelectedNode as PccTreeNode;
            if (pccNode != null)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select folder where to save the morphs";
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    this.Cursor = Cursors.WaitCursor;
                    var exporter = new MEMeshMorphExporter.Exporters.MeshExporter(pccNode.pcc);
                    exporter.ExportMorphsToJson(fbd.SelectedPath);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Done.");
                }
            }
        }

        // export all morph and all meshes from all pcc in the tree
        private void exportAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Beware, this operation may be very long  (hours long!). Proceed anyway?");
        }

        // export contextual menu
        private void exportContextMenuItem_Click(object sender, EventArgs e)
        {
            IDisplayNode selectedNode = LeftTree.SelectedNode as IDisplayNode;
            if (selectedNode != null)
            {
                if (selectedNode.GetType() == "mesh")
                {
                    exportMeshToolStripMenuItem_Click(sender, e);
                }
                else if (selectedNode.GetType() == "morph")
                {
                    exportMorphToolStripMenuItem_Click(sender, e);
                }
            }
        }
    }
}
