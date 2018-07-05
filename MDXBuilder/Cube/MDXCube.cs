namespace Wilmar.SSAS.MDXBuilder.Cube
{
    using Wilmar.SSAS.MDXBuilder.Interfaces;


    public  class MDXCube:IBuilder
    {
        private IMDXAxis axis;

        public MDXCube(IMDXAxis axis)
        {
            this.axis = axis;
        }

        public MDXCube(string cubeName)
        {
            this.CubeName = cubeName;
        }

        public MDXCube(string cubeName, IMDXAxis axis)
        {
            this.axis = axis;
            this.CubeName = cubeName;
        }

        public MDXCube(MDXCube subCube, IMDXAxis axis)
        {
            this.axis = axis;
            this.SubCube = subCube;
        }

        public MDXCube SubCube { get; private set; }

        public string CubeName { get; set; }

        public string Build()
        {
            if (this.axis == null)
            {
                return this.CubeName;
            }
            else
            {
                if (this.SubCube == null)
                {
                    return $" (SELECT {this.axis.Build()} FROM {this.CubeName})";
                }
                else
                {
                    return $" (SELECT {this.axis.Build()} FROM {this.SubCube.Build()})";
                }
            }
        }
    }
}
