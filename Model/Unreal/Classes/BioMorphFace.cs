using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ME3Explorer.Packages;
using ME3Explorer.Unreal;
using ME3Explorer.Unreal.Classes;

namespace MEMeshMorphExporter.Unreal
{
    public class BioMorphFace : BaseUnrealObject
    {
        private byte[] data;

        #region properties
        public string Name
        {
            get
            {
                return pcc.Exports[MyIndex].ObjectName;
            }
        }
        public string BaseMeshName
        {
            get
            {
                return m_oBaseHead != null ? m_oBaseHead.Name : "";
            }
        }
        public string HairMeshName
        {
            get
            {
                return m_oHairMesh != null ? m_oHairMesh.Name : "";
            }
        }
        public MaterialOverrides MaterialsOverrides
        {
            get
            {
                return m_oMaterialOverrides;
            }
        }
        public List<BoneOffset> BonesOffset
        {
            get
            {
                return m_aFinalSkeleton;
            }
        }
        #endregion

        MaterialOverrides m_oMaterialOverrides;
        MESkeletalMesh m_oBaseHead;
        MESkeletalMesh m_oHairMesh;
        List<BioFeature> m_aMorphFeatures = new List<BioFeature>();
        List<BoneOffset> m_aFinalSkeleton = new List<BoneOffset>();

        List<List<Vector>> Vertices = new List<List<Vector>>();
        List<Vector> Lod0Vertices = new List<Vector>();
        List<Vector> Lod1Vertices = new List<Vector>();
        List<Vector> Lod2Vertices = new List<Vector>();


        public BioMorphFace(IMEPackage Pcc, int Index) : base (Pcc, Index)
        {
            if (pcc.isExport(Index))
            {
                data = pcc.Exports[Index].Data;
                IExportEntry export = pcc.Exports[MyIndex];
                Props = PropertyReader.getPropList(export);

                int startVerticesIndex = -1;
                foreach (PropertyReader.Property prop in Props)
                {
                    string propName = pcc.getNameEntry(prop.Name);
                    switch (propName)
                    {
                        case "m_aMorphFeatures":
                            ReadMorphFeatures(prop);
                            break;
                        case "m_aFinalSkeleton":
                            ReadFinalSkeleton(prop);
                            break;
                        case "m_oBaseHead":
                            int objIndex = prop.Value.IntValue;
                            if (pcc.isExport(objIndex - 1))
                            {
                                m_oBaseHead = new MESkeletalMesh(pcc, objIndex - 1);
                            }
                            else if (pcc.isImport(-objIndex - 1))
                            {
                                // look for a package actually containing the object
                                IMEPackage nPcc = null;
                                int expIndex = -1;
                                LookupObjectInImports(-objIndex - 1, out nPcc, out expIndex);
                                if (nPcc != null && expIndex > -1)
                                {
                                    m_oBaseHead = new MESkeletalMesh(nPcc, expIndex);
                                }
                            }
                            break;
                        case "m_oHairMesh":
                            int objHairIndex = prop.Value.IntValue;
                            if (pcc.isExport(objHairIndex - 1))
                            {
                                m_oHairMesh = new MESkeletalMesh(pcc, objHairIndex - 1);
                            }
                            else if (pcc.isImport(-objHairIndex - 1))
                            {
                                IMEPackage nPcc = null;
                                int expIndex = -1;
                                LookupObjectInImports(-objHairIndex - 1, out nPcc, out expIndex);
                                if (nPcc != null && expIndex > -1)
                                {
                                    m_oHairMesh = new MESkeletalMesh(nPcc, expIndex);
                                }
                            }
                            break;
                        case "m_nInternalMorphFaceContentVersion":
                            break;
                        case "m_oMaterialOverrides":
                            int objMatOIndex = prop.Value.IntValue;
                            if (pcc.isExport(objMatOIndex - 1))
                            {
                                m_oMaterialOverrides = new MaterialOverrides(pcc, objMatOIndex - 1);
                            }
                            else if (pcc.isImport(-objMatOIndex - 1))
                            {
                                IMEPackage nPcc = null;
                                int expIndex = -1;
                                LookupObjectInImports(-objMatOIndex - 1, out nPcc, out expIndex);
                                if (nPcc != null && expIndex > -1)
                                {
                                    m_oMaterialOverrides = new MaterialOverrides(nPcc, expIndex);
                                }
                            }
                            break;
                        case "CurrentMorphFaceContentVersion":
                        case "bRequiresDynamicUpdates":
                            break;
                        case "None":
                            startVerticesIndex = prop.offsetval + prop.raw.Length;
                            break;
                        default:
                            Console.WriteLine("Unknow property for BioMorphFace: " + pcc.getNameEntry(prop.Name));
                            break;
                    }
                }
                ReadVertices(startVerticesIndex);
            }
        }

