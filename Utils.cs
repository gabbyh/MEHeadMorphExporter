using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using ME3Explorer.Packages;

namespace MEMeshMorphExporter
{
    static class Utils
    {
        public static float HalfToFloat(UInt16 val)
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

        public static void GetObjectFromPcc(string newPackageName, IMEPackage pcc, string objectName, string objectType, out IMEPackage NewPcc, out int ExpIndex)
        {
            NewPcc = null;
            ExpIndex = -1;

            IMEPackage nPcc = null;
            var OkPackage = MEPackageHandler.packagesInTools.Where(p => p.FileName == newPackageName);
            if (OkPackage.Count() > 0)
            {
                nPcc = OkPackage.ElementAt(0);
            }
            else
            {
                string nPccPath = LocatePackage(pcc, newPackageName);   
                if (nPccPath != null)
                    nPcc = MEPackageHandler.OpenMEPackage(nPccPath);
            }

            if (nPcc != null)
            {
                var objIndex = nPcc.Exports.Select((value, index) => new { value, index })
                                            .Where(z => z.value.ObjectName == objectName && z.value.ClassName == objectType)
                                            .Select(z => z.index).DefaultIfEmpty(-1).FirstOrDefault();

                NewPcc = nPcc;
                ExpIndex = objIndex;
            }
        }

        private static string LocatePackage(IMEPackage origPackage, string newPackageName)
        {
            // we try first in the same folder : should be Ok for ME2 and ME3 and some ME1 cases.
            string packageDir = Path.GetDirectoryName(origPackage.FileName);
            string nPccPath = Path.Combine(packageDir, newPackageName);
            if (File.Exists(nPccPath)) return nPccPath;
            
            if (origPackage is ME1Package)
            {
                // find cookedPC dir
                int cookedPCIndex = origPackage.FileName.IndexOf("CookedPC");
                string CookedPcDir = origPackage.FileName.Substring(0, cookedPCIndex + 8);
                string HumanoidsDir = Path.Combine(CookedPcDir, "Packages", "GameObjects", "Characters", "Humanoids");

                string finalDir = null;
                switch (newPackageName)
                {
                    case "BIOG_ASA_HED_PROMorph_R.upk":
                        finalDir = Path.Combine(HumanoidsDir, "Asari");
                        break;
                    case "BIOG_HMM_HED_PROMorph.upk":
                        finalDir = Path.Combine(HumanoidsDir, "HumanMale");
                        break;
                    case "BIOG_HMF_HED_PROMorph_R.upk":
                        finalDir = Path.Combine(HumanoidsDir, "HumanFemale");
                        break;
                    case "BIOG_SAL_HED_PROMorph_R.upk":
                        finalDir = Path.Combine(HumanoidsDir, "Salarian");
                        break;
                    case "BIOG_KRO_HED_PROMorph.upk":
                        finalDir = Path.Combine(HumanoidsDir, "Krogan");
                        break;
                    case "BIOG_TUR_HED_PROMorph_R.upk":
                        finalDir = Path.Combine(HumanoidsDir, "Turian");
                        break;
                }
                nPccPath = Path.Combine(finalDir, newPackageName);
                if (File.Exists(nPccPath)) return nPccPath;
            }
            return null;
        }
    }
}
