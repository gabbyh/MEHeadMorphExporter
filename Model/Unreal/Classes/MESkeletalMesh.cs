﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using ME3Explorer.Packages;

using ME3Explorer;
using ME3Explorer.Unreal;
using ME3Explorer.Unreal.Classes;

namespace MEMeshMorphExporter.Unreal
{
    public enum MEVersion
    {
        ME1, 
        ME2,
        ME3
    }
    
    public class MESkeletalMesh
    {
        public struct BoundingStruct
        {
            public Vector3 origin;
            public Vector3 size;
            public float r;
        }

        public struct BoneStruct
        {
            public int Name;
            public int Flags;
            public int Unk1;
            public Vector4 Orientation;
            public Vector3 Position;
            public int NumChildren;
            public int Parent;
            public int BoneColor;

            public string BoneName;
        }

        public struct SectionStruct
        {
            public short MaterialIndex;
            public short ChunkIndex;
            public int BaseIndex;
            public short NumTriangles;
            public void Serialize(SerializingContainer Container, MEVersion version)
            {
                MaterialIndex = Container + MaterialIndex;
                ChunkIndex = Container + ChunkIndex;
                BaseIndex = Container + BaseIndex;
                NumTriangles = Container + NumTriangles;
                if (version == MEVersion.ME3) 
                {
                    short s = 0;
                    s = Container + s;
                }                
            }

            public TreeNode ToTree(int MyIndex)
            {
                TreeNode res = new TreeNode("Section " + MyIndex);
                res.Nodes.Add("Material Index : " + MaterialIndex);
                res.Nodes.Add("Chunk Index : " + ChunkIndex);
                res.Nodes.Add("Base Index : " + BaseIndex);
                res.Nodes.Add("Num Triangles : " + NumTriangles);            
                return res;
            }
        }

        public struct MultiSizeIndexContainerStruct
        {
            public int IndexSize;
            public int IndexCount;
            public List<ushort> Indexes;

            public void Serialize(SerializingContainer Container)
            {
                IndexSize = Container + IndexSize;
                IndexCount = Container + IndexCount;
                if (Container.isLoading)
                {
                    Indexes = new List<ushort>();
                    for (int i = 0; i < IndexCount; i++)
                        Indexes.Add(0);
                }
                for (int i = 0; i < IndexCount; i++)
                    Indexes[i] = Container + Indexes[i];
            }

            public TreeNode ToTree()
            {
                TreeNode res = new TreeNode("MultiSizeIndexContainer");
                res.Nodes.Add("IndexSize : " + IndexSize);
                res.Nodes.Add("IndexCount : " + IndexCount);
                TreeNode t = new TreeNode("Indexes");
                for (int i = 0; i < Indexes.Count; i++)
                    t.Nodes.Add(i + " : " + Indexes[i]);
                res.Nodes.Add(t);
                return res;
            }
        }

        public struct RigidSkinVertexStruct
        {
            public Vector3 Position;
            public int TangentX;
            public int TangentY;
            public int TangentZ;
            public Vector2[] UV;
            public int Color;
            public byte Bone;

            public int NumUVsets;

            public void Serialize(SerializingContainer Container)
            {
                NumUVsets = 1;
                
                Position.X = Container + Position.X;
                Position.Y = Container + Position.Y;
                Position.Z = Container + Position.Z;
                TangentX = Container + TangentX;
                TangentY = Container + TangentY;
                TangentZ = Container + TangentZ;

                if (Container.isLoading)
                    UV = new Vector2[NumUVsets];
                for (int i = 0; i < NumUVsets; i++)
                {
                    UV[i].X = Container + UV[i].X;
                    UV[i].Y = Container + UV[i].Y;
                }
                //Color = Container + Color;
                Bone = Container + Bone;
            }

            public TreeNode ToTree(int MyIndex)
            {
                string s = MyIndex + " : Position : X(";
                s += Position.X + ") Y(" + Position.Y + ") Z(" + Position.Z + ") ";
                s += "TangentX(" + TangentX.ToString("X8") + ") TangentY(" + TangentY.ToString("X8") + ") TangentZ(" + TangentZ.ToString("X8") + ") ";
                for (int i = 0; i < NumUVsets; i++)
                    s += "UV[" + i + "](" + UV[i].X + " " + UV[i].Y + ") ";
                s += "Color : " + Color.ToString("X8") + " Bone : " + Bone;
                return new TreeNode(s);
            }
        }

        public struct SoftSkinVertexStruct
        {
            public Vector3 Position;
            public int TangentX;
            public int TangentY;
            public int TangentZ;
            public Vector2[] UV;
            public int Color;
            public byte[] InfluenceBones;
            public byte[] InfluenceWeights;

            public int NumUVSet;

