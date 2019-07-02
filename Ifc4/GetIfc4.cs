using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Collections.Generic;
using System.Linq;
using Xbim.Common.Geometry;
using Xbim.Ifc4.Interfaces;

namespace IfcOperations
{
    internal class GetIfc4
    {
        internal static List<Data> GetHierarchialData
            (IIfcObjectDefinition project, int level, List<XbimShapeInstance> allShapeInstances)
        {

            List<Data> collectedData = new List<Data>();
            
            var spatialElement = project as IIfcSpatialStructureElement;
            if (spatialElement != null)
            {
                //using IfcRelContainedInSpatialElement to get contained elements
                var containedElements = spatialElement.ContainsElements.SelectMany(rel => rel.RelatedElements);
                IEnumerable<XbimShapeInstance> allShapeEnumer = allShapeInstances.AsEnumerable<XbimShapeInstance>();

                foreach (var instance in allShapeInstances)
                {
                    var current = instance;
                    IIfcProduct element = containedElements.FirstOrDefault(x => x.EntityLabel == current.IfcProductLabel);

                    if (element == null) continue;
                    IIfcElement ifcelement = element as IIfcElement;
                    string name = ifcelement.Name;
                    string id = ifcelement.GlobalId;
                    BoundingBox bbox = new BoundingBox(
                            new XYZ(current.BoundingBox.Min.X, current.BoundingBox.Min.Y, current.BoundingBox.Min.Z),
                            new XYZ(current.BoundingBox.Max.X, current.BoundingBox.Max.Y, current.BoundingBox.Max.Z)
                        );
                    var transformation = current.Transformation;
                    var translation = transformation.Translation;
                    var ro = transformation;
                    Matrix<double> matrixData = DenseMatrix.OfArray(new double[,] {
                            { ro.M11, ro.M12, ro.M13, ro.M14 },
                            { ro.M21, ro.M22, ro.M23, ro.M24},
                            { ro.M31, ro.M32, ro.M33, ro.M34},
                            { ro.OffsetX, ro.OffsetY, ro.OffsetZ, ro.M44} });
                    matrixData = matrixData.Transpose();

                    Data data = new Data
                        (
                           name,
                           id,
                           bbox,
                           new XYZ(translation.X, translation.Y, translation.Z),
                           matrixData,
                           element.GetType().Name
                        );
                    collectedData.Add(data);
                }

            }

            foreach
                (
                    var item in
                    project.IsDecomposedBy
                    .SelectMany(r => r.RelatedObjects)
                )
            {
                Data itemData = new Data(item.Name, item.GlobalId);
                var dataChild = GetHierarchialData(item, level + 1, allShapeInstances);
                if (dataChild == null) continue;
                itemData.AddChild(dataChild);
                collectedData.Add(itemData);
            }
           
            return collectedData;
        }
    }
}