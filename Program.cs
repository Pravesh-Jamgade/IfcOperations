using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xbim.Common;
using Xbim.Common.Geometry;
using Xbim.Common.Step21;
using Xbim.Ifc;
using Xbim.IO;
using Xbim.ModelGeometry.Scene;

namespace IfcOperations
{
    class Program
    {
        static void Main(string[] args)
        {
            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
            Console.WriteLine("Hello World!");

            const string ifcPath = @"C:\Users\Techture\Desktop\SampleHouse.ifc";
            string dir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop).ToString();
            //string []arg = Environment.GetCommandLineArgs();
            //string ifcPath = args[0];
            //string dir = string.Empty;
            //DirectoryInfo di = Directory.GetParent(ifcPath);
            //dir = di.FullName;

            //if (!Directory.Exists(dir))
            //{
            //    Console.WriteLine("Folder does not exists !");
            //}

            var instCount = 0L;
            //create file types
            XbimSchemaVersion version;
            using (var model = IfcStore.Open(ifcPath))
            {
                version = model.SchemaVersion;
                instCount = model.Instances.Count;
                model.Close();
            }

            //Esent, IFC
            List<Data> allData = new List<Data>();
            List<Data> hAllData = null;
            using (var ifc = File.Open(ifcPath, FileMode.Open))
            {
                using (var model = IfcStore.Open(ifc, StorageType.Ifc, version, XbimModelType.MemoryModel))
                {
                    if (version.ToString() is "Ifc4")
                    {
                        allData = Ifc4.Run(model);
                        hAllData = Ifc4.HierarchyRun(model);
                    }
                    if (version.ToString() is "Ifc2X3")
                    {
                        allData = Ifc2x3.Run(model);
                        hAllData = Ifc2x3.HierarchyRun(model);
                    }
                    //TestClass.Show(model);
                    //Test.TestClass.Run(model);

                    // change unit
                    double scale = model.ModelFactors.OneFoot;
                    foreach (Data data in hAllData)
                    {
                        data.ChagUnits(scale);
                    }
                }
            }

            

            // JSON
            
            string json = JsonConvert.SerializeObject( allData );
            File.WriteAllText(Path.Combine(dir, "IFC-Sample.json"), json);

            json = JsonConvert.SerializeObject(hAllData);
            File.WriteAllText(Path.Combine(dir, "IFC-Sample-H.json"), json);
        }
    }
}






//using(StreamReader stream = new StreamReader(fileName))
//{
//    data = stream.ReadToEnd();
//}

//using (var model = IfcStore.Open(fileName))
//{
//    // get all doors in the model (using IFC4 interface of IfcDoor - this will work both for IFC2x3 and IFC4)


//    List<IIfcWall> allWalls = model.Instances.OfType<IIfcWall>().ToList();

//    var properties = allWalls[0].IsDefinedBy
//        .Where(r => r.RelatingPropertyDefinition is IIfcPropertySet)
//        .SelectMany(r => ((IIfcPropertySet)r.RelatingPropertyDefinition).HasProperties)
//        .OfType<IIfcPropertySingleValue>();

//    var xbim3DModelContext = new Xbim3DModelContext(model);
//    var shapeInstances = xbim3DModelContext.ShapeInstances();

//    XbimShapeGeometry xbimShapeGeometry = xbim3DModelContext.ShapeGeometry(shapeInstances.First());
//    XbimRect3D xbimRect3D = xbimShapeGeometry.BoundingBox;
//    int shapeLabel = xbimShapeGeometry.IfcShapeLabel;
//    string shapeData = xbimShapeGeometry.ShapeData;
//    XbimGeometryType xbimGeometryType = xbimShapeGeometry.Format;

//foreach (var instance in shapeInstances)
//{
//    var geometry = xbim3DModelContext.ShapeGeometry(instance);
//    var data = ((IXbimShapeGeometryData)geometry).ShapeData;
//    using (var stream = new MemoryStream(data))
//    {
//        using (var reader = new BinaryReader(stream))
//        {
//            var mesh = reader.ReadShapeTriangulation();
//        }
//    }
//}

//for (int i = 0; i < allWalls.Count; i++)
//{
//    string id = allWalls[i].GlobalId;
//    Console.WriteLine($"Wall: {id}");

//    var openingsId = allWalls[i].HasOpenings
//    .Select(s => s.RelatedOpeningElement);

//    foreach (var opening in openingsId)
//    {
//        Console.WriteLine($"Opening: {opening.GlobalId}");
//    }
//    Console.WriteLine("\n");
//}

//Console.WriteLine($"Element Id: {id}");
//foreach(var property in properties)
//{
//    Console.WriteLine($"Property: {property.Name}, Value:{property.NominalValue}"); 
//}