            public void Serialize(SerializingContainer Container)
            {
                NumUVSet = 1;

                Position.X = Container + Position.X;
                Position.Y = Container + Position.Y;
                Position.Z = Container + Position.Z;
                TangentX = Container + TangentX;
                TangentY = Container + TangentY;
                TangentZ = Container + TangentZ;
                if (Container.isLoading)
                {
                    UV = new Vector2[NumUVSet];
                    InfluenceBones = new byte[4];
                    InfluenceWeights = new byte[4];
                }
                for (int i = 0; i < NumUVSet; i++)
                {
                    UV[i].X = Container + UV[i].X;
                    UV[i].Y = Container + UV[i].Y;
                }
                //Color = Container + Color;
                for (int i = 0; i < 4; i++)
                    InfluenceBones[i] = Container + InfluenceBones[i];
                for (int i = 0; i < 4; i++)
                    InfluenceWeights[i] = Container + InfluenceWeights[i];
            }
            public TreeNode ToTree(int MyIndex)
            {
                string s = MyIndex + " : Position : X(";
                s += Position.X + ") Y(" + Position.Y + ") Z(" + Position.Z + ") ";
                s += "TangentX(" + TangentX.ToString("X8") + ") TangentY(" + TangentY.ToString("X8") + ") TangentZ(" + TangentZ.ToString("X8") + ") ";
                for (int i = 0; i < NumUVSet; i++)
                    s += "UV[" + i + "](" + UV[i].X + " " + UV[i].Y + ") ";
                s += "Color : " + Color.ToString("X8") + " InfluenceBones (";
                for (int i = 0; i < 3; i++)
                    s += InfluenceBones[i] + ", ";
                s += InfluenceBones[3] + ") InfluenceWeights (";
                for (int i = 0; i < 3; i++)
                    s += InfluenceWeights[i].ToString("X2") + ", ";
                s += InfluenceWeights[3].ToString("X2") + ")";
                return new TreeNode(s);
            }

        }

        public struct SkelMeshChunkStruct
        {
            public int BaseVertexIndex;
            public List<RigidSkinVertexStruct> RiginSkinVertices;
            public List<SoftSkinVertexStruct> SoftSkinVertices;
            public List<ushort> BoneMap;
            public int NumRigidVertices;
            public int NumSoftVertices;
            public int MaxBoneInfluences;

            public void Serialize(SerializingContainer Container)
            {
                //basevertex
                BaseVertexIndex = Container + BaseVertexIndex;
                //rigid vertices
                int count = 0;
                if (!Container.isLoading)
                    count = RiginSkinVertices.Count();
                count = Container + count;
                if (Container.isLoading)
                {
                    RiginSkinVertices = new List<RigidSkinVertexStruct>();
                    for (int i = 0; i < count; i++)
                        RiginSkinVertices.Add(new RigidSkinVertexStruct());
                }
                for (int i = 0; i < count; i++)
                {
                    RigidSkinVertexStruct v = RiginSkinVertices[i];
                    v.Serialize(Container);
                    RiginSkinVertices[i] = v;
                }
                //soft vertices
                if (!Container.isLoading)
                    count = SoftSkinVertices.Count();
                count = Container + count;
                if (Container.isLoading)
                {
                    SoftSkinVertices = new List<SoftSkinVertexStruct>();
                    for (int i = 0; i < count; i++)
                        SoftSkinVertices.Add(new SoftSkinVertexStruct());
                }
                for (int i = 0; i < count; i++)
                {
                    SoftSkinVertexStruct v = SoftSkinVertices[i];
                    v.Serialize(Container);
                    SoftSkinVertices[i] = v;
                }
                //bonemap
                if (!Container.isLoading)
                    count = BoneMap.Count();
                count = Container + count;
                if (Container.isLoading)
                {
                    BoneMap = new List<ushort>();
                    for (int i = 0; i < count; i++)
                        BoneMap.Add(0);
                }
                for (int i = 0; i < count; i++)
                    BoneMap[i] = Container + BoneMap[i];
                //rest
                NumRigidVertices = Container + NumRigidVertices;
                NumSoftVertices = Container + NumSoftVertices;
                MaxBoneInfluences = Container + MaxBoneInfluences;
            }

            public TreeNode ToTree(int MyIndex)
            {
                TreeNode res = new TreeNode("SkelMeshChunk " + MyIndex);
                res.Nodes.Add("Base Vertex Index : " + BaseVertexIndex);
                TreeNode t = new TreeNode("RigidSkinVertices (" + RiginSkinVertices.Count() + ")");
                for (int i = 0; i < RiginSkinVertices.Count; i++)
                    t.Nodes.Add(RiginSkinVertices[i].ToTree(i));
                res.Nodes.Add(t);
                t = new TreeNode("SoftSkinVertices (" + SoftSkinVertices.Count() + ")");
                for (int i = 0; i < SoftSkinVertices.Count; i++)
                    t.Nodes.Add(SoftSkinVertices[i].ToTree(i));
                res.Nodes.Add(t);
                t = new TreeNode("BoneMap (" + BoneMap.Count() + ")");
                for (int i = 0; i < BoneMap.Count; i++)
                    t.Nodes.Add(i + " : " + BoneMap[i]);
                res.Nodes.Add(t);
                res.Nodes.Add("NumRigidVertices : " + NumRigidVertices);
                res.Nodes.Add("NumSoftVertices : " + NumSoftVertices);
                res.Nodes.Add("MaxBoneInfluences : " + MaxBoneInfluences);
                return res;
            }
        }

        public struct GPUSkinVertexStruct
        {
            public int TangentX;
            public int TangentY; // used only in ME1 case
            public int TangentZ;
            public byte[] InfluenceBones;
            public byte[] InfluenceWeights;
            public Vector3 Position;
            public float U;
            public float V;

