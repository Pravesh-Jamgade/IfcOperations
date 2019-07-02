using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common.Geometry;
using Xbim.Ifc;
using Xbim.Ifc4.Interfaces;
using Xbim.ModelGeometry.Scene;

namespace IfcOperations
{
    internal class Ifc4
    {
        internal static List<Data> Run(Xbim.Ifc.IfcStore model)
        {
            // Specify elements
            List<IIfcWall> allWalls =
                model.Instances.OfType<IIfcWall>().ToList(); int a = allWalls.Count();

            List<IIfcOpeningElement> allOpenigs =
                model.Instances.OfType<IIfcOpeningElement>().ToList(); int b = allOpenigs.Count();

            List<IIfcBeamType> allBeams =
                model.Instances.OfType<IIfcBeamType>().ToList(); int c = allBeams.Count();

            List<IIfcColumnType> allColumns =
                model.Instances.OfType<IIfcColumnType>().ToList(); int d = allColumns.Count();

            List<IIfcElement> ifcElements =
                model.Instances.OfType<IIfcElement>().ToList(); int e = ifcElements.Count();


            // Extract geometry
            var context = new Xbim3DModelContext(model);
            context.CreateContext();
            var instances = context.ShapeInstances().ToList();
            IEnumerator<XbimShapeInstance> allShapeInstances = instances.GetEnumerator();

            List<Data> allData = new List<Data>();
            while (allShapeInstances.MoveNext())
            {
                XbimShapeInstance xbimShapeInstance = allShapeInstances.Current;
                IIfcElement element = ifcElements.FirstOrDefault(x => x.EntityLabel == xbimShapeInstance.IfcProductLabel);
                if (element == null) continue;
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
                //Console.WriteLine(matrixData.ToString()); 
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
            var project = model.Instances.FirstOrDefault<IIfcProject>();

            var context = new Xbim3DModelContext(model);
            context.CreateContext();
            List<XbimShapeInstance> allShapeInstances = context.ShapeInstances().ToList();

            List<Data> data = GetIfc4.GetHierarchialData(project, 0, allShapeInstances);
            return data;
        }
    }
}