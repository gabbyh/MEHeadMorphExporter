﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ME3Explorer.Packages;
using ME3Explorer.Unreal;

using ME3Explorer.Unreal.Classes;

using MEMeshMorphExporter.Unreal;

using FBXWrapper;

namespace MEMeshMorphExporter.Exporters
{
    public class MeshExporter
    {
        private IMEPackage Pcc;

        public MeshExporter(string pccPath)
        {
            MEPackageHandler.Initialize();
            Pcc = MEPackageHandler.OpenMEPackage(pccPath);
        }

        public MeshExporter(IMEPackage pcc)
        {
            Pcc = pcc;
        }

        public void ExportMeshesToFbx(string targetDir)
        {
            if (Pcc != null)
            {
                var meshExpIndexes = Pcc.Exports.Select((value, index) => new { value, index })
                      .Where(z => z.value.ClassName == "SkeletalMesh")
                      .Select(z => z.index);

                foreach (int meshIndex in meshExpIndexes)
                {
                    IExportEntry meshExp = Pcc.Exports[meshIndex];
                    MESkeletalMesh skMesh = new MESkeletalMesh(Pcc, meshIndex);
                               
                    string fileDest = System.IO.Path.Combine(targetDir, meshExp.ObjectName + ".fbx");
                    ExportSkeletalMeshToFbx(skMesh, meshExp.ObjectName, fileDest);
                }               
            }
        }

        public int ExportMorphsToFbx(string targetDir)
        {
            int countMorphNotExported = 0;
            if (Pcc != null)
            {
                var morphExpIndexes = Pcc.Exports.Select((value, index) => new { value, index })
                      .Where(z => z.value.ClassName == "BioMorphFace")
                      .Select(z => z.index);

                
                foreach (int morphIndex in morphExpIndexes)
                {
                    IExportEntry morphExp = Pcc.Exports[morphIndex];
                    var morph = new MEMeshMorphExporter.Unreal.BioMorphFace(Pcc, morphIndex);
                    if (morph.IsExportable)
                    {
                        string fileDest = System.IO.Path.Combine(targetDir, morph.Name + ".fbx");
                        var expMesh = morph.Apply();
                        ExportMeshWithMorph(morph, 0, fileDest);
                    }
                    else
                        countMorphNotExported++;
                }
            }
            return countMorphNotExported;
        }

        public void ExportMorphsToJson(string targetDir)
        {
            if (Pcc != null)
            {
                var morphExpIndexes = Pcc.Exports.Select((value, index) => new { value, index })
                      .Where(z => z.value.ClassName == "BioMorphFace")
                      .Select(z => z.index);

                foreach (int morphIndex in morphExpIndexes)
                {
                    IExportEntry morphExp = Pcc.Exports[morphIndex];
                    var morph = new MEMeshMorphExporter.Unreal.BioMorphFace(Pcc, morphIndex);

                    string fileDest = System.IO.Path.Combine(targetDir, morph.Name + ".json");
                    morph.ExportToJson(fileDest);
                }
            }
        }