            float[] Normals;
            float[] Tangents;
            public void Serialize(SerializingContainer Container, MEVersion version)
            {
                if (version == MEVersion.ME2)
                {
                    Position.X = Container + Position.X;
                    Position.Y = Container + Position.Y;
                    Position.Z = Container + Position.Z;
                }
                TangentX = Container + TangentX;
                TangentZ = Container + TangentZ;
                if (Container.isLoading)
                {
                    InfluenceBones = new byte[4];
                    InfluenceWeights = new byte[4];
                }
                for (int i = 0; i < 4; i++)
                    InfluenceBones[i] = Container + InfluenceBones[i];
                for (int i = 0; i < 4; i++)
                    InfluenceWeights[i] = Container + InfluenceWeights[i];
                if (version == MEVersion.ME3)
                {
                    Position.X = Container + Position.X;
                    Position.Y = Container + Position.Y;
                    Position.Z = Container + Position.Z;
                }
                
                ushort sU = 0;
                ushort sV = 0;
                sU = Container + sU;
                sV = Container + sV;
                U = HalfToFloat(sU);
                V = HalfToFloat(sV);

                Normals = UnpackNormal(TangentZ);
                Tangents = UnpackNormal(TangentX);
            }

            public GPUSkinVertexStruct(SoftSkinVertexStruct sv)
            {
                InfluenceBones = sv.InfluenceBones;
                InfluenceWeights = sv.InfluenceWeights;
                Position = sv.Position;
                TangentX = sv.TangentX;
                TangentY = sv.TangentY;
                TangentZ = sv.TangentZ;
                U = sv.UV[0].X;
                V = sv.UV[0].Y;
                Normals = new float[3];
                Tangents = new float[3];
            }
            
            public TreeNode ToTree(int MyIndex)
            {
                string s = MyIndex + " : TanX : 0x" + TangentX.ToString("X8") + " ";
                s += "TanZ : 0x" + TangentZ.ToString("X8") + ") Position : X(";
                s += Position.X + ") Y(" + Position.Y + ") Z(" + Position.Z + ") ";
                s += "Influences  : [";
                for (int i = 0; i < 4; i++)
                    s += "(B:0x" + InfluenceBones[i].ToString("X2") + " W:" + InfluenceWeights[i].ToString("X2") + ")";
                s += "] UV : U(" + U + ") V(" + V + ") ";
                s += "Normals  : (" + Normals[0].ToString() + ", " + Normals[1].ToString() + ", " + Normals[2].ToString() + ")";
                s += "Tangents  : (" + Tangents[0].ToString() + ", " + Tangents[1].ToString() + ", " + Tangents[2].ToString() + ")";
                
                TreeNode res = new TreeNode(s);
                return res;
            }

            private float HalfToFloat(UInt16 val)
            {

                UInt16 u = val;
                int sign = (u >> 15) & 0x00000001;
                int exp = (u >> 10) & 0x0000001F;
                int mant = u & 0x000003FF;
                exp = exp + (127 - 15);
                int i = (sign << 31) | (exp << 23) | (mant << 13);
                byte[] buff = BitConverter.GetBytes(i);
                return BitConverter.ToSingle(buff, 0);
            }

            private float[] UnpackNormal(int ToDecode)
            {
                ToDecode = (int)(ToDecode ^ 0x80808080);		// offset by 128
                float[] result = new float[3];
                result[0] = (sbyte)(ToDecode & 0xFF) / 127.0f;
                result[1] = (sbyte)((ToDecode >> 8) & 0xFF) / 127.0f;
                result[2] = (sbyte)((ToDecode >> 16) & 0xFF) / 127.0f;

                return result;
            }
        }

        public struct VertexBufferGPUSkinStruct
        {
            public int NumTexCoords;
            public int UseFullPrecisionUVs;
            public int UsePackedPosition;
            public Vector3 Extension;
            public Vector3 Origin;
            public int VertexSize;
            public List<GPUSkinVertexStruct> Vertices;

            public void Serialize(SerializingContainer Container, MEVersion version)
            {
                //NumTexCoords
                NumTexCoords = 1;

                if (version == MEVersion.ME1)
                {
                    //VertexSize
                    VertexSize = Container + VertexSize;

                    Vertices = new List<GPUSkinVertexStruct>();
                    int VertsCount = 0;
                    VertsCount = Container + VertsCount;
                    for (int i = 0; i < VertsCount; i++)
                    {
                        var aSoftVert = new SoftSkinVertexStruct();
                        aSoftVert.Serialize(Container);
                        GPUSkinVertexStruct gpuv = new GPUSkinVertexStruct(aSoftVert);
                        Vertices.Add(gpuv);
                    }
                    UseFullPrecisionUVs = 1;
                    return;
                }

                ////UseFullPrecisionUVs
                UseFullPrecisionUVs = Container + UseFullPrecisionUVs;
                if (version == MEVersion.ME3)
                {                  
                    ////UsePackedPosition
                    UsePackedPosition = Container + UsePackedPosition;
                    //Extension
                    Extension.X = Container + Extension.X;
                    Extension.Y = Container + Extension.Y;
                    Extension.Z = Container + Extension.Z;
                    //origin
                    Origin.X = Container + Origin.X;
                    Origin.Y = Container + Origin.Y;
                    Origin.Z = Container + Origin.Z;
                }
                
                //vertexsize
                VertexSize = Container + VertexSize;
                int count = 0;
                if (!Container.isLoading)
                    count = Vertices.Count();
                count = Container + count;
                if (Container.isLoading)
                {
                    Vertices = new List<GPUSkinVertexStruct>();
                    for (int i = 0; i < count; i++)
                        Vertices.Add(new GPUSkinVertexStruct());
                }
                int VertexDiff = VertexSize - 32;
                for (int i = 0; i < count; i++)
                {
                    GPUSkinVertexStruct v = Vertices[i];
                    v.Serialize(Container, version);

                    if (VertexDiff > 0)
                    {
                        byte b = 0;
                        for (int j = 0; j < VertexDiff; j++)
                            b = Container + b;
                    }
                    Vertices[i] = v;
                }
            }