        public bool IsExportable
        {
            get
            {
                return m_oBaseHead != null;
            }
        }

        public MESkeletalMesh Apply()
        {
            // apply vertices morph first
            // in skeletalMesh, we load only LOD0, so we only apply for lod0
            for (int lod = 0; lod < 1; lod++)
            {
                for (int v=0; v < m_oBaseHead.LODModels[lod].VertexBufferGPUSkin.Vertices.Count; v++) 
                {
                    var vertex = m_oBaseHead.LODModels[lod].VertexBufferGPUSkin.Vertices[v];
                    vertex.Position.X = Vertices[lod][v].X;
                    vertex.Position.Y = Vertices[lod][v].Y;
                    vertex.Position.Z = Vertices[lod][v].Z;
                    m_oBaseHead.LODModels[lod].VertexBufferGPUSkin.Vertices[v] = vertex;
                }
            }

            // return mesh
            return m_oBaseHead;
        }

        private MESkeletalMesh.BoneStruct SearchBoneByName(string name)
        {
            foreach (var b in m_oBaseHead.Bones)
            {
                string boneName = pcc.Names[b.Name];
                if (boneName == name)
                {
                    return b;
                }
            }
            MESkeletalMesh.BoneStruct empty = new MESkeletalMesh.BoneStruct();
            empty.Name = -1;
            return empty;
        }

        private void ReadFinalSkeleton(PropertyReader.Property p)
        {
            int count = BitConverter.ToInt32(p.raw, 24);
            int propStart = 28;

            for (int i = 0; i < count; i++)
            {
                List<PropertyReader.Property> BoneOffsetProps = PropertyReader.ReadProp(pcc, p.raw, propStart);
                propStart = BoneOffsetProps[BoneOffsetProps.Count - 1].offend;
                BoneOffset bone = new BoneOffset(BoneOffsetProps, pcc);
                m_aFinalSkeleton.Add(bone);
            }
        }

        private void ReadMorphFeatures(PropertyReader.Property p)
        {
            int count = BitConverter.ToInt32(p.raw, 24);

            int propStart = 28;

            for (int i = 0; i < count; i++)
            {
                List<PropertyReader.Property> FeatureProps = PropertyReader.ReadProp(pcc, p.raw, propStart);
                propStart = FeatureProps[FeatureProps.Count - 1].offend;
                BioFeature Feature = new BioFeature(FeatureProps, pcc);
                m_aMorphFeatures.Add(Feature);
            }
        }

        private void ReadVertices(int start)
        {
            if (pcc.Exports[MyIndex].Data.Length > start + 8)
            {
                // lod0
                int count = BitConverter.ToInt32(pcc.Exports[MyIndex].Data, start + 8);

                int dataIndex = start + 12;
                for (int i = 0; i < count; i++)
                {
                    // read Vector
                    Vector vert = ReadVert(pcc.Exports[MyIndex].Data, dataIndex);
                    Lod0Vertices.Add(vert);
                    dataIndex = dataIndex + 12;
                }
                Vertices.Add(Lod0Vertices);

                if (pcc.Exports[MyIndex].Data.Length > dataIndex + 4)
                {
                    // lod1
                    count = BitConverter.ToInt32(pcc.Exports[MyIndex].Data, dataIndex + 4);
                    dataIndex = dataIndex + 8;
                    for (int i = 0; i < count; i++)
                    {
                        // read Vector
                        Vector vert = ReadVert(pcc.Exports[MyIndex].Data, dataIndex);
                        Lod1Vertices.Add(vert);
                        dataIndex = dataIndex + 12;
                    }
                    Vertices.Add(Lod1Vertices);

                    if (pcc.Exports[MyIndex].Data.Length > dataIndex + 4)
                    {
                        // lod2
                        count = BitConverter.ToInt32(pcc.Exports[MyIndex].Data, dataIndex + 4);
                        dataIndex = dataIndex + 8;
                        for (int i = 0; i < count; i++)
                        {
                            // read Vector
                            Vector vert = ReadVert(pcc.Exports[MyIndex].Data, dataIndex);
                            Lod2Vertices.Add(vert);
                            dataIndex = dataIndex + 12;
                        }
                        Vertices.Add(Lod2Vertices);
                    }
                }
            }           
        }

