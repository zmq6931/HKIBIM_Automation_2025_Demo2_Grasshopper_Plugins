using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TSM = Tekla.Structures.Model;
using TSG = Tekla.Structures.Geometry3d;

using System.Linq;
using TSMUI=Tekla.Structures.Model.UI;
//using Tekla.Structures.Model;




namespace HKIBIM_Automation_2025_Demo2
{
    public class HKIBIM_Automation_2025_DemoComponent2 : GH_Component
    {
        #region Properties
        bool isConnectToTekla=false;
        TSM.Model model = null;

        List<int> beamlist =new List<int>();

        #endregion



        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public HKIBIM_Automation_2025_DemoComponent2()
          : base("transom generation", "Andy",//HKIBIM_Automation_2025_Demo2Component
            "transom generation",
            "HKIBIM_Automation_2025_Demo2", "transom")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Profile", "P", "tekla beam Profile", GH_ParamAccess.item, "D100");
            pManager.AddLineParameter("Line", "L", "line", GH_ParamAccess.list);
            //pManager.AddBooleanParameter("Run","R","Run",GH_ParamAccess.item,false);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Use the pManager object to register your output parameters.
            // Output parameters do not have default values, but they too must have the correct access type.

            //pManager.AddCurveParameter("Spiral", "S", "Spiral curve", GH_ParamAccess.item);
            
            // Sometimes you want to hide a specific parameter from the Rhino preview.
            // You can use the HideParameter() method as a quick way:
            //pManager.HideParameter(0);
        }

        protected override void AfterSolveInstance()
        {


        }
        protected override void BeforeSolveInstance()
        {
            model = new TSM.Model();

            isConnectToTekla = model.GetConnectionStatus();


            if (isConnectToTekla == false)
            {
                MessageBox.Show("Tekla is not connected. Connection status: " + model.GetConnectionStatus().ToString());

            }

            var objs = model.GetModelObjectSelector().GetAllObjects();

            while (objs.MoveNext())
            {
                TSM.Beam b = objs.Current as TSM.Beam;
                if (b != null)
                {
                    if (beamlist.Where(x => x == b.Identifier.ID).Count() != 0)
                    {
                        beamlist.Remove(b.Identifier.ID);
                        b.Delete();

                        //MessageBox.Show("ff");
                    }
                }
            }
            model.CommitChanges();


            


        }

        

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string profile = "D100";
            bool bool_run = false;
            List<Line> lines = new List<Line>();

            //Line line=new Line();


            if (!DA.GetData(0, ref profile)) return;
            if (!DA.GetDataList(1, lines)) return;
            //if (!DA.GetData(2, ref bool_run)) return;


            try
            {

                foreach (Line l in lines)
                {
                    Point3d pt1 = l.PointAt(0);
                    Point3d pt2 = l.PointAt(1);

                    TSG.Point tk_pt1 = new TSG.Point(pt1.X, pt1.Y, pt1.Z);
                    TSG.Point tk_pt2 = new TSG.Point(pt2.X, pt2.Y, pt2.Z);

                    var beam = myfun.BeamCreatePtPt(tk_pt1, tk_pt2, profile);
                    beamlist.Add(beam.Identifier.ID);

                }
            }
            catch (Exception)
            {
            }

            model.CommitChanges();

        }

        Curve CreateSpiral(Plane plane, double r0, double r1, Int32 turns)
        {
            Line l0 = new Line(plane.Origin + r0 * plane.XAxis, plane.Origin + r1 * plane.XAxis);
            Line l1 = new Line(plane.Origin - r0 * plane.XAxis, plane.Origin - r1 * plane.XAxis);

            Point3d[] p0;
            Point3d[] p1;

            l0.ToNurbsCurve().DivideByCount(turns, true, out p0);
            l1.ToNurbsCurve().DivideByCount(turns, true, out p1);

            PolyCurve spiral = new PolyCurve();

            for (int i = 0; i < p0.Length - 1; i++)
            {
                Arc arc0 = new Arc(p0[i], plane.YAxis, p1[i + 1]);
                Arc arc1 = new Arc(p1[i + 1], -plane.YAxis, p0[i + 1]);

                spiral.Append(arc0);
                spiral.Append(arc1);
            }

            return spiral;
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5c7e40c2-cc64-4563-9cfa-254a3b59e3d6");
    }


    public static class myfun
    {
        public static TSM.Beam BeamCreatePtPt(TSG.Point pt1, TSG.Point pt2, string ProfileString, string MaterialString = "Concrete_Undefined")
        {
            TSM.Beam beam = new TSM.Beam();
            beam.Profile.ProfileString = ProfileString;
            beam.Material.MaterialString = MaterialString;
            beam.Class = "6";
            beam.Name = "Beam";

            beam.Position.Rotation = TSM.Position.RotationEnum.TOP;
            beam.Position.Plane = TSM.Position.PlaneEnum.MIDDLE;
            beam.Position.Depth = TSM.Position.DepthEnum.BEHIND;
            beam.StartPoint = pt1;
            beam.EndPoint = pt2;
            beam.AssemblyNumber.Prefix = "B";

            beam.Insert();

            return beam;


        }
    }


}