            public TreeNode ToTree()
            {
                TreeNode res = new TreeNode("VertexBufferGPUSkin");
                res.Nodes.Add("NumTexCoords : " + NumTexCoords);
                //res.Nodes.Add("UseFullPrecisionUVs : " + UseFullPrecisionUVs);
                //res.Nodes.Add("UsePackedPosition : " + UsePackedPosition);
                res.Nodes.Add("Extension : X(" + Extension.X + ") Y(" + Extension.Y + ") Z(" + Extension.Z + ")");
                res.Nodes.Add("Origin : X(" + Origin.X + ") Y(" + Origin.Y + ") Z(" + Origin.Z + ")");
                res.Nodes.Add("VertexSize : " + VertexSize);
                TreeNode t = new TreeNode("Vertices (" + Vertices.Count + ")");
                for (int i = 0; i < Vertices.Count; i++)
                    t.Nodes.Add(Vertices[i].ToTree(i));
                res.Nodes.Add(t);
                return res;
            }

        }

        public struct LODModelStruct
        {
            public List<SectionStruct> Sections;
            public MultiSizeIndexContainerStruct IndexBuffer;
            public int Unk1;
            public List<ushort> ActiveBones;
            public int Unk2;
            public List<SkelMeshChunkStruct> Chunks;
            public int Size;
            public int NumVertices;
            public int Unk3;
            public List<byte> RequiredBones;
            public int RawPointIndicesFlag;
            public int RawPointIndicesCount;
            public int RawPointIndicesSize;
            public int RawPointIndicesOffset;
            public List<ushort> RawPointIndices;
            public int NumTexCoords;
            public VertexBufferGPUSkinStruct VertexBufferGPUSkin;
            public int Unk4;

            public void Serialize(SerializingContainer Container, MEVersion version)
            {
                //Sections
                int count = 0;
                if (!Container.isLoading)
                    count = Sections.Count();
                count = Container + count;
                if (Container.isLoading)
                {
                    Sections = new List<SectionStruct>();
                    for (int i = 0; i < count; i++)
                        Sections.Add(new SectionStruct());
                }
                for (int i = 0; i < count; i++)
                {
                    SectionStruct sec = Sections[i];
                    sec.Serialize(Container, version);
                    Sections[i] = sec;
                }
                //IndexBuffer
                if (Container.isLoading)
                    IndexBuffer = new MultiSizeIndexContainerStruct();
                IndexBuffer.Serialize(Container);
                //unk1
                Unk1 = Container + Unk1;
                if (Unk1 > 0)
                {
                    ushort[] indices = new ushort[Unk1];
                    for (int i = 0; i < Unk1; i++)
                    {
                        indices[i] = Container + indices[i];
                    }
                }
                //Active Bones
                int activeBonesCount = 0;
                if (!Container.isLoading)
                    activeBonesCount = (short)ActiveBones.Count();
                activeBonesCount = Container + activeBonesCount;
                if (Container.isLoading)
                {
                    ActiveBones = new List<ushort>();
                    for (int i = 0; i < activeBonesCount; i++)
                        ActiveBones.Add(0);
                }
                for (int i = 0; i < activeBonesCount; i++)
                    ActiveBones[i] = Container + ActiveBones[i];
                //unk2
                Unk2 = Container + Unk2;
                if (Unk2 > 0)
                {
                    byte[] f74 = new byte[Unk2];
                    for (int i=0; i < Unk2; i++) 
                    {
                        f74[i] = Container + f74[i];
                    }
                }
                //Chunks
                if (!Container.isLoading)
                    count = Chunks.Count();
                count = Container + count;
                if (Container.isLoading)
                {
                    Chunks = new List<SkelMeshChunkStruct>();
                    for (int i = 0; i < count; i++)
                        Chunks.Add(new SkelMeshChunkStruct());
                }
                for (int i = 0; i < count; i++)
                {
                    SkelMeshChunkStruct c = Chunks[i];
                    c.Serialize(Container);
                    Chunks[i] = c;
                }
                //Size
                Size = Container + Size;
                //NumVertices
                NumVertices = Container + NumVertices;
                //unk3
                Unk3 = Container + Unk3;
                if (Unk3 > 0)
                {
                    for (int i = 0; i < Unk3; i++)
                    {
                        int[] fedge3 = new int[4];
                        fedge3[0] = Container + fedge3[0];
                        fedge3[1] = Container + fedge3[1];
                        fedge3[2] = Container + fedge3[2];
                        fedge3[3] = Container + fedge3[3];
                    }
                }
                //RequiredBones (f24)
                if (!Container.isLoading)
                    count = RequiredBones.Count();
                count = Container + count;
                if (Container.isLoading)
                {
                    RequiredBones = new List<byte>();
                    for (int i = 0; i < count; i++)
                        RequiredBones.Add(0);
                }
                for (int i = 0; i < count; i++)
                    RequiredBones[i] = Container + RequiredBones[i];
                //RawPointIndicesFlag
                RawPointIndicesFlag = Container + RawPointIndicesFlag;                               
                //RawPointIndicesCount
                RawPointIndicesCount = Container + RawPointIndicesCount;
                RawPointIndices = new List<ushort>();
                if (RawPointIndicesCount > 0)
                {
                    //RawPointIndices
                    if (Container.isLoading)
                    {
                        for (int i = 0; i < RawPointIndicesCount; i++)
                            RawPointIndices.Add(0);
                    }
                    for (int i = 0; i < RawPointIndicesCount; i++)
                        RawPointIndices[i] = Container + RawPointIndices[i];
                }
                //RawPointIndicesSize
                RawPointIndicesSize = Container + RawPointIndicesSize;                
                //RawPointIndicesOffset
                RawPointIndicesOffset = Container + RawPointIndicesOffset;
               
                //VertexBufferGPUSkin
                if (Container.isLoading)
                    VertexBufferGPUSkin = new VertexBufferGPUSkinStruct();

                VertexBufferGPUSkin.Serialize(Container, version);
               
                //unk4
                Unk4 = Container + Unk4;
            }

