using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace HKIBIM_Automation_2025_Demo2
{
    public class HKIBIM_Automation_2025_Demo2Info : GH_AssemblyInfo
    {
        public override string Name => "HKIBIM_Automation_2025_Demo2";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("c743b114-38a7-4edf-bb64-bfb6e98ed1c7");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";

        //Return a string representing the version.  This returns the same version as the assembly.
        public override string AssemblyVersion => GetType().Assembly.GetName().Version.ToString();
    }
}