        private Vector ReadVert(byte[] bytes, int start)
        {
            Vector vert = new Vector();
            vert.X = BitConverter.ToSingle(pcc.Exports[MyIndex].Data, start);
            vert.Y = BitConverter.ToSingle(pcc.Exports[MyIndex].Data, start + 4);
            vert.Z = BitConverter.ToSingle(pcc.Exports[MyIndex].Data, start + 8);

            return vert;
        }

        public void ExportToFile(string baseDir, bool overrideIfExists)
        {
            string path = Path.Combine(baseDir, pcc.Exports[MyIndex].ObjectName + ".morph");
            if (File.Exists(path) && overrideIfExists)
                File.Delete(path);
            else if (File.Exists(path) && !overrideIfExists)
                return;

            List<string> lines = new List<string>();

            lines.Add("[MorphName=" + pcc.Exports[MyIndex].ObjectName + "]");
            if (m_oBaseHead != null)
            {
                lines.Add("[HeadMesh=" + pcc.Exports[m_oBaseHead.MyIndex].ObjectName + "]");
            }
            if (m_oHairMesh != null)
            {
                lines.Add("[HairMesh=" + pcc.Exports[m_oHairMesh.MyIndex].ObjectName + "]");
            }
            if (m_oMaterialOverrides != null)
            {
                lines.Add("[MaterialOverrides]");
                if (m_oMaterialOverrides.TextureOverrides.Count > 0)
                {
                    lines.Add("[TextureOverrides]");
                    foreach (TextureOverride to in m_oMaterialOverrides.TextureOverrides)
                    {
                        lines.Add(to.ParamName + "=" + to.TextureName);
                    }
                }
                if (m_oMaterialOverrides.ColorOverrides.Count > 0)
                {
                    lines.Add("[ColorOverrides]");
                    foreach (ColorOverride co in m_oMaterialOverrides.ColorOverrides)
                    {
                        lines.Add(co.ParamName + "=" + co.Value.ToString());
                    }
                }
                if (m_oMaterialOverrides.ScalarOverrides.Count > 0)
                {
                    lines.Add("[ScalarOverrides]");
                    foreach (ScalarOverride so in m_oMaterialOverrides.ScalarOverrides)
                    {
                        lines.Add(so.ParamName + "=" + so.Value.ToString());
                    }
                }
            }
            lines.Add("[Lod0Verts]");
            foreach (Vector lod0Vert in Lod0Vertices)
            {
                lines.Add(lod0Vert.X + ";" + lod0Vert.Y + ";" + lod0Vert.Z);
            }
            File.AppendAllLines(path, lines);

            lines.Clear();
            lines.Add("[OffsetBones]");
            foreach (BoneOffset bo in m_aFinalSkeleton)
            {
                string skel = bo.BoneName + ";" + bo.Offset.X + ";" + bo.Offset.Y + ";" + bo.Offset.Z;
                lines.Add(skel);
            }
            File.AppendAllLines(path, lines);
        }

        public void ExportToJson(string targetFile)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(targetFile, json);
        }

