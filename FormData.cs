using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

using MEMeshMorphExporter.Model;

using ME3Explorer.Packages;
using ME3Explorer.Unreal.Classes;

namespace MEMeshMorphExporter
{
    class FormData
    {
        private bool _isPackageHandlerInitialized = false;

        public TreeNode BuildPccTree(string pcc)
        {
            if (!_isPackageHandlerInitialized)
            {
                MEPackageHandler.Initialize();
                _isPackageHandlerInitialized = true; ;
            }
            var Pcc = MEPackageHandler.OpenMEPackage(pcc);

            /*
            ME3Package me3Pcc = Pcc as ME3Package;

            if (me3Pcc == null)
            {
                // supports only ME3 for the time being
                throw new ArgumentException("This is not a ME3 package. The tool only supports ME3 at this time.");
            }
             * */
            
            PccTreeNode pccRootNode = null;

            var MorphExpIndexes = Pcc.Exports.Select((value, index) => new { value, index })
                      .Where(z => z.value.ClassName == "BioMorphFace")
                      .Select(z => z.index);

            if (MorphExpIndexes.Count() > 0)
            {
                pccRootNode = new PccTreeNode(Pcc);

                // Morphs
                TreeNode ParentMorphNode = new TreeNode();
                ParentMorphNode.Text = "BioMorphFace";

                foreach (int morphIndex in MorphExpIndexes)
                {
                    var morph = new Unreal.BioMorphFace(Pcc, morphIndex);
                    MorphTreeNode morphNode = new MorphTreeNode(morph);
                    ParentMorphNode.Nodes.Add(morphNode);
                }

                // Meshes
                TreeNode ParentMeshNode = new TreeNode();
                ParentMeshNode.Text = "SkeletalMesh";

                var MeshExpIndexes = Pcc.Exports.Select((value, index) => new { value, index })
                          .Where(z => z.value.ClassName == "SkeletalMesh")
                          .Select(z => z.index);

                foreach (int meshIndex in MeshExpIndexes)
                {
                    IExportEntry exp = Pcc.Exports[meshIndex];
                    //SkeletalMesh skMesh = new SkeletalMesh((ME3Package)Pcc, meshIndex);
                    var skMesh = new Unreal.MESkeletalMesh(Pcc, meshIndex);
                    MeshTreeNode meshNode = new MeshTreeNode(skMesh, exp);
                    ParentMeshNode.Nodes.Add(meshNode);
                }

                // build hierarchy
                pccRootNode.Nodes.Add(ParentMeshNode);
                pccRootNode.Nodes.Add(ParentMorphNode);
            }
            else
            {
                Pcc = null;
            }
            return pccRootNode;
        }
    }

    interface IDisplayNode
    {
        void DisplayDetails(Control target);
        string GetType();
    }

    class PccTreeNode : TreeNode, IDisplayNode
    {
        public IMEPackage pcc;

        public PccTreeNode(IMEPackage aPcc)
        {
            pcc = aPcc;
            Text = System.IO.Path.GetFileName(pcc.FileName);
        }

        public void DisplayDetails(Control target)
        {
            // do nothing
        }

        string IDisplayNode.GetType()
        {
            return "pcc";
        }
    }

    class MorphTreeNode : TreeNode, IDisplayNode
    {
        public Unreal.BioMorphFace morph;

        public MorphTreeNode(Unreal.BioMorphFace aMorph)
        {
            morph = aMorph;
            Text = morph.Name;
        }

        public void DisplayDetails(Control target)
        {
            try
            {
                TreeView tv = target as TreeView;
                tv.Nodes.Clear();
                TreeNode JsonTree = JsonToTree.ToNode(morph);
                tv.Nodes.Add(JsonTree);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }    
        }

        string IDisplayNode.GetType()
        {
            return "morph";
        }

        public PccTreeNode GetParentNode()
        {
            return (PccTreeNode)Parent.Parent;
        }
    }

    class MeshTreeNode : TreeNode, IDisplayNode
    {
        public Unreal.MESkeletalMesh mesh;

        public MeshTreeNode(Unreal.MESkeletalMesh aMesh, IExportEntry exp)
        {
            mesh = aMesh;
            Text = exp.ObjectName;
        }

        public void DisplayDetails(Control target)
        {
            TreeView tv = target as TreeView;
            tv.Nodes.Clear();
            tv.Nodes.Add(mesh.ToTree());
        }

        string IDisplayNode.GetType()
        {
            return "mesh";
        }

        public PccTreeNode GetParentNode()
        {
            return (PccTreeNode)Parent.Parent;
        }
    }
}