            public TreeNode ToTree(int MyIndex)
            {
                TreeNode res = new TreeNode("LOD " + MyIndex);
                TreeNode t = new TreeNode("Sections");
                for (int i = 0; i < Sections.Count; i++)
                    t.Nodes.Add(Sections[i].ToTree(i));
                res.Nodes.Add(t);
                res.Nodes.Add(IndexBuffer.ToTree());
                res.Nodes.Add("Unk1 : 0x" + Unk1.ToString("X8"));
                t = new TreeNode("Active Bones");
                for (int i = 0; i < ActiveBones.Count; i++)
                    t.Nodes.Add(i + " : " + ActiveBones[i]);
                res.Nodes.Add(t);
                res.Nodes.Add("Unk2 : 0x" + Unk2.ToString("X8"));
                t = new TreeNode("Chunks");
                for (int i = 0; i < Chunks.Count; i++)
                    t.Nodes.Add(Chunks[i].ToTree(i));
                res.Nodes.Add(t);
                res.Nodes.Add("Size : " + Size);
                res.Nodes.Add("NumVertices : " + NumVertices);
                res.Nodes.Add("Unk3 : 0x" + Unk3.ToString("X8"));
                t = new TreeNode("Required Bones (" + RequiredBones.Count + ")");
                for (int i = 0; i < RequiredBones.Count; i++)
                    t.Nodes.Add(i + " : " + RequiredBones[i]);
                res.Nodes.Add(t);
                res.Nodes.Add("RawPointIndicesFlag: 0x" + RawPointIndicesFlag.ToString("X8"));
                res.Nodes.Add("RawPointIndicesCount: " + RawPointIndicesCount);
                res.Nodes.Add("RawPointIndicesSize: 0x" + RawPointIndicesSize.ToString("X8"));
                res.Nodes.Add("RawPointIndicesOffset: 0x" + RawPointIndicesOffset.ToString("X8"));
                t = new TreeNode("RawPointIndices (" + RawPointIndices.Count + ")");
                for (int i = 0; i < RawPointIndices.Count; i++)
                    t.Nodes.Add(i + " : " + RawPointIndices[i]);
                res.Nodes.Add(t);
                res.Nodes.Add("NumTexCoords : " + NumTexCoords);
                res.Nodes.Add(VertexBufferGPUSkin.ToTree());
                res.Nodes.Add("Unk4 : 0x" + Unk4.ToString("X8"));
                return res;
            }
        }

        public struct TailNamesStruct
        {
            public int Name;
            public int Unk1;
            public int Unk2;

            public void Serialize(SerializingContainer Container)
            {
                Name = Container + Name;
                Unk1 = Container + Unk1;
                Unk2 = Container + Unk2;
            }
        }

        public int Flags;
        public BoundingStruct Bounding = new BoundingStruct();
        public List<int> Materials;
        public List<MaterialInstanceConstant> MatInsts;
        public Vector3 Origin;
        public Vector3 Rotation;
        public List<BoneStruct> Bones;
        public int SkeletonDepth;
        public List<LODModelStruct> LODModels;
        public List<TailNamesStruct> TailNames;
        public int Unk1;
        public int Unk2;
        public List<int> Unk3;

        public IMEPackage Owner;
        public int MyIndex;
        public bool Loaded = false;
        private int ReadEnd;

        public List<CustomVertex.PositionTextured[]> DirectXSections;

        public MESkeletalMesh()
        {
            Loaded = true;
        }