        private void LookupObjectInImports(int impIndex, out IMEPackage newPcc, out int expIndex) 
        {
            newPcc = null;
            expIndex = -1;

            if (pcc.isImport(impIndex))
            {
                var import = pcc.Imports[impIndex];
                string objectName = import.ObjectName;

                // this is a very ugly fix: there is probably a way to find out which pcc contains the object
                // but this is not the package referenced in import.PackageFile
                // therefore, we will try the following mapping, 'deducted' by experience only.

                string newPackageName = null;
                // in ME1 case, we have a BIOG_...Morph package for every species, so we will work with that
                if (pcc is ME1Package)
                {
                    if (objectName.Contains("ASA"))
                    {
                        newPackageName = "BIOG_ASA_HED_PROMorph_R.upk";
                    }
                    else if (objectName.Contains("HMM"))
                    {
                        newPackageName = "BIOG_HMM_HED_PROMorph.upk";
                    }
                    else if (objectName.Contains("HMF"))
                    {
                        newPackageName = "BIOG_HMF_HED_PROMorph_R.upk";
                    }
                    else if (objectName.Contains("TUR"))
                    {
                        newPackageName = "BIOG_TUR_HED_PROMorph_R.upk";
                    }
                    else if (objectName.Contains("SAL"))
                    {
                        newPackageName = "BIOG_SAL_HED_PROMorph_R.upk";
                    }
                    else if (objectName.Contains("KRO"))
                    {
                        newPackageName = "BIOG_KRO_HED_PROMorph.upk";
                    }
                }
                else if (pcc is ME2Package)
                {
                    // in ME2 case, we take a package containing the mesh for each species        
                    if ((objectName.StartsWith("HMM") || objectName.StartsWith("HMF")))
                    {
                        // if human, we can try BIOG_MORPH_FACE.pcc
                        newPackageName = "BIOG_MORPH_FACE.pcc";
                    }
                    else if (objectName.StartsWith("TUR") || objectName.StartsWith("ASA"))
                    {
                        newPackageName = "BioP_CitHub.pcc";
                    }
                    else if (objectName.StartsWith("KRO"))
                    {
                        newPackageName = "BioP_KroHub.pcc";
                    }
                    else if (objectName.StartsWith("SAL"))
                    {
                        newPackageName = "BioP_TwrHub.pcc";
                    }
                }
                // no case reported for ME3 so we do nothing at this time.

                if (newPackageName != null)
                {
                    Utils.GetObjectFromPcc(newPackageName, pcc, objectName, import.ClassName, out newPcc, out expIndex);
                }
            }
        }
    }

    class BioFeature
    {
        public string Name = "";
        public float Value = 0.0f;

        public BioFeature(List<PropertyReader.Property> props,IMEPackage pcc)
        {
            foreach(PropertyReader.Property p in props)
            {
                string propName = pcc.getNameEntry(p.Name);
                switch (propName) 
                {
                    case "sFeatureName":
                        int nameI = p.Value.IntValue;
                        if (pcc.isName(nameI - 1))
                        {
                            Name = pcc.Names[nameI - 1];
                        }
                        break;
                    case "Offset":
                        Value = BitConverter.ToSingle(p.raw, p.raw.Length - 4);
                        break;
                    case "None":
                        //
                        break;
                    default:
                        Console.WriteLine("Prop name for Bioture is " + propName);
                        break;
                }
            }
        }
    }

    public class BoneOffset
    {
        public string BoneName = "";
        public Vector Offset;

        public BoneOffset(List<PropertyReader.Property> props, IMEPackage pcc)
        {
            foreach (PropertyReader.Property p in props)
            {
                string propName = pcc.getNameEntry(p.Name);
                switch (propName)
                {
                    case "nName":
                        int nameI = p.Value.IntValue;
                        if (pcc.isName(nameI))
                        {
                            BoneName = pcc.Names[nameI];
                        }
                        break;
                    case "vPos":                 
                        Offset = new Vector(BitConverter.ToSingle(p.raw, p.raw.Length - 12),
                                              BitConverter.ToSingle(p.raw, p.raw.Length - 8),
                                              BitConverter.ToSingle(p.raw, p.raw.Length - 4));
                        break;
                    case "None":
                        //
                        break;
                    default:
                        Console.WriteLine("Prop name for BoneOffset is " + propName);
                        break;
                }
            }
        }
    }

    public class MaterialOverrides
    {
        public List<TextureOverride> TextureOverrides = new List<TextureOverride>();
        public List<ColorOverride> ColorOverrides = new List<ColorOverride>();
        public List<ScalarOverride> ScalarOverrides = new List<ScalarOverride>();

        public MaterialOverrides(IMEPackage pcc, int index)
        {
            
            byte[] data = pcc.Exports[index].Data;
            List<PropertyReader.Property> props = PropertyReader.getPropList(pcc.Exports[index]);
            foreach (PropertyReader.Property p in props)
            {
                string propName = pcc.getNameEntry(p.Name);
                switch (propName)
                {
                    case "m_aTextureOverrides":
                        ReadTextures(p, pcc);
                        break;
                    case "m_aColorOverrides":
                        ReadColors(p, pcc);
                        break;
                    case "m_aScalarOverrides":
                        ReadScalars(p, pcc);
                        break;
                    case "None":
                        break;
                }
            }
        }

        private void ReadTextures(PropertyReader.Property p, IMEPackage pcc)
        {
            int count = BitConverter.ToInt32(p.raw, 24);
            int start = 28;

            for (int i=0; i < count; i++)
            {
                // read next 68 bytes
                List<PropertyReader.Property> props = PropertyReader.ReadProp(pcc, p.raw, start);
                TextureOverride to = new TextureOverride();

                foreach (PropertyReader.Property sp in props)
                {
                    string propName = pcc.getNameEntry(sp.Name);
                    switch (propName)
                    {
                        case "nName":
                            int nameI = sp.Value.IntValue;
                            if (pcc.isName(nameI))
                            {
                                to.ParamName = pcc.Names[nameI];
                            }
                            break;
                        case "m_pTexture":
                            int objTextIndex = sp.Value.IntValue;
                            if (pcc.isExport(objTextIndex - 1))
                            {
                                to.TextureName = pcc.Exports[objTextIndex - 1].ObjectName;
                            }
                            break;
                        case "None":
                            break;
                    }
                }
                TextureOverrides.Add(to);
                start = props[props.Count - 1].offend;
            }
        }

        private void ReadColors(PropertyReader.Property p, IMEPackage pcc)
        {
            int count = BitConverter.ToInt32(p.raw, 24);
            int start = 28;

            for (int i = 0; i < count; i++)
            {
                List<PropertyReader.Property> props = PropertyReader.ReadProp(pcc, p.raw, start);
                ColorOverride co = new ColorOverride();

                foreach (PropertyReader.Property sp in props)
                {
                    string propName = pcc.getNameEntry(sp.Name);
                    switch (propName)
                    {
                        case "nName":
                            int nameI = sp.Value.IntValue;
                            if (pcc.isName(nameI))
                            {
                                co.ParamName = pcc.Names[nameI];
                            }
                            break;
                        case "cValue":
                            co.Value = new LinearColor(sp);
                            break;
                        case "None":
                            break;
                    }
                }
                ColorOverrides.Add(co);
                start = props[props.Count - 1].offend;
            }
        }

        private void ReadScalars(PropertyReader.Property p, IMEPackage pcc)
        {
            int count = BitConverter.ToInt32(p.raw, 24);
            int start = 28;

            for (int i = 0; i < count; i++)
            {
                // read next 68 bytes
                List<PropertyReader.Property> props = PropertyReader.ReadProp(pcc, p.raw, start);
                ScalarOverride so = new ScalarOverride();

                foreach (PropertyReader.Property sp in props)
                {
                    string propName = pcc.getNameEntry(sp.Name);
                    switch (propName)
                    {
                        case "nName":
                            int nameI = sp.Value.IntValue;
                            if (pcc.isName(nameI))
                            {
                                so.ParamName = pcc.Names[nameI];
                            }
                            break;
                        case "sValue":
                            so.Value = BitConverter.ToSingle(sp.raw, sp.raw.Length - 4);
                            break;
                        case "None":
                            break;
                    }
                }
                ScalarOverrides.Add(so);
                start = props[props.Count - 1].offend;
            }
        }
    }

    public struct TextureOverride
    {
        public string TextureName { get; set; }
        public string ParamName { get; set; }
    }

    public struct ScalarOverride
    {
        public float Value { get; set; }
        public string ParamName { get; set; }
    }

    public struct ColorOverride
    {
        public LinearColor Value { get; set; }
        public string ParamName { get; set; }
    }

    public class LinearColor
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }


        LinearColor White { get { return new LinearColor(1, 1, 1, 1); } }

        public LinearColor(PropertyReader.Property p)
        {
           R = BitConverter.ToSingle(p.raw, 32);
           G = BitConverter.ToSingle(p.raw, 36);
           B = BitConverter.ToSingle(p.raw, 40);
           A = BitConverter.ToSingle(p.raw, 44);
        }

        public LinearColor (float red, float green, float blue, float alpha)
        {
            R = red;
            G = green;
            B = blue;
            A = alpha;
        }

        public override string ToString()
        {
            return "(R=" + R + ", G=" + G + ", B=" + B + ", A=" + A + ")";
        }
    }

    public struct Vector
    {
        public float X;
        public float Y;
        public float Z;

        public Vector(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
