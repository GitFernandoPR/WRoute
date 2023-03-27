using System.Collections.Generic;

namespace maps
{

    class Startspoints
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public string name { get; set; }
    }

    class Test
    {
        public int GridX { get; set; }
        public int GridY { get; set; }
        public double MinCoorX { get; set; }
        public double MaxCoorX { get; set; }
        public double MinCoorY { get; set; }
        public double MaxCoorY { get; set; }
        public double sizegrid { get; set; }
        public int Cache { get; set; }
        public string route { get; set; }
    }

    class TestRestUni
    {
        public double Cells0 { get; set; }
        public double Cells1 { get; set; }
        public double Cells2 { get; set; }
        public double Cells3 { get; set; }
        public double Cells4 { get; set; }
        public double Cells5 { get; set; }
        public double Cells6 { get; set; }
        public double Cells7 { get; set; }
        public double Cells8 { get; set; }
        public double Cells9 { get; set; }
        public double Cells10 { get; set; }
        public double Cells11 { get; set; }
        public double Cells12 { get; set; }
        public double Cells13 { get; set; }
        public double Cells14 { get; set; }
        public string Cells15 { get; set; }
    }

    class testResList
    {
        public List<TestRestUni> list = new List<TestRestUni>();
    }

    class output_graph
    {
        public double distance { get; set; }
        public double Fct { get; set; }
        public double EmCO2 { get; set; }
        public double velocity { get; set; }
        public double elevation { get; set; }
    }

    class one_route_inputs
    {
        public int tType { get; set; }
        public int cargo { get; set; }
        public double lblXmax { get; set; }
        public double lblXmin { get; set; }
        public double lblYmax { get; set; }
        public double lblYmin { get; set; }
        public double lblCsize { get; set; }
        public string lblRoute { get; set; }
    }

    class regional_assessment_inputs
    {
        public int Gridx { get; set; }
        public int GridY { get; set; }
        public double point_lat { get; set; }
        public double point_lng { get; set; }
        public int Cycle { get; set; }
        public int tType { get; set; }
        public int cargo_b_a { get; set; }
        public int cargo_a_b { get; set; }
        public double lblXmax { get; set; }
        public double lblXmin { get; set; }
        public double lblYmax { get; set; }
        public double lblYmin { get; set; }
        public double lblCsize { get; set; }
        public string lblRoute { get; set; }
        public bool CBT1 { get; set; }
        public bool CBT2 { get; set; }
        public bool CBT3 { get; set; }
        public bool CBT4 { get; set; }

    }
}
