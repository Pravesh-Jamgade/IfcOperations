using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.Ifc2x3.Interfaces;
using Xbim.ModelGeometry.Scene;

namespace IfcOperations
{
    internal class Ifc2x3
    {
        internal static List<Data> Run(Xbim.Ifc.IfcStore model)
        {
           
            // Specify elements
            List<Xbim.Ifc2x3.Interfaces.IIfcWall> allWalls =
                model.Instances.OfType<Xbim.Ifc2x3.Interfaces.IIfcWall>().ToList(); int a = allWalls.Count();

            List<Xbim.Ifc2x3.Interfaces.IIfcOpeningElement> allOpenigs =
                model.Instances.OfType<Xbim.Ifc2x3.Interfaces.IIfcOpeningElement>().ToList(); int b = allOpenigs.Count();

            List<Xbim.Ifc2x3.Interfaces.IIfcBeamType> allBeams =
                model.Instances.OfType<Xbim.Ifc2x3.Interfaces.IIfcBeamType>().ToList(); int c = allBeams.Count();

            List<Xbim.Ifc2x3.Interfaces.IIfcColumnType> allColumns =
                model.Instances.OfType<Xbim.Ifc2x3.Interfaces.IIfcColumnType>().ToList(); int d = allColumns.Count();

            List<Xbim.Ifc2x3.Interfaces.IIfcElement> ifcElements =
                model.Instances.OfType<Xbim.Ifc2x3.Interfaces.IIfcElement>().ToList(); int e = ifcElements.Count();


            // Extract geometry
            var context = new Xbim3DModelContext(model);
            context.CreateContext();
            var instances = context.ShapeInstances().ToList();
            IEnumerator<XbimShapeInstance> allShapeInstances = instances.GetEnumerator();

            List<Data> allData = new List<Data>();
            while (allShapeInstances.MoveNext())
            {
               
                // get data from shape instance
                XbimShapeInstance xbimShapeInstance = allShapeInstances.Current;
                Xbim.Ifc2x3.Interfaces.IIfcElement element = ifcElements.FirstOrDefault(x => x.EntityLabel == xbimShapeInstance.IfcProductLabel);
                string name = element.Name;
                string id = element.GlobalId;

                var ro = xbimShapeInstance.Transformation;
                var translation = ro.Translation;
                var decompose = ro.Decompose(out XbimVector3D scale, out XbimQuaternion rot, out XbimVector3D trans);

                Matrix<double> matrixData = DenseMatrix.OfArray(new double[,] {
                            { ro.M11, ro.M12, ro.M13, ro.M14 },
                            { ro.M21, ro.M22, ro.M23, ro.M24},
                            { ro.M31, ro.M32, ro.M33, ro.M34},
                            { ro.OffsetX, ro.OffsetY, ro.OffsetZ, ro.M44} });
                matrixData = matrixData.Transpose();
                Console.WriteLine(matrixData.ToString());
                XbimRect3D rect = xbimShapeInstance.BoundingBox;
                XbimPoint3D Min = rect.Min;
                XbimPoint3D Max = rect.Max;
                Data data = new Data
                    (
                        name,
                        id,
                        new BoundingBox(new XYZ(Min.X, Min.Y, Min.Z), new XYZ(Max.X, Max.Y, Max.Z)),
                        new XYZ(translation.X, translation.Y, translation.Z),
                        matrixData
                    );
                allData.Add(data);
            }

            return allData;
        }

        internal static List<Data> HierarchyRun(IfcStore model)
        {
            //List<Data> allData = new List<Data>();
            var project = model.Instances.FirstOrDefault<IIfcProject>();

            var context = new Xbim3DModelContext(model);
            context.CreateContext();
            List<XbimShapeInstance> allShapeInstances = context.ShapeInstances().ToList();
            //IEnumerator<XbimShapeInstance> allShapeInstances = instances.GetEnumerator();

            List<Data> data = GetIfc2X3.GetHierarchialData(project , 0, allShapeInstances);
            return data;
        }
    }
}