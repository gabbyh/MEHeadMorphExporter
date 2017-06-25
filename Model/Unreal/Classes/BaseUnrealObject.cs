using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ME3Explorer.Packages;
using ME3Explorer.Unreal;

namespace MEMeshMorphExporter.Unreal
{
    public class BaseUnrealObject
    {
        // index
        public int MyIndex;

        protected string objName { get; set; }

        protected List<PropertyReader.Property> Props = new List<PropertyReader.Property>();

        protected IMEPackage pcc;

        public BaseUnrealObject(IMEPackage aPcc, int index)
        {
            pcc = aPcc;
            MyIndex = index;
        }
    }
}