        public MESkeletalMesh(IMEPackage pcc, int Index)
        {
            Loaded = true;
            MyIndex = Index;
            Owner = pcc;
            Flags = (int)(pcc.Exports[Index].ObjectFlags >> 32);
            int start = GetPropertyEnd();
            byte[] data = pcc.Exports[Index].Data;
            byte[] buff = new byte[data.Length - start];
            for (int i = 0; i < data.Length - start; i++)
                buff[i] = data[i + start];
            MemoryStream m = new MemoryStream(buff);
            SerializingContainer Container = new SerializingContainer(m);
            Container.isLoading = true;
            Serialize(Container);
            try
            {
                for (int i = 0; i < Materials.Count; i++)
                {

                    //MatInsts.Add(new MaterialInstanceConstant(Owner, Materials[i] - 1));
                }
            }
            catch
            {
            }
            GenerateDXMeshes();
        }

        public string Name
        {
            get
            {
                return Owner.Exports[MyIndex].ObjectName;
            }
        }

        public void Serialize(SerializingContainer Container)
        {
            SerializeBoundings(Container);
            SerializeMaterials(Container);
            SerializeOrgRot(Container);
            SerializeBones(Container);
            SerializeLODs(Container);
            // since we only serialize LOD0, we don't know where Tails starts (and we don't care anyway)
            //SerializeTail(Container);
            ReadEnd = Container.GetPos();
        }

        private void SerializeBoundings(SerializingContainer Container)
        {
            Bounding.origin.X = Container + Bounding.origin.X;
            Bounding.origin.Y = Container + Bounding.origin.Y;
            Bounding.origin.Z = Container + Bounding.origin.Z;
            Bounding.size.X = Container + Bounding.size.X;
            Bounding.size.Y = Container + Bounding.size.Y;
            Bounding.size.Z = Container + Bounding.size.Z;
            Bounding.r = Container + Bounding.r;
        }

        private void SerializeMaterials(SerializingContainer Container)
        {
            int count = 0;
            if (!Container.isLoading)
                count = Materials.Count();
            count = Container + count;
            if (Container.isLoading)
            {
                Materials = new List<int>();
                MatInsts = new List<MaterialInstanceConstant>();
                for (int i = 0; i < count; i++)
                {
                    Materials.Add(0);
                }
            }
            for (int i = 0; i < count; i++)
            {
                Materials[i] = Container + Materials[i];
            }
        }

        private void SerializeOrgRot(SerializingContainer Container)
        {
            Origin.X = Container + Origin.X;
            Origin.Y = Container + Origin.Y;
            Origin.Z = Container + Origin.Z;
            Rotation.X = Container + Rotation.X;
            Rotation.Y = Container + Rotation.Y;
            Rotation.Z = Container + Rotation.Z;

        }

        private void SerializeBones(SerializingContainer Container)
        {
            int count = 0;
            if (!Container.isLoading)
                count = Bones.Count();
            count = Container + count;
            if (Container.isLoading)
            {
                Bones = new List<BoneStruct>();
                for (int i = 0; i < count; i++)
                    Bones.Add(new BoneStruct());
            }
            for (int i = 0; i < count; i++)
            {
                BoneStruct b = Bones[i];
                b.Name = Container + b.Name;
                b.Flags = Container + b.Flags;
                b.Unk1 = Container + b.Unk1;
                b.Orientation.X = Container + b.Orientation.X;
                b.Orientation.Y = Container + b.Orientation.Y;
                b.Orientation.Z = Container + b.Orientation.Z;
                b.Orientation.W = Container + b.Orientation.W;
                b.Position.X = Container + b.Position.X;
                b.Position.Y = Container + b.Position.Y;
                b.Position.Z = Container + b.Position.Z;
                b.NumChildren = Container + b.NumChildren;
                b.Parent = Container + b.Parent;
                if (Owner is ME3Package)
                    b.BoneColor = Container + b.BoneColor;

                b.BoneName = Owner.isName(b.Name) ? Owner.Names[b.Name] : "bone_" + b.Name;
                Bones[i] = b;
            }
            SkeletonDepth = Container + SkeletonDepth;
        }

        private void SerializeLODs(SerializingContainer Container)
        {
            int count = 0;
            if (!Container.isLoading)
                count = LODModels.Count();
            count = Container + count;
            // this is the actual number of lods but we only care about lod0 (and it prevents us from looking for other lods in ME1/ME2)
            // so we force count to 1.
            count = 1;
            if (Container.isLoading)
            {
                LODModels = new List<LODModelStruct>();
                for (int i = 0; i < count; i++)
                    LODModels.Add(new LODModelStruct());
            }
            for (int i = 0; i < count; i++)
            {
                LODModelStruct lod = LODModels[i];
                MEVersion version = MEVersion.ME1;
                if (Owner is ME2Package)
                {
                    version = MEVersion.ME2;
                }
                else if (Owner is ME3Package)
                {
                    version = MEVersion.ME3;
                }
                lod.Serialize(Container, version);
                LODModels[i] = lod;
            }
        }

