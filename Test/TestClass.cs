using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xbim.Common.Geometry;
using Xbim.Common.XbimExtensions;
using Xbim.Ifc;
using Xbim.Ifc2x3.Interfaces;
using Xbim.ModelGeometry.Scene;

namespace IfcOperations.Test
{
    class TestClass
    {
        internal static void Run(IfcStore model)
        {

            var ctx = new Xbim3DModelContext(model);
            ctx.CreateContext();

            using (var geomRead = model.GeometryStore.BeginRead())
            {

                var scale = model.ModelFactors.OneMetre;

                var prodIds = new HashSet<int>();
                var products = model.Instances.OfType<IIfcProduct>();
                foreach (var product in products)
                {
                    if (product is IIfcFeatureElement) continue;
                    prodIds.Add(product.EntityLabel);
                }

                var toIgnore = new short[4];
                toIgnore[0] = model.Metadata.ExpressTypeId("IFCOPENINGELEMENT");
                toIgnore[1] = model.Metadata.ExpressTypeId("IFCPROJECTIONELEMENT");
                if (model.SchemaVersion.ToString() == "Ifc4")
                {
                    toIgnore[2] = model.Metadata.ExpressTypeId("IFCVOIDINGFEATURE");
                    toIgnore[3] = model.Metadata.ExpressTypeId("IFCSURFACEFEATURE");
                }

                List<Vector3> vertices = new List<Vector3>();

                foreach (var geometry in geomRead.ShapeGeometries)
                {

                    if (geometry.ShapeData.Length <= 0) //no geometry to display so don't write out any products for it
                        continue;
                    var instances = geomRead.ShapeInstancesOfGeometry(geometry.ShapeLabel);

                    var xbimShapeInstances = instances.Where
                        (
                            si => !toIgnore.Contains(si.IfcTypeId)
                            &&
                            si.RepresentationType == XbimGeometryRepresentationType.OpeningsAndAdditionsIncluded
                            //&&
                            //prodIds.Contains(si.IfcProductLabel)
                        ).ToList();

                    if (!xbimShapeInstances.Any()) continue;

                    XbimShapeTriangulation tr;
                    using (var ms = new MemoryStream(((IXbimShapeGeometryData)geometry).ShapeData))
                    using (var br = new BinaryReader(ms))
                        tr = br.ReadShapeTriangulation();

                    foreach (IXbimShapeInstanceData xbimShapeInstance in xbimShapeInstances)
                    {

                        var styleId = xbimShapeInstance.StyleLabel > 0
                            ? xbimShapeInstance.StyleLabel
                            : xbimShapeInstance.IfcTypeId * -1;

                        var instanceTransform = ((XbimShapeInstance)xbimShapeInstance).Transformation;
                        var trTransformed = tr.Transform(instanceTransform);

                        var offset = vertices.Count;

                        for (int k = 0; k < trTransformed.Vertices.Count; k++)
                        {
                            var v = trTransformed.Vertices[k];
                            vertices.Add(new Vector3((float)(v.X / scale), (float)(v.Y / scale), (float)(v.Z / scale)));
                        }

                        for (int k = 0; k < trTransformed.Faces.Count; k++)
                        {
                            var face = trTransformed.Faces[k];
                            var indices = face.Indices;
                            for (int z = 0; z < indices.Count; z += 3)
                            {
                                int t0 = indices[z + 0], t1 = indices[z + 1], t2 = indices[z + 2];
                                //triangles.Add(offset + t0);
                                //triangles.Add(offset + t1);
                                //triangles.Add(offset + t2);
                            }
                        }
                    }
                }
            }
        }
    }
}
