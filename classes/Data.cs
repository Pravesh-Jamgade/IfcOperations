using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IfcOperations
{
    public class Data:Item
    {
        [JsonProperty("tag")]
        string tag;

        [JsonProperty("name")]
        string Name;

        [JsonProperty("id")]
        string Id;

        [JsonProperty("boundingBox")]
        BoundingBox BoundingBox;

        [JsonProperty("translation")]
        XYZ Translation;

        [JsonProperty("matrix")]
        double[][] matrix;
        
        [JsonProperty("elementType")]
        string elementType;

        [JsonProperty("level")]
        int level;

        [JsonProperty("childrens")]
        Dictionary<string, Data> children;

        public Data(string name, string id) : base(name, id)
        {
            this.tag = "Parent";
            this.Name = name;
            this.Id = id;
            this.children = new Dictionary<string, Data>();
        }

        [JsonConstructor]
        public Data(string Name, string Id, BoundingBox BoundingBox, XYZ Translation, Matrix<double> matrix):base(Name, Id)
        {
            this.Name = Name;
            this.Id = Id;
            this.BoundingBox = BoundingBox;
            this.Translation = Translation;
            this.matrix = matrix.ToColumnArrays();
        }

        public Data
            (
                string Name, 
                string Id, 
                BoundingBox BoundingBox, 
                XYZ Translation, 
                Matrix<double> matrix,
                string elementType
            ):base(Name, Id)
        {
            this.Name = Name;
            this.Id = Id;
            this.BoundingBox = BoundingBox;
            this.Translation = Translation;
            this.matrix = matrix.ToColumnArrays();
            this.elementType = elementType;
            this.children = new Dictionary<string, Data>();
        }

        internal void AddChild(List<Data> allChildData)
        {
            foreach(Data dataChild in allChildData)
            {
                if(!this.children.TryGetValue(dataChild.Id, out Data value))
                {
                    this.children.Add(dataChild.Id, dataChild);
                }
                
            }
        }
        internal void AddChild(Data allChildData)
        {
            this.children.Add(allChildData.Id, allChildData);
        }

        internal void ChagUnits(double scale)
        {
            if(this.BoundingBox != null)    this.BoundingBox.ChangeUnits(scale);
            if(this.Translation!= null)    this.Translation.ChangeUnits(scale);

            // Translation
            if (this.matrix != null)
            {
                //for (int i = 0; i < this.matrix.Length; i++)
                //{
                //    for (int j = 0; j < this.matrix[i].Count(); j++)
                //    {
                //        if (i == j) continue;
                //        this.matrix[i][j] = this.matrix[i][j] / scale;
                //    }
                //}

                this.matrix[3][0] /= scale;
                this.matrix[3][1] /= scale;
                this.matrix[3][2] /= scale;
            }

            // childrens

            if (this.children != null)
            {
                foreach (Data data in this.children.Values)
                {
                    data.ChagUnits(scale);
                }
            }
        }
    }

    public class BoundingBox
    {
        [JsonProperty("min")]
        XYZ Min;
        [JsonProperty("max")]
        XYZ Max;

        [JsonConstructor]
        public BoundingBox(XYZ Min, XYZ Max)
        {
            this.Max = Max;
            this.Min = Min;
        }

        internal void ChangeUnits(double scale)
        {
            this.Min.ChangeUnits(scale);
            this.Max.ChangeUnits(scale);
        }
    }

    public class XYZ
    {
        [JsonProperty("x")]
        double X;
        [JsonProperty("y")]
        double Y;
        [JsonProperty("z")]
        double Z;

        [JsonConstructor]
        public XYZ(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        internal void ChangeUnits(double scale)
        {
            this.X /= scale;
            this.Y /= scale;
            this.Z /= scale;
        }
    }
}