        public void ExportSkeletalMeshToFbx(MESkeletalMesh mesh, string meshName, string targetdir)
        {
            FBXManager lSdkManager = new FBXManager();
            FBXScene lScene = new FBXScene();
            FBXHelper.InitializeSdkObjects(lSdkManager, lScene);
            FBXNode SceneRoot = lScene.GetRootNode();
            FBXNode lSkeletonRoot = CreateFbxSkeleton(mesh.Bones, lScene);
            SceneRoot.AddChild(lSkeletonRoot);
            try
            {
                FBXNode fbxMesh = CreateFbxMesh(mesh, meshName, lScene);
                CreateMeshSkinning(mesh, 0, fbxMesh, lSkeletonRoot, lScene);
                SceneRoot.AddChild(fbxMesh);
                //StoreRestPose(lScene, lSkeletonRoot, skeleton);                          
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            bool lResult = FBXHelper.SaveScene(lSdkManager, lScene, targetdir);
            FBXHelper.DestroySdkObjects(lSdkManager, lResult);
            
        }

        public void ExportMeshWithMorph(Unreal.BioMorphFace morph, int lodIndex, String targetdir)
        {
            FBXManager lSdkManager = new FBXManager();
            FBXScene lScene = new FBXScene();
            FBXHelper.InitializeSdkObjects(lSdkManager, lScene);
            FBXNode SceneRoot = lScene.GetRootNode();
            MESkeletalMesh mesh = morph.Apply();
            FBXNode fbxMesh = CreateFbxMesh(mesh, morph.Name, lScene);
            SceneRoot.AddChild(fbxMesh);
            if (mesh.Bones != null)
            {
                FBXNode lSkeletonRoot = CreateFbxSkeleton(mesh.Bones, lScene);
                CreateMeshSkinning(mesh, lodIndex, fbxMesh, lSkeletonRoot, lScene);
                UpdateSkeletonWithMorph(morph, lSkeletonRoot);
                SceneRoot.AddChild(lSkeletonRoot);
                //StoreBindPose(lScene, fbxMesh);
            }
            bool lResult = FBXHelper.SaveScene(lSdkManager, lScene, targetdir);
            FBXHelper.DestroySdkObjects(lSdkManager, lResult);
        }

        private void UpdateSkeletonWithMorph(Unreal.BioMorphFace morph, FBXNode pSkeletonNode)
        {
            foreach (var bo in morph.BonesOffset)
            {
                FBXNode fbxBone = pSkeletonNode.FindChild(bo.BoneName);
                List<double> tmp = fbxBone.LclTranslation;
                tmp[0] = bo.Offset.X;
                tmp[1] = -bo.Offset.Y;
                tmp[2] = bo.Offset.Z;
                fbxBone.LclTranslation = tmp;
            }
        }

        private FBXNode CreateFbxMesh(MESkeletalMesh mesh, string mname, FBXScene pScene, float exportScale = 1.0f)
        {
            MESkeletalMesh.LODModelStruct lod = mesh.LODModels[0];
            FBXMesh fbxMesh = FBXMesh.Create(pScene, mname);
            FBXNode lMeshNode = FBXNode.Create(pScene, mname);
            lMeshNode.SetNodeAttribute(fbxMesh);
            FBXGeometryElementTangent lGeometryElementTangent = fbxMesh.CreateElementTangent();
            lGeometryElementTangent.SetMappingMode(FBXWrapper.MappingMode.eByControlPoint);
            FBXGeometryElementMaterial lMaterialElement = fbxMesh.CreateElementMaterial();
            lMaterialElement.SetMappingMode(FBXWrapper.MappingMode.eByPolygon);
            lMaterialElement.SetReferenceMode(FBXWrapper.ReferenceMode.eIndexToDirect);
            fbxMesh.InitControlPoints(lod.NumVertices);

            // init UV maps
            FBXGeometryElementUV[] UVs = new FBXGeometryElementUV[lod.Sections.Count];
            for (int s = 0; s < lod.Sections.Count; s++)
            {
                string matName = GetMatName(mesh, lod, s);
                UVs[s] = fbxMesh.CreateElementUV(matName);
                UVs[s].SetMappingMode(FBXWrapper.MappingMode.eByControlPoint);
            }

            // vertices
            for (int j = 0; j < lod.VertexBufferGPUSkin.Vertices.Count; j++)
            {
                var vertex = lod.VertexBufferGPUSkin.Vertices[j];
                FBXVector4 position = new FBXVector4(vertex.Position.X * exportScale, -vertex.Position.Y * exportScale, vertex.Position.Z * exportScale, 0);
                FBXVector4 normal = new FBXVector4(vertex.TangentX, 0, vertex.TangentZ, 0);
                fbxMesh.SetControlPoint(j, position);
                lGeometryElementTangent.Add(normal);

                // uvs
                for (int s = 0; s < lod.Sections.Count; s++)
                {
                    var sectionVerts = GetSectionVertices(mesh, 0, s);
                    if (sectionVerts.Contains(j))
                    {
                        FBXVector4 texCoords = new FBXVector4(vertex.U, 1 - vertex.V, 0, 0);
                        UVs[s].Add(texCoords);
                    }
                    else
                    {
                        UVs[s].Add(new FBXVector4(0, 0, 0, 0));
                    }
                }
            }

            // faces & mats
            for (int s = 0; s < lod.Sections.Count; s++)
            {
                int chunkId = lod.Sections[s].ChunkIndex;
                var chunk = lod.Chunks[chunkId];
                // mat
                string matName = GetMatName(mesh, lod, s);
                lMeshNode.AddMaterial(pScene, matName);
                // faces
                for (int j = 0 ; j < lod.Sections[s].NumTriangles; j++)
                {
                    
                    int baseI = lod.Sections[s].BaseIndex;
                    fbxMesh.BeginPolygon(s);
                    fbxMesh.AddPolygon(lod.IndexBuffer.Indexes[baseI + j*3]);
                    fbxMesh.AddPolygon(lod.IndexBuffer.Indexes[baseI + j*3 + 1]);
                    fbxMesh.AddPolygon(lod.IndexBuffer.Indexes[baseI + j*3 + 2]);
                    fbxMesh.EndPolygon();
                }
            }
            return lMeshNode;
        }

        private FBXNode CreateFbxSkeleton(List<MESkeletalMesh.BoneStruct> Bones, FBXScene pScene)
        {
            FBXSkeleton lSkeletonRootAttribute = FBXSkeleton.Create(pScene, "Skeleton");
            lSkeletonRootAttribute.SetSkeletonType(FBXWrapper.SkelType.eRoot);
            FBXNode lSkeletonRoot = FBXNode.Create(pScene, "Skeleton");
            lSkeletonRoot.SetNodeAttribute(lSkeletonRootAttribute);
            lSkeletonRoot.LclTranslation = new FBXVector4().ToList();
            FBXNode FbxSkeletonRootNode = CreateFbxBone(0, Bones, pScene, lSkeletonRoot);
            lSkeletonRoot.AddChild(FbxSkeletonRootNode);
            return lSkeletonRoot;
        }

        private FBXNode CreateFbxBone(int boneIndex, List<MESkeletalMesh.BoneStruct> Skeleton, FBXScene pScene, FBXNode parent)
        {
            MESkeletalMesh.BoneStruct bone = Skeleton[boneIndex];
            string boneName = bone.BoneName;
            FBXSkeleton lSkeletonLimbNodeAttribute1 = FBXSkeleton.Create(pScene, boneName);
            lSkeletonLimbNodeAttribute1.SetSkeletonType(FBXWrapper.SkelType.eLimbNode);
            lSkeletonLimbNodeAttribute1.SetSize(1.0);
            FBXNode lSkeletonLimbNode1 = FBXNode.Create(pScene, boneName);
            lSkeletonLimbNode1.SetNodeAttribute(lSkeletonLimbNodeAttribute1);

            lSkeletonLimbNode1.LclTranslation = new List<double> { bone.Position.X, -bone.Position.Y, bone.Position.Z };

            ME3Explorer.Unreal.Classes.SkeletalMeshOld.Quad boneQuad;
            boneQuad.x = -bone.Orientation.X;
            boneQuad.y = bone.Orientation.Y;
            boneQuad.z = bone.Orientation.Z;
            boneQuad.w = -bone.Orientation.W;
            if (boneIndex == 0)
            {
                boneQuad.w = boneQuad.w * -1;
            }
            Rotator rot = QuatToRotator(boneQuad);
            lSkeletonLimbNode1.LclRotation = new List<double> { rot.Roll, rot.Pitch, rot.Yaw };

            var children = Skeleton.Where(b => b.Parent == boneIndex).ToList();

            foreach (var childBone in children)
            {
                int childIndexInSkeleton = Skeleton.FindIndex(b => b.Name == childBone.Name);
                if (childIndexInSkeleton != boneIndex)
                {
                    FBXNode fbxChildBone = CreateFbxBone(childIndexInSkeleton, Skeleton, pScene, lSkeletonLimbNode1);
                    lSkeletonLimbNode1.AddChild(fbxChildBone);
                }
            }
            return lSkeletonLimbNode1;
        }

        // skinning
        private void CreateMeshSkinning(Dictionary<string, List<VertexWeight>> vg, FBXNode pFbxMesh, FBXNode pSkeletonRoot, FBXScene pScene)
        {
            FBXSkin lFbxSkin = FBXSkin.Create(pScene, "");
            FBXAMatrix lMeshMatTransform = pFbxMesh.EvaluateGlobalTransform();
            foreach (string key in vg.Keys)
            {
                List<VertexWeight> bvg = vg[key];
                FBXCluster lCluster = FBXCluster.Create(pScene, key);
                FBXNode lFbxBone = pSkeletonRoot.FindChild(key);
                if (lFbxBone != null)
                {
                    lCluster.SetLink(lFbxBone);
                    foreach (VertexWeight v in bvg)
                        lCluster.AddControlPointIndex(v.vertexIndex, v.weight);
                    lFbxSkin.AddCluster(lCluster);
                    lCluster.SetTransformMatrix(lMeshMatTransform);
                    FBXAMatrix lBoneMatTransform = lFbxBone.EvaluateGlobalTransform();
                    lCluster.SetTransformLinkMatrix(lBoneMatTransform);
                }
            }

            FBXGeometry lFbxMeshAtt = (FBXGeometry)pFbxMesh.GetNodeAttribute().ToGeometry();
            if (lFbxMeshAtt != null)
                lFbxMeshAtt.AddDeformer(lFbxSkin);
        }

        // SkeletalMesh
        private void CreateMeshSkinning(MESkeletalMesh mesh, int lod, FBXNode pFbxMesh, FBXNode pSkeletonRoot, FBXScene pScene)
        {
            var vg = GetVertexGroups(mesh, lod);
            CreateMeshSkinning(vg, pFbxMesh, pSkeletonRoot, pScene);
        }

        private Dictionary<string, List<VertexWeight>> GetVertexGroups(MESkeletalMesh mesh, int lodIndex=0)
        {
            Dictionary<string, List<VertexWeight>> VertexGroups = new Dictionary<string, List<VertexWeight>>();
            var lod = mesh.LODModels[lodIndex];


            for (int c = 0; c < lod.Chunks.Count; c++) 
            {
                var chunk = lod.Chunks[c];
                for (int v=chunk.BaseVertexIndex; v < chunk.BaseVertexIndex + chunk.NumRigidVertices + chunk.NumSoftVertices; v++) 
                {
                    var vertex = lod.VertexBufferGPUSkin.Vertices[v];
                    for (int x = 0; x < vertex.InfluenceWeights.Length; x++)
                    {
                        try
                        {
                            float weight = vertex.InfluenceWeights[x] / 255f;
                            if (weight != 0)
                            {
                                int bone = vertex.InfluenceBones[x];
                                bone = chunk.BoneMap[bone];
                                string boneName = mesh.Bones[bone].BoneName;
                                VertexWeight vw;
                                vw.vertexIndex = v;
                                vw.weight = weight;
                                if (!VertexGroups.ContainsKey(boneName))
                                {
                                    VertexGroups.Add(boneName, new List<VertexWeight>());
                                }
                                VertexGroups[boneName].Add(vw);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                }
            }
            return VertexGroups;
        }

        // Utils
        struct VertexWeight
        {
            public int vertexIndex;
            public float weight;
            
        }
        struct Rotator
        {
            public float Yaw;
            public float Pitch;
            public float Roll;
        }

        // calculation rules found in UE4 code. Can't be too far from UDK after all.
        private Rotator QuatToRotator(ME3Explorer.Unreal.Classes.SkeletalMeshOld.Quad quat)
        {
            float X = quat.x;
            float Y = quat.y;
            float Z = quat.z;
            float W = quat.w;

            float SingularityTest = Z*X-W*Y;
            float YawY = 2f*(W*Z+X*Y);
            float YawX = (1f-2f*(Y*Y + Z*Z));

            
            const float SINGULARITY_THRESHOLD = 0.4999995f;
            const float RAD_TO_DEG = (float) ((180)/ Math.PI);
            Rotator RotatorFromQuat;

            if (SingularityTest < -SINGULARITY_THRESHOLD)
            {
                RotatorFromQuat.Pitch = -90f;
                RotatorFromQuat.Yaw = (float) Math.Atan2(YawY, YawX) * RAD_TO_DEG;
                RotatorFromQuat.Roll = NormalizeAxis((float)(-RotatorFromQuat.Yaw - (2f * Math.Atan2(X, W) * RAD_TO_DEG)));
            }
            else if (SingularityTest > SINGULARITY_THRESHOLD)
            {
                RotatorFromQuat.Pitch = 90f;
                RotatorFromQuat.Yaw = (float) Math.Atan2(YawY, YawX) * RAD_TO_DEG;
                RotatorFromQuat.Roll = NormalizeAxis((float)(RotatorFromQuat.Yaw - (2f * Math.Atan2(X, W) * RAD_TO_DEG)));
            }
            else
            {
                RotatorFromQuat.Pitch = (float) Math.Asin(2f*(SingularityTest)) * RAD_TO_DEG;
                RotatorFromQuat.Yaw = (float) Math.Atan2(YawY, YawX) * RAD_TO_DEG;
                RotatorFromQuat.Roll = (float) Math.Atan2(-2f*(W*X+Y*Z), (1f-2f*(X*X + Y*Y))) * RAD_TO_DEG;
            }
            return RotatorFromQuat;
        }

        float NormalizeAxis(float Angle)
        {
            // returns Angle in the range (-360,360)
            Angle = Fmod(Angle, 360);

            if (Angle < 0f)
            {
                // shift to [0,360) range
                Angle += 360f;
            }

            if (Angle > 180f)
            {
                // shift to (-180,180]
                Angle -= 360f;
            }
            return Angle;
        }

        float Fmod(float X, float Y)
        {
            if (Math.Abs(Y) <= 1E-8f)
            {
                return 0;
            }
            float Quotient = (float)((int)(X / Y));
            float IntPortion = Y * Quotient;

            // Rounding and imprecision could cause IntPortion to exceed X and cause the result to be outside the expected range.
            // For example Fmod(55.8, 9.3) would result in a very small negative value!
            if (Math.Abs(IntPortion) > Math.Abs(X))
            {
                IntPortion = X;
            }

            float Result = X - IntPortion;
            return Result;
        }

        private List<int> GetSectionVertices(MESkeletalMesh mesh, int lod, int section)
        {
            var vertices = new HashSet<int>();
            var cLOD = mesh.LODModels[lod];
            for (int i = 0; i < cLOD.Sections[section].NumTriangles * 3; i++)
            {
                vertices.Add(cLOD.IndexBuffer.Indexes[(int)cLOD.Sections[section].BaseIndex + i]);
            }
            return vertices.ToList();
        }

        private string GetMatName(MESkeletalMesh mesh, MESkeletalMesh.LODModelStruct lod, int s)
        {
            if (lod.Sections[s].MaterialIndex < mesh.Materials.Count)
            {
                if (mesh.Owner.isExport(mesh.Materials[lod.Sections[s].MaterialIndex] - 1)) 
                {
                    return mesh.Owner.Exports[mesh.Materials[lod.Sections[s].MaterialIndex] - 1].ObjectName;
                }
                else if (mesh.Owner.isImport(-mesh.Materials[lod.Sections[s].MaterialIndex] - 1)) 
                {
                    return mesh.Owner.Imports[-mesh.Materials[lod.Sections[s].MaterialIndex] - 1].ObjectName;
                }
                else
                {
                    return "mat_" + s; 
                }
            }
            else
            {
                return "mat_" + s;
            }

        }
    }
}