        private void SerializeTail(SerializingContainer Container)
        {
            int count = 0;
            if (!Container.isLoading)
                count = TailNames.Count();
            count = Container + count;
            if (Container.isLoading)
            {
                TailNames = new List<TailNamesStruct>();
                for (int i = 0; i < count; i++)
                    TailNames.Add(new TailNamesStruct());
            }
            for (int i = 0; i < count; i++)
            {
                TailNamesStruct t = TailNames[i];
                t.Serialize(Container);
                TailNames[i] = t;
            }
            Unk1 = Container + Unk1;
            Unk2 = Container + Unk2;
            if (!Container.isLoading)
                count = Unk3.Count();
            count = Container + count;
            if (Container.isLoading)
            {
                Unk3 = new List<int>();
                for (int i = 0; i < count; i++)
                    Unk3.Add(0);
            }
            for (int i = 0; i < count; i++)
                Unk3[i] = Container + Unk3[i];
        }

        private void GenerateDXMeshes()
        {
            DirectXSections = new List<CustomVertex.PositionTextured[]>();
            for (int i = 0; i < LODModels.Count; i++)
            {
                LODModelStruct l = LODModels[i];
                CustomVertex.PositionTextured[] list = new CustomVertex.PositionTextured[l.IndexBuffer.Indexes.Count];
                for (int j = 0; j < l.IndexBuffer.Indexes.Count; j++)
                {
                    int idx = l.IndexBuffer.Indexes[j];
                    GPUSkinVertexStruct v = l.VertexBufferGPUSkin.Vertices[idx];
                    list[j] = new CustomVertex.PositionTextured(v.Position, v.U, v.V);
                }
                DirectXSections.Add(list);
            }
        }

        public void DrawMesh(Device device, int lod)
        {
            try
            {
                device.VertexFormat = CustomVertex.PositionTextured.Format;
                device.RenderState.Lighting = false;
                device.RenderState.FillMode = FillMode.Solid;
                device.RenderState.CullMode = Cull.None;
                if (DirectXSections[lod].Length > 2)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, DirectXSections[lod].Length / 3, DirectXSections[lod]);
                device.RenderState.FillMode = FillMode.WireFrame;
                device.RenderState.Lighting = true;
                if (DirectXSections[lod].Length > 2)
                    device.DrawUserPrimitives(PrimitiveType.TriangleList, DirectXSections[lod].Length / 3, DirectXSections[lod]);
            }
            catch (Direct3DXException)
            {
            }
        }

        public TreeNode ToTree()
        {
            TreeNode res = new TreeNode("Skeletal Mesh");
            res.Nodes.Add(GetFlags(MyIndex));
            res.Nodes.Add(GetProperties(MyIndex));
            res.Nodes.Add(BoundingsToTree());
            res.Nodes.Add(MaterialsToTree());
            res.Nodes.Add(OrgRotToTree());
            res.ExpandAll();
            res.Nodes.Add(BonesToTree());
            res.Nodes.Add(LODsToTree());
            if (TailNames != null)
                res.Nodes.Add(TailToTree());
            res.Nodes.Add("Read End @0x" + ReadEnd.ToString("X8"));
            return res;
        }

        public int GetPropertyEnd()
        {

            int pos = 0x00;
            try
            {
                byte[] data = Owner.Exports[MyIndex].Data;
                int test = BitConverter.ToInt32(data, 8);
                if (test == 0)
                    pos = 0x04;
                else
                    pos = 0x08;
                if ((Flags & 0x02000000) != 0)
                    pos = 0x1A;
                while (true)
                {
                    int idxname = BitConverter.ToInt32(data, pos);
                    if (Owner.getNameEntry(idxname) == "None" || Owner.getNameEntry(idxname) == "")
                        break;
                    int idxtype = BitConverter.ToInt32(data, pos + 8);
                    int size = BitConverter.ToInt32(data, pos + 16);
                    if (size == 0)
                        size = 1;   //boolean fix
                    if (Owner.getNameEntry(idxtype) == "StructProperty")
                        size += 8;
                    if (Owner.getNameEntry(idxtype) == "ByteProperty")
                        size += 8;
                    pos += 24 + size;
                    if (pos > data.Length)
                    {
                        pos -= 24 + size;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            return pos + 8;
        }

        private TreeNode GetFlags(int n)
        {
            TreeNode res = new TreeNode("Flags 0x" + Flags.ToString("X8"));
            foreach (string row in UnrealFlags.flagdesc)//0x02000000
            {
                string[] t = row.Split(',');
                long l = long.Parse(t[1].Trim(), System.Globalization.NumberStyles.HexNumber);
                l = l >> 32;
                if ((l & Flags) != 0)
                    res.Nodes.Add(t[0].Trim());
            }
            return res;
        }

        private TreeNode GetProperties(int n)
        {
            TreeNode res = new TreeNode("Properties");

            int pos = 0x00;
            try
            {
                byte[] data = Owner.Exports[n].Data;
                int test = BitConverter.ToInt32(data, 8);
                if (test == 0)
                    pos = 0x04;
                else
                    pos = 0x08;
                if ((Flags & 0x02000000) != 0)
                    pos = 0x1A;
                while (true)
                {
                    int idxname = BitConverter.ToInt32(data, pos);
                    if (Owner.getNameEntry(idxname) == "None" || Owner.getNameEntry(idxname) == "")
                        break;
                    int idxtype = BitConverter.ToInt32(data, pos + 8);
                    int size = BitConverter.ToInt32(data, pos + 16);
                    if (size == 0)
                        size = 1;   //boolean fix
                    if (Owner.getNameEntry(idxtype) == "StructProperty")
                        size += 8;
                    if (Owner.getNameEntry(idxtype) == "ByteProperty")
                        size += 8;
                    string s = pos.ToString("X8") + " " + Owner.getNameEntry(idxname) + " (" + Owner.getNameEntry(idxtype) + ") : ";
                    switch (Owner.getNameEntry(idxtype))
                    {
                        case "ObjectProperty":
                        case "IntProperty":
                            int val = BitConverter.ToInt32(data, pos + 24);
                            s += val.ToString();
                            break;
                        case "NameProperty":
                        case "StructProperty":
                            int name = BitConverter.ToInt32(data, pos + 24);
                            s += Owner.getNameEntry(name);
                            break;
                        case "FloatProperty":
                            float f = BitConverter.ToSingle(data, pos + 24);
                            s += f.ToString();
                            break;
                        case "BoolProperty":
                            s += (data[pos + 24] == 1).ToString();
                            break;
                        case "StrProperty":
                            int len = BitConverter.ToInt32(data, pos + 24);
                            for (int i = 0; i < len - 1; i++)
                                s += (char)data[pos + 28 + i];
                            break;
                    }
                    res.Nodes.Add(s);
                    pos += 24 + size;
                    if (pos > data.Length)
                    {
                        pos -= 24 + size;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            res.Nodes.Add(pos.ToString("X8") + " None");
            return res;
        }

        private TreeNode BoundingsToTree()
        {
            TreeNode res = new TreeNode("Boundings");
            res.Nodes.Add("Origin : X(" + Bounding.origin.X + ") Y(" + Bounding.origin.Y + ") Z(" + Bounding.origin.Z + ")");
            res.Nodes.Add("Size : X(" + Bounding.size.X + ") Y(" + Bounding.size.Y + ") Z(" + Bounding.size.Z + ")");
            res.Nodes.Add("Radius : R(" + Bounding.r + ")");
            return res;
        }

        private TreeNode MaterialsToTree()
        {
            TreeNode res = new TreeNode("Materials");
            for (int i = 0; i < Materials.Count; i++)
                res.Nodes.Add(i + " : #" + Materials[i]);
            return res;
        }

        private TreeNode OrgRotToTree()
        {
            TreeNode res = new TreeNode("Origin/Rotation");
            res.Nodes.Add("Origin : X(" + Origin.X + ") Y(" + Origin.Y + ") Z(" + Origin.Z + ")");
            res.Nodes.Add("Rotation : X(" + Rotation.X + ") Y(" + Rotation.Y + ") Z(" + Rotation.Z + ")");
            return res;
        }

        private TreeNode BonesToTree()
        {
            TreeNode res = new TreeNode("Bones (" + Bones.Count + ") Depth : " + SkeletonDepth);
            for (int i = 0; i < Bones.Count; i++)
            {
                BoneStruct b = Bones[i];
                string s = "Name : \"" + Owner.getNameEntry(b.Name) + "\" ";
                s += "Flags : 0x" + b.Flags.ToString("X8") + " ";
                s += "Unk1 : 0x" + b.Unk1.ToString("X8") + " ";
                s += "Orientation : X(" + b.Orientation.X + ") Y(" + b.Orientation.X + ") Z(" + b.Orientation.Z + ") W(" + b.Orientation.W + ")";
                s += "Position : X(" + b.Position.X + ") Y(" + b.Position.X + ") Z(" + b.Position.Z + ")";
                s += "NumChildren : " + b.NumChildren + " ";
                s += "Parent : " + b.Parent + " ";
                s += "Color : 0x" + b.BoneColor.ToString("X8");
                res.Nodes.Add(s);
            }
            return res;
        }

        private TreeNode LODsToTree()
        {
            TreeNode res = new TreeNode("LOD Models");
            for (int i = 0; i < LODModels.Count; i++)
                res.Nodes.Add(LODModels[i].ToTree(i));
            return res;
        }

        private TreeNode TailToTree()
        {
            TreeNode res = new TreeNode("Tail");
            if (TailNames != null)
            {
                TreeNode t = new TreeNode("Weird Bone List (" + TailNames.Count + ")");
                for (int i = 0; i < TailNames.Count; i++)
                    t.Nodes.Add(i + " : Name \"" + Owner.getNameEntry(TailNames[i].Name) + "\" Unk1 (" + TailNames[i].Unk1.ToString("X8") + ") Unk2(" + TailNames[i].Unk2.ToString("X8") + ")");
                res.Nodes.Add(t);
                res.Nodes.Add("Unk1 : " + Unk1.ToString("X8"));
                res.Nodes.Add("Unk2 : " + Unk2.ToString("X8"));
                t = new TreeNode("Unk3 (" + Unk3.Count + ")");
                for (int i = 0; i < Unk3.Count; i++)
                    t.Nodes.Add(i + " : " + Unk3[i]);
                res.Nodes.Add(t);
            }         
            return res;
        }

        private float HalfToFloat(UInt16 val)
        {

            UInt16 u = val;
            int sign = (u >> 15) & 0x00000001;
            int exp = (u >> 10) & 0x0000001F;
            int mant = u & 0x000003FF;
            exp = exp + (127 - 15);
            int i = (sign << 31) | (exp << 23) | (mant << 13);
            byte[] buff = BitConverter.GetBytes(i);
            return BitConverter.ToSingle(buff, 0);
        }
    }
}
