//Generic
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using OsmSharp.Math.Geo;
using OsmSharp.Osm.PBF.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.Osm.Interpreter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;



namespace maps
{
    public partial class Main : Form
    {
        private Router router;
        private GMapOverlay routesOverlay3;
        private GMapOverlay overlayOne;
        private GMapOverlay overlayOne2;
        private PointLatLng start;
        private PointLatLng finish;
        private List<GMapPolygon> poligones;
        private List<output_graph> Output_graph;
        private one_route_inputs or_inputs;
        private regional_assessment_inputs ra_inputs;
        private List<PointLatLng> Lista_points_reg;
        private DataSet data_set;
        private int time;
        private bool set_region;
        private int move_x;
        private int move_y;
        private bool flag_move;
        private List<double> points_lat;
        private List<double> points_long;


        public Main()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            InitializeComponent();
            LBL_OSM_DATA_LOAD.Visible = true;
            PB_Inic.Visible = true;
            Load_OSM_Data.RunWorkerAsync();
            MainMap.MapProvider = GMapProviders.GoogleMap;
            MainMap.Position = new PointLatLng(41.808828, -6.760720);
            MainMap.MinZoom = 0;
            MainMap.MaxZoom = 24;
            MainMap.Zoom = 9;
            time = 1;
            Timer.Start();
            routesOverlay3 = new GMapOverlay(MainMap, "Route_fino");
            overlayOne = new GMapOverlay(MainMap, "initial");
            overlayOne2 = new GMapOverlay(MainMap, "Region");
            MainMap.Overlays.Add(routesOverlay3);
            or_inputs = new one_route_inputs();
            ra_inputs = new regional_assessment_inputs();
            Load_Mdt(0);
            data_set = new DataSet();
            new_dataset();
            MainMap.Overlays.Add(overlayOne);
            MainMap.Overlays.Add(overlayOne2);
        }

        private void new_dataset()
        {
            DataTable table = new DataTable("data");
            table.Columns.Add("ID", typeof(int));
            table.Columns["ID"].AutoIncrement = true;
            table.Columns.Add("lat", typeof(string));
            table.Columns.Add("long", typeof(string));
            table.Columns.Add("Dist A-B", typeof(string));
            table.Columns.Add("Dist B-A", typeof(string));
            table.Columns.Add("Dist beeline", typeof(double));
            table.Columns.Add("FC T1", typeof(double));
            table.Columns.Add("CO2 T1", typeof(double));
            table.Columns.Add("FC T2", typeof(double));
            table.Columns.Add("CO2 T2", typeof(double));
            table.Columns.Add("FC T3", typeof(double));
            table.Columns.Add("CO2 T3", typeof(double));
            table.Columns.Add("FC T4", typeof(double));
            table.Columns.Add("CO2 T4", typeof(double));
            data_set.Tables.Add(table);
        }


        private void MainMap_mousemove(object sender, MouseEventArgs e)
        {
            int this_x;
            int this_y;
            if (flag_move)
            {
                this_x = e.X - move_x;
                this_y = e.Y - move_y;

                MainMap.Offset(this_x, this_y);
                move_x = e.X;
                move_y = e.Y;
            }
            Lbl_long.Text = Math.Round(MainMap.FromLocalToLatLng(e.X, e.Y).Lng, 7).ToString();
            Lbl_lat.Text = Math.Round(MainMap.FromLocalToLatLng(e.X, e.Y).Lat, 7).ToString();
            if (set_region == true && points_lat.Count > 0)
            {

                polygone(MainMap.FromLocalToLatLng(e.X, e.Y).Lat, MainMap.FromLocalToLatLng(e.X, e.Y).Lng);
            }
            else
            {
                try
                {
                    lbl_elv.Text = Convert.ToString(elevation(MainMap.FromLocalToLatLng(e.X, e.Y).Lng, MainMap.FromLocalToLatLng(e.X, e.Y).Lat, or_inputs.lblXmax, or_inputs.lblXmin, or_inputs.lblYmax, or_inputs.lblYmin, or_inputs.lblCsize, or_inputs.lblRoute));
                }
                catch { }
            }
        }

        private void polygone(double lat, double lng)
        {
            List<double> part_lat = new List<double>();
            part_lat.Add(points_lat.ElementAt(0));
            part_lat.Add(lat);
            List<double> part_long = new List<double>();
            part_long.Add(points_long.ElementAt(0));
            part_long.Add(lng);
            List<PointLatLng> points = new List<PointLatLng>();
            points.Add(new PointLatLng(part_lat.Min(), part_long.Min()));
            points.Add(new PointLatLng(part_lat.Min(), part_long.Max()));
            points.Add(new PointLatLng(part_lat.Max(), part_long.Max()));
            points.Add(new PointLatLng(part_lat.Max(), part_long.Min()));
            overlayOne2.Polygons.Clear();
            GMapPolygon pol = new GMapPolygon(points, "pol");
            pol.Fill = new SolidBrush(Color.FromArgb(100, Color.Yellow));
            pol.Stroke = new Pen(Color.FromArgb(100, Color.Black));
            overlayOne2.Polygons.Add(pol);
        }

        private void BTN_run_route_Click(object sender, EventArgs e)
        {
            if (Load_OSM_Data.IsBusy == false)
            {

                El_Chart.Series.ElementAt(0).Points.Clear();
                FC_Chart.Series.ElementAt(0).Points.Clear();
                routesOverlay3.Polygons.Clear();
                if (CB_RLoad1.Checked == true) { or_inputs.cargo = 0; };
                if (CB_RLoad2.Checked == true) { or_inputs.cargo = 1; };
                if (CB_RLoad3.Checked == true) { or_inputs.cargo = 2; };
                if (CB_RType1.Checked == true) { or_inputs.tType = 1; };
                if (CB_RType2.Checked == true) { or_inputs.tType = 2; };
                if (CB_RType3.Checked == true) { or_inputs.tType = 3; };
                if (CB_RType4.Checked == true) { or_inputs.tType = 4; };
                SP_Ouput.Panel1Collapsed = false;
                SP_Ouput.Panel2Collapsed = true;
                El_Chart.Series.ElementAt(0).Points.Clear();
                FC_Chart.Series.ElementAt(0).Points.Clear();
                VE_Chart.Series.ElementAt(0).Points.Clear();
                EM_Chart.Series.ElementAt(0).Points.Clear();
                BK_route_assessment.RunWorkerAsync();
            }
        }

        private static double maxVel(string HighwayType, int vehicle)
        {
            if (vehicle == 0)
            {
                if (HighwayType == "motorway") { return 120; }
                if (HighwayType == "trunk") { return 100; }
                if (HighwayType == "primary") { return 100; }
                if (HighwayType == "secondary") { return 90; }
                if (HighwayType == "tertiary") { return 80; }
                if (HighwayType == "unclassified") { return 60; }
                if (HighwayType == "residential") { return 50; }
                if (HighwayType == "service") { return 30; }
                if (HighwayType == "motorway_link") { return 100; }
                if (HighwayType == "trunk_link") { return 80; }
                if (HighwayType == "primary_link") { return 70; }
                if (HighwayType == "secondary_link") { return 60; }
                if (HighwayType == "tertiary_link") { return 50; }
                if (HighwayType == "living_street") { return 30; }
                if (HighwayType == "pedestrian") { return 20; }
                if (HighwayType == "track") { return 20; }
            }
            else
            {
                if (HighwayType == "motorway") { return 100; }
                if (HighwayType == "trunk") { return 80; }
                if (HighwayType == "primary") { return 80; }
                if (HighwayType == "secondary") { return 70; }
                if (HighwayType == "tertiary") { return 70; }
                if (HighwayType == "unclassified") { return 80; }
                if (HighwayType == "residential") { return 40; }
                if (HighwayType == "service") { return 30; }
                if (HighwayType == "motorway_link") { return 60; }
                if (HighwayType == "trunk_link") { return 60; }
                if (HighwayType == "primary_link") { return 50; }
                if (HighwayType == "secondary_link") { return 50; }
                if (HighwayType == "tertiary_link") { return 50; }
                if (HighwayType == "living_street") { return 30; }
                if (HighwayType == "pedestrian") { return 20; }
                if (HighwayType == "track") { return 20; }
            }
            return 100;
        }

        private static double Velodidadacelerada(double velocity, double distance, double Teficiency, double PercTransmisionAxle, double AreaCabin, double weigth, double mu, double slope, double elevation, double power)
        {
            if (distance < 5)
            {
                return velocity;
            }
            else
            {
                for (int i = 0; i <= distance; i++)
                {
                    double F = 9.8066 * weigth * PercTransmisionAxle * mu;
                    double Ft = 3600 * Teficiency * (power / velocity);

                    double c1 = 0.047285;
                    double Cd = 0.7;
                    double Ch = 1 - 0.000085 * elevation;
                    double Ra = c1 * Cd * Ch * AreaCabin * Math.Pow(velocity, 2);

                    double Cr = 1.75;
                    double C2 = 0.0328;
                    double C3 = 4.575;
                    double Rr = 9.8066 * Cr * (C2 * velocity + C3) * weigth / 1000;

                    double RG = 9.8066 * weigth * slope / 100;
                    double R = Ra + Rr + RG;

                    double a = (Math.Min(F, Ft) - R) / weigth;
                    double t = 1 / (velocity);
                    velocity = velocity + (a * t);
                }
            }
            return velocity;
        }

        private static double Radio_curva(double X1, double Y1, double X2, double Y2, double X3, double Y3)
        {
            double Ratio;
            try
            {
                double a = Math.Sqrt(Math.Pow(X1 - X2, 2) + Math.Pow(Y1 - Y2, 2)) * 100;
                double b = Math.Sqrt(Math.Pow(X2 - X3, 2) + Math.Pow(Y2 - Y3, 2)) * 100;
                double c = Math.Sqrt(Math.Pow(X3 - X1, 2) + Math.Pow(Y3 - Y1, 2)) * 100;
                double denRatio = (Math.Sin(Math.Acos((Math.Pow(a, 2) - Math.Pow(b, 2) - Math.Pow(c, 2)) / (-2 * b * c)))) * 2;

                if (denRatio < 0.0001 || double.IsNaN(denRatio))
                {
                    Ratio = 0;
                }
                else
                {
                    Ratio = a / (Math.Sin(Math.Acos((Math.Pow(a, 2) - Math.Pow(b, 2) - Math.Pow(c, 2)) / (-2 * b * c)))) * 2;
                }
            }
            catch
            {
                Ratio = 0;
            }

            return Ratio * 1000;
        }

        private static int slopeclass(double slope)
        {
            int slopecase;
            slope = slope * 100;
            if (slope < -6)
            {
                slopecase = -6;
            }
            else if (slope < -4)
            {
                slopecase = -4;
            }
            else if (slope < -2)
            {
                slopecase = -2;
            }
            else if (slope < 0)
            {
                slopecase = 0;
            }
            else if (slope < 2)
            {
                slopecase = 0;
            }
            else if (slope < 4)
            {
                slopecase = 2;
            }
            else if (slope < 6)
            {
                slopecase = 4;
            }
            else
            {
                slopecase = 6;
            }
            return slopecase;
        }


        private static double Consumption(double velocity, int trucktype, double slope, int cargo)
        {
            double result = 0;
            if (velocity < 12)
            {
                velocity = 12;
            }
            double a, b, c, d, e;
            switch (slopeclass(slope))
            {
                case -6:
                    //SLOPE -6%
                    switch (trucktype)
                    {
                        //TRUCK TYPE I
                        case 1:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 664.55822724278; b = 0.957002239635257; c = -0.513474825736931;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(a+(b/(1+exp((((-1)*c)+(d*ln(x)))+(e*x)))))
                                    a = -7.97776114024111; b = 182.132638149312; c = 5.51537459372131; d = 2.09751864253246; e = -0.00894663759019215;
                                    result = (a + (b / (1 + Math.Exp((((-1) * c) + (d * Math.Log(velocity))) + (e * velocity)))));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(a+(b/(1+exp((((-1)*c)+(d*ln(x)))+(e*x)))))			
                                    a = -7.53941954702805; b = 166.299793705797; c = 5.81767051683045; d = 2.16776220481814; e = -0.00961301463610918;
                                    result = (a + (b / (1 + Math.Exp((((-1) * c) + (d * Math.Log(velocity))) + (e * velocity)))));
                                    return result;
                            }
                            break;
                        //TRUCK TYPE II
                        case 2:
                            switch (cargo)
                            {
                                //load 0%
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //rxp((a+(b/x))+(c*ln(x)))
                                    a = 13.758598546874; b = -23.4426706549724; c = -2.69624317174424;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(a+(b/(1+rxp((((-1)*c)+(d*ln(x)))+(e*x)))))
                                    a = -14.5586498699863; b = 311.605013156162; c = 4.65952686279799; d = 1.84563952938403; e = -0.0060371879341371;
                                    result = (a + (b / (1 + Math.Exp((((-1) * c) + (d * Math.Log(velocity))) + (e * velocity)))));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(a+(b/(1+rxp((((-1)*c)+(d*ln(x)))+(e*x)))))
                                    a = -12.6219635697248; b = 245.528136855828; c = 5.67144831409705; d = 2.08818286241628; e = -0.00843567447266535;
                                    result = (a + (b / (1 + Math.Exp((((-1) * c) + (d * Math.Log(velocity))) + (e * velocity)))));
                                    return result;
                            }
                            break;
                        //TRUCK TYPE III
                        case 3:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //exp((a+(b/x))+(c*ln(x)))
                                    a = 14.105519329482; b = -25.4499924632811; c = -2.81737410796156;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(a+(b/(1+rxp((((-1)*c)+(d*ln(x)))+(e*x)))))
                                    a = -10.6240788898359; b = 228.768870376751; c = 5.86948749955834; d = 2.18139225441586; e = -0.00996159051700096;
                                    result = (a + (b / (1 + Math.Exp((((-1) * c) + (d * Math.Log(velocity))) + (e * velocity)))));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(a+(b/(1+rxp((((-1)*c)+(d*ln(x)))+(e*x)))))
                                    a = -9.5945151713242; b = 193.860298590713; c = 6.7541125997592; d = 2.39603490061964; e = -0.0120615619587493;
                                    result = (a + (b / (1 + Math.Exp((((-1) * c) + (d * Math.Log(velocity))) + (e * velocity)))));
                                    return result;
                            }

                            break;
                        //TRUCK TYPE IV
                        case 4:
                            switch (cargo)
                            {
                                //load 0%
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //exp((a+(b/x))+(c*ln(x)))
                                    a = 14.2863506775255; b = -24.3223120217125; c = -2.81542027576167;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //exp((a+(b/x))+(c*ln(x)))
                                    a = 14.8488276858479; b = -28.3881244596625; c = -2.93967729889085;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //exp((a+(b/x))+(c*ln(x)))
                                    a = 15.2151953472999; b = -31.5579123632681; c = -2.99663310321911;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                            }
                            break;
                    }
                    break;
                case -4:
                    //SLOPE -4%
                    switch (trucktype)
                    {
                        //TRUCK TYPE I
                        case 1:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 82) { velocity = 82; }
                                    //Exp((a+(b/x))+(c*ln(x)))
                                    a = 11.2824701196395; b = -15.586500240322; c = -2.01772006163588; d = 0; e = 0;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 82) { velocity = 82; }
                                    //rxp((a+(b/x))+(c*ln(x)))
                                    a = 12.0564957491207; b = -19.5329773887566; c = -2.20670219678881; d = 0; e = 0;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                                //load 100%;
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 82) { velocity = 82; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.000825378839451085; b = 0.154226517184361; c = -9.98512944361134; d = 237.76994241817; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE II
                        case 2:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 82) { velocity = 82; }
                                    //((a*(b^x))*(x^c))
                                    a = 1188.53252709325; b = 0.968927724435374; c = -0.515911919556323; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 82) { velocity = 82; }
                                    //((a*(b^x))*(x^c))
                                    a = 759.806312128011; b = 0.962138294364368; c = -0.30990589613834; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 82) { velocity = 82; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00118137979943617; b = 0.22442669771308; c = -14.9559743969259; d = 368.971175260408; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE III
                        case 3:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 82) { velocity = 82; }
                                    //((a*(b^x))*(x^c))
                                    a = 930.326416503948; b = 0.965467208554786; c = -0.446524869280186; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 82) { velocity = 82; }
                                    //((a*(b^x))*(x^c))
                                    a = 594.305717628964; b = 0.959513213846961; c = -0.238591990829544; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 82) { velocity = 82; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00108742681455617; b = 0.206729135796017; c = -13.8036473402313; d = 341.582947074713; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE IV
                        case 4:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 81) { velocity = 81; }
                                    //((a*(b^x))*(x^c))
                                    a = 1417.73071780127; b = 0.966154210322586; c = -0.518667290655703; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 81) { velocity = 81; }
                                    //((a*(b^x))*(x^c))
                                    a = 701.98610337866; b = 0.956938871425916; c = -0.181019380596168; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 81) { velocity = 81; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.0014343411816492; b = 0.273837889873391; c = -18.4713732166938; d = 464.159149738577; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                    }
                    break;
                case -2:
                    //SLOPE -2%
                    switch (trucktype)
                    {
                        //TRUCK TYPE I
                        case 1:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 2917.75948879965; b = 1.01256285444207; c = -1.14309335984722; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //rxp((a+(b/x))+(c*ln(x)))
                                    a = 7.92926718001571; b = -2.47834304082217; c = -0.9694031742729; d = 0; e = 0;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00100263321065985; b = 0.187635434469476; c = -12.3608457196763; d = 332.176622628711; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE II
                        case 2:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //rxp((a+(b/x))+(c*ln(x)))
                                    a = 8.2647890964261; b = -1.48576453050482; c = -0.965416196524684; d = 0; e = 0;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00167798137563478; b = 0.305808955658635; c = -19.7671927531518; d = 518.630791270125; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00159899502309364; b = 0.29393718193545; c = -19.6981910483888; d = 546.273952452347; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE III
                        case 3:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //rxp((a+(b/x))+(c*ln(x)))
                                    a = 9.00516568065961; b = -5.19468166118337; c = -1.18091738720245; d = 0; e = 0;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00150424545936582; b = 0.275941774114231; c = -18.042470449706; d = 478.225762499314; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00146805954322845; b = 0.270660221722843; c = -18.2900584226574; d = 512.428835299467; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE IV
                        case 4:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //rxp((a+(b/x))+(c*ln(x)))
                                    a = 9.80963072137431; b = -7.34644929203937; c = -1.34348101490717; d = 0; e = 0;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.0019590204105398; b = 0.358513387437993; c = -23.6427264788047; d = 634.564295291167; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00187150972407779; b = 0.347396161922084; c = -24.2019135873633; d = 708.188992973386; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                    }
                    break;
                case 0:
                    //SLOPE 0%
                    switch (trucktype)
                    {
                        //TRUCK TYPE I
                        case 1:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 2028.45509865951; b = 1.01462460616189; c = -0.898326952809425; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 1691.06730483651; b = 1.01070967946185; c = -0.763813261930301; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 1478.6154867383; b = 1.00742813786235; c = -0.654038763343325; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                            }
                            break;
                        //TRUCK TYPE II
                        case 2:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 2509.19967826415; b = 1.0081737581775; c = -0.76326237829551; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 2072.22852621348; b = 1.00400104118124; c = -0.606854375354946; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 1817.44605702115; b = 1.00055699572704; c = -0.484127112967542; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                            }
                            break;
                        //TRUCK TYPE III
                        case 3:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 2478.59362839227; b = 1.00882895267449; c = -0.789091646546731; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 1915.30297702991; b = 1.00356374940078; c = -0.594467895407732; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00136948128701893; b = 0.271843719842682; c = -19.1575681265127; d = 713.753017415658; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE IV
                        case 4:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 3135.99224765336; b = 1.00781829447097; c = -0.795221851460223; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 2183.59424240723; b = 1.0005482890017; c = -0.509936901309526; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00178238891416501; b = 0.350877010889144; c = -25.5700918784972; d = 1017.36980976262; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                    }
                    break;
                case 2:
                    //SLOPE 2%
                    switch (trucktype)
                    {
                        //TRUCK TYPE I
                        case 1:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 1443.05538086768; b = 1.01267873029858; c = -0.666495275778436; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 1285.83854543731; b = 1.00992975904976; c = -0.541410608350016; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 1216.01733916939; b = 1.00774479107794; c = -0.450775096371809; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                            }
                            break;
                        //TRUCK TYPE II
                        case 2:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 2119.25600159159; b = 1.00922794171909; c = -0.610601661233503; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 1915.84228550406; b = 1.00683746375621; c = -0.469826255773856; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 1875.38490958218; b = 1.00485501712932; c = -0.376467357826823; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                            }
                            break;
                        //TRUCK TYPE III
                        case 3:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    a = 1975.4005684026; b = 1.00936075945887; c = -0.60316304365991; d = 0; e = 0;
                                    //((a*(b^x))*(x^c))
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //((a*(b^x))*(x^c))
                                    a = 1737.94916739452; b = 1.00622611039921; c = -0.440597793954862; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 86) { velocity = 86; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00116601022614368; b = 0.242784460772124; c = -16.9497730590846; d = 924.936524932731; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE IV
                        case 4:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 76) { velocity = 76; }
                                    //((a*(b^x))*(x^c))
                                    a = 2561.61862865815; b = 1.00935301711544; c = -0.62532685871632; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 76) { velocity = 76; }
                                    //((a*(b^x))*(x^c))
                                    a = 2148.83618671908; b = 1.00476251868568; c = -0.385169513561421; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 76) { velocity = 76; }
                                    //((a*(b^x))*(x^c))
                                    a = 2174.17090368449; b = 1.00161675002583; c = -0.264482034882315; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                            }
                            break;
                    }
                    break;
                case 4:
                    //SLOPE 4%
                    switch (trucktype)
                    {
                        //TRUCK TYPE I
                        case 1:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 73) { velocity = 73; }
                                    //((a*(b^x))*(x^c))
                                    a = 1211.93791942604; b = 1.01056999871142; c = -0.506344779873494; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 73) { velocity = 73; }
                                    //((a*(b^x))*(x^c))
                                    a = 1063.36924862027; b = 1.00703664064666; c = -0.357055191898852; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 73) { velocity = 73; }
                                    //((a*(b^x))*(x^c))
                                    a = 1048.98656651364; b = 1.00481634536263; c = -0.271974632091579; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                            }
                            break;
                        //TRUCK TYPE II
                        case 2:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 70) { velocity = 70; }
                                    //((a*(b^x))*(x^c))
                                    a = 1945.72977168235; b = 1.00885582472937; c = -0.49358588683008; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 70) { velocity = 70; }
                                    //((a*(b^x))*(x^c))
                                    a = 1709.11082316738; b = 1.00522478655392; c = -0.316049039457634; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 70) { velocity = 70; }
                                    //((a*(b^x))*(x^c))
                                    a = 1777.37939138248; b = 1.00327779816125; c = -0.233683571480636; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                            }
                            break;
                        //TRUCK TYPE III
                        case 3:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 66) { velocity = 66; }
                                    //((a*(b^x))*(x^c))
                                    a = 1695.34593814388; b = 1.00814988410241; c = -0.450897477305905; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 66) { velocity = 66; }
                                    //((a*(b^x))*(x^c))
                                    a = 1521.95986951551; b = 1.00419842459242; c = -0.272479040536237; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 66) { velocity = 66; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00236628933428141; b = 0.354921035442941; c = -18.9523706369173; d = 1201.16000353767; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE IV
                        case 4:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 51) { velocity = 51; }
                                    //((a*(b^x))*(x^c))
                                    a = 2178.92252441812; b = 1.0082995970743; c = -0.47154682792687; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 50%;
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 51) { velocity = 51; }
                                    //Exp((a+(b/x))+(c*ln(x)))
                                    a = 7.00456737111946; b = 2.23885449153651; c = -0.0537346476916945; d = 0; e = 0;
                                    result = Math.Exp((a + (b / velocity)) + (c * Math.Log(velocity)));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 51) { velocity = 51; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00677637244267044; b = 0.752455006155616; c = -32.6911709928431; d = 1870.65691654692; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                    }
                    break;
                case 6:
                    //SLOPE 6%
                    switch (trucktype)
                    {
                        //TRUCK TYPE I
                        case 1:
                            switch (cargo)
                            {
                                //load 100%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 59) { velocity = 59; }
                                    //((a*(b^x))*(x^c))
                                    a = 1029.70317638197; b = 1.0076370887377; c = -0.358751300064334; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 59) { velocity = 59; }
                                    //((a*(x^b))+(c*(x^d)))
                                    a = 1488.0338055235; b = -0.684967032799408; c = 215.967615627223; d = 0.153567354257417; e = 0;
                                    result = ((a * Math.Pow(velocity, b)) + (c * Math.Pow(velocity, d)));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 59) { velocity = 59; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00241094760092731; b = 0.31765891047326; c = -14.4666831689337; d = 842.454788104008; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE II
                        case 2:
                            switch (cargo)
                            {
                                //load 0%
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 52) { velocity = 52; }
                                    //((a*(x^b))+(c*(x^d)))
                                    a = 177.107004462942; b = 0.237186731421663; c = 3411.25509650248; d = -0.825888480907287; e = 0;
                                    result = ((a * Math.Pow(velocity, b)) + (c * Math.Pow(velocity, d)));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 52) { velocity = 52; }
                                    //((a*(b^x))*(x^c))
                                    a = 1767.14415865984; b = 1.00429660368781; c = -0.237912520388382; d = 0; e = 0;
                                    result = ((a * Math.Pow(b, velocity)) * Math.Pow(velocity, c));
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 52) { velocity = 52; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.006004155034518; b = 0.707802355811556; c = -29.3401749879166; d = 1576.66015994353; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE III
                        case 3:
                            switch (cargo)
                            {
                                //load 0%
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 50) { velocity = 50; }
                                    //((a*(x^b))+(c*(x^d)))
                                    a = 233.337566959406; b = 0.180723645719976; c = 2927.94113834455; d = -0.830322022812569; e = 0;
                                    result = ((a * Math.Pow(velocity, b)) + (c * Math.Pow(velocity, d)));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 50) { velocity = 50; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00267269512943653; b = 0.386125748534289; c = -19.0933277705111; d = 1198.07688839982; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 50) { velocity = 50; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00548535343927112; b = 0.604789024569413; c = -25.0077228985182; d = 1534.04130625237; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                        //TRUCK TYPE IV
                        case 4:
                            switch (cargo)
                            {
                                //load 0%;
                                case 0:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 37) { velocity = 37; }
                                    //((a*(x^b))+(c*(x^d)))
                                    a = 4097.53804796675; b = -0.888360677420922; c = 296.507469840635; d = 0.1697847062043; e = 0;
                                    result = ((a * Math.Pow(velocity, b)) + (c * Math.Pow(velocity, d)));
                                    return result;
                                //load 50%
                                case 1:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 37) { velocity = 37; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.00895755505641522; b = 0.983453375776278; c = -37.9680380152179; d = 1805.08699981613; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                                //load 100%
                                case 2:
                                    if (velocity < 12) { velocity = 12; }
                                    if (velocity > 37) { velocity = 37; }
                                    //(((a*(x^3))+(b*(x^2))+(c*x))+d)
                                    a = -0.0357515579032402; b = 2.85998945705869; c = -81.5013537495611; d = 2682.06190673814; e = 0;
                                    result = (((a * Math.Pow(velocity, 3)) + (b * Math.Pow(velocity, 2)) + (c * velocity)) + d);
                                    return result;
                            }
                            break;
                    }
                    break;
            }
            return result;
        }

        private void TSM_position_i_Click(object sender, EventArgs e)
        {
            start = new PointLatLng();
            start.Lat = Convert.ToDouble(Lbl_lat.Text);
            start.Lng = Convert.ToDouble(Lbl_long.Text);
            BK_route_assessment.CancelAsync();
            overlayOne.Markers.Clear();
            overlayOne.Markers.Add(new GMap.NET.WindowsForms.Markers.GMapMarkerGoogleGreen(start));
            TSM_position_i.Checked = true;
            if (TSM_position_f.Checked == true)
            {
                overlayOne.Markers.Add(new GMap.NET.WindowsForms.Markers.GMapMarkerGoogleRed(finish));
            }
        }

        private void Clear_data_Click(object sender, EventArgs e)
        {
            routesOverlay3.Polygons.Clear();
            overlayOne.Markers.Clear();
            start = new PointLatLng();
            finish = new PointLatLng();
            BK_route_assessment.CancelAsync();
        }

        private void TSM_position_f_Click(object sender, EventArgs e)
        {
            BK_route_assessment.CancelAsync();
            finish = new PointLatLng();
            finish.Lat = Convert.ToDouble(Lbl_lat.Text);
            finish.Lng = Convert.ToDouble(Lbl_long.Text);
            overlayOne.Markers.Clear();
            overlayOne.Markers.Add(new GMap.NET.WindowsForms.Markers.GMapMarkerGoogleRed(finish));
            overlayOne.Markers.Add(new GMap.NET.WindowsForms.Markers.GMapMarkerGoogleGreen(start));
            TSM_position_f.Checked = true;
            if (TSM_position_i.Checked == true)
            {
                overlayOne.Markers.Add(new GMap.NET.WindowsForms.Markers.GMapMarkerGoogleGreen(start));
            }
        }


        private void BTN_run_region_Click(object sender, EventArgs e)
        {
            if (Load_OSM_Data.IsBusy == false)
            {
                if (points_lat.Count < 2 || TSM_position_i.Checked == false)
                {
                    MessageBox.Show("Select start point and region");
                }
                else
                {

                    ra_inputs.cargo_a_b = 0;
                    if (LoadAB1.Checked == true)
                    {
                        ra_inputs.cargo_a_b = 1;
                    }
                    if (LoadAB2.Checked == true)
                    {
                        ra_inputs.cargo_a_b = 2;
                    }
                    ra_inputs.cargo_b_a = 0;
                    if (LoadBA1.Checked == true)
                    {
                        ra_inputs.cargo_b_a = 1;
                    }
                    if (LoadBA2.Checked == true)
                    {
                        ra_inputs.cargo_b_a = 2;
                    }
                    ra_inputs.Gridx = Convert.ToInt16(TXT_grid_x.Text);
                    ra_inputs.GridY = Convert.ToInt16(TXT_Grid_Y.Text);
                    ra_inputs.point_lat = start.Lat;
                    ra_inputs.point_lng = start.Lng;
                    ra_inputs.CBT1 = CBT1.Checked;
                    ra_inputs.CBT2 = CBT2.Checked;
                    ra_inputs.CBT3 = CBT3.Checked;
                    ra_inputs.CBT4 = CBT4.Checked;
                    time = 1;
                    Timer.Start();
                    Lista_points_reg = new List<PointLatLng>();
                    BK_regional_assessment.RunWorkerAsync();
                    SP_Ouput.Panel1Collapsed = true;
                    SP_Ouput.Panel2Collapsed = false;
                    data_set.Tables["data"].Rows.Clear();
                    El_Chart.Series.ElementAt(0).Points.Clear();
                    FC_Chart.Series.ElementAt(0).Points.Clear();
                    VE_Chart.Series.ElementAt(0).Points.Clear();
                    EM_Chart.Series.ElementAt(0).Points.Clear();
                }
            }
            else
            {
                MessageBox.Show("The program is loading the the static OpenStreetMap file in memory. Please wait...  ", "Confirm", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Information);
            }
        }

        private static double elevation(double x, double y, double maxX, double minX, double maxY, double minY, double size, string route)
        {
            try
            {
                double MinCoorX = minX;
                double MaxCoorX = maxX;
                double MinCoorY = minY;
                double MaxCoorY = maxY;
                double sizegrid = size;
                if (y > MaxCoorY) { return -9999; }
                if (y < MinCoorY) { return -9999; }
                if (x > MaxCoorX) { return -9999; }
                if (x < MinCoorX) { return -9999; }
                string a = "-9999";
                int counter = 0;
                string line;
                int xid = Convert.ToInt32((x - MinCoorX) / sizegrid);
                int yid = Convert.ToInt32((MaxCoorY - y) / sizegrid);
                // Read the file and display it line by line.
                System.IO.StreamReader file =
                    new System.IO.StreamReader(route);
                while ((line = file.ReadLine()) != null)
                {
                    if (counter == yid + 5)
                    {
                        string b = "";
                        b = line;
                        try
                        {
                            a = b.Split(new Char[] { ',', ' ' }).ElementAt(xid).ToString();
                        }
                        catch
                        {
                            a = "-9999";
                        }
                    }
                    counter++;
                }

                file.Close();
                return Convert.ToDouble(a);
            }
            catch
            {
                return -9999;
            }
        }

        private static double[,] elevationMatrix(double maxX, double minX, double maxY, double minY, double size, string route)
        {
            int counter = 0;
            int counter2 = 0;
            string line;
            int xid = Convert.ToInt32((maxX - minX) / size);
            int yid = Convert.ToInt32((maxY - minY) / size);
            double[,] mT = new double[xid, yid];
            // Read the file and display it line by line.
            StreamReader file = new StreamReader(route);
            while ((line = file.ReadLine()) != null)
            {
                if (counter > 5)
                {
                    string b = "";
                    b = line;
                    for (int i = 0; i < b.Split(new char[] { ',', ' ' }).Count() - 1; i++)
                    {
                        mT[i, counter2] = Convert.ToDouble(b.Split(new char[] { ',', ' ' }).ElementAt(i).ToString());
                    }
                    counter2++;
                }
                counter++;
            }
            file.Close();
            return mT;
        }

        private void Load_Mdt(int mode)
        {
            int ncols = 0;
            int nrows = 0;
            double xllcorner = 0;
            double yllcorner = 0;
            double cellsize = 0;
            string FileName = new FileInfo("rastertPTmdt.txt").FullName;
            if (mode == 1)
            {
                OpenFileDialog a = new OpenFileDialog();
                a.ShowDialog();
                if (a.FileName != null && a.FileName != "")
                {
                    FileName = a.FileName;
                }
            }
            string line;

            StreamReader file = new StreamReader(FileName);
            int counter = 0;
            while ((line = file.ReadLine()) != null || counter < 7)
            {
                string b;
                b = line;
                b = b.Replace(",", ".");
                if (b.Split(new char[] { ' ' }).ElementAt(0).ToString() == "ncols")
                {
                    b = b.Replace(" ", "");
                    b = b.Replace("ncols", "ncols ");
                    ncols = Convert.ToInt32(b.Split(new char[] { ' ' }).ElementAt(1).ToString());
                }
                if (b.Split(new char[] { ',', ' ' }).ElementAt(0).ToString() == "nrows")
                {
                    b = b.Replace(" ", "");
                    b = b.Replace("nrows", "nrows ");
                    nrows = Convert.ToInt32(b.Split(new char[] { ' ' }).ElementAt(1).ToString());
                }
                if (b.Split(new char[] { ',', ' ' }).ElementAt(0).ToString() == "xllcorner")
                {
                    b = b.Replace(" ", "");
                    b = b.Replace("xllcorner", "xllcorner ");
                    xllcorner = Convert.ToDouble(b.Split(new char[] { ' ' }).ElementAt(1).ToString());
                }
                if (b.Split(new char[] { ',', ' ' }).ElementAt(0).ToString() == "yllcorner")
                {
                    b = b.Replace(" ", "");
                    b = b.Replace("yllcorner", "yllcorner ");
                    yllcorner = Convert.ToDouble(b.Split(new char[] { ' ' }).ElementAt(1).ToString());
                }
                if (b.Split(new char[] { ',', ' ' }).ElementAt(0).ToString() == "cellsize")
                {
                    b = b.Replace(" ", "");
                    b = b.Replace("cellsize", "cellsize ");
                    cellsize = Convert.ToDouble(b.Split(new char[] { ' ' }).ElementAt(1).ToString());
                }
                counter++;
            }
            or_inputs.lblRoute = FileName;
            or_inputs.lblXmin = xllcorner;
            or_inputs.lblYmin = yllcorner;
            or_inputs.lblXmax = xllcorner + (ncols * cellsize);
            or_inputs.lblYmax = yllcorner + (nrows * cellsize);
            or_inputs.lblCsize = cellsize;
            ra_inputs.lblRoute = FileName;
            ra_inputs.lblXmin = xllcorner;
            ra_inputs.lblYmin = yllcorner;
            ra_inputs.lblXmax = xllcorner + (ncols * cellsize);
            ra_inputs.lblYmax = yllcorner + (nrows * cellsize);
            ra_inputs.lblCsize = cellsize;
        }



        private void BK_region_assessment_DoWork(object sender, DoWorkEventArgs e)
        {
            PointLatLng start;
            PointLatLng end;
            double[,] elevationMT;
            elevationMT = elevationMatrix(ra_inputs.lblXmax, ra_inputs.lblXmin, ra_inputs.lblYmax, ra_inputs.lblYmin, ra_inputs.lblCsize, ra_inputs.lblRoute);
            double star_lat = ra_inputs.point_lat;
            double star_lon = ra_inputs.point_lng;
            double Lat_recogida;
            double Lon_recogida;
            string Type_road;
            //testResList lista = new testResList();
            PointLatLng a = new PointLatLng();
            a.Lat = star_lat;
            a.Lng = star_lon;
            double aa = ra_inputs.Gridx;
            double bb = ra_inputs.GridY;
            double end_lon;
            double end_lat;
            int contador_ext = 0;
            for (int i = 0; i <= aa; i++)
            {
                for (int j = 0; j <= bb; j++)
                {
                    try
                    {
                        end_lon = points_long.Max() - (points_long.Max() - points_long.Min()) / aa * i;
                        end_lat = points_lat.Max() - (points_lat.Max() - points_lat.Min()) / bb * j;
                        //end_lon = (-8.15) + ((8.15 - 6.10) / aa) * i;
                        //end_lat = (42.02) - ((42.02 - 40.90) / bb) * j;
                        start = new PointLatLng(star_lat, star_lon);
                        end = new PointLatLng(end_lat, end_lon);
                        double CType1 = 0;
                        double CO2Type1 = 0;
                        double CType2 = 0;
                        double CO2Type2 = 0;
                        double CType3 = 0;
                        double CO2Type3 = 0;
                        double CType4 = 0;
                        double CO2Type4 = 0;
                        Type_road = "--";
                        // trayecto de ida
                        var resolved1 = router.Resolve(Vehicle.BigTruck, 1, new GeoCoordinate(start.Lat, start.Lng));
                        var resolved2 = router.Resolve(Vehicle.BigTruck, 1, new GeoCoordinate(end.Lat, end.Lng));
                        // calculate route.
                        var route = router.Calculate(Vehicle.BigTruck, resolved1, resolved2);
                        int counter = 0;
                        double velocidad1 = 0.1;
                        double velocidad2 = 0.1;
                        double velocidad3 = 0.1;
                        double velocidad4 = 0.1;
                        double lat = 0, lng = 0;
                        double maxvelo = 0.1;
                        double maxvelo2 = 0.1;
                        double distanceAB = 0;
                        double distanceBA = 0;
                        foreach (var item in route.Entries)
                        {
                            //punto inicial
                            if (counter == 0)
                            {
                                lat = item.Latitude;
                                lng = item.Longitude;
                            }
                            if (counter == 1)
                            {
                                double distancepoint = Math.Sqrt(Math.Pow(lat - item.Latitude, 2) + Math.Pow(lng - item.Longitude, 2)) * 100000;
                                distanceAB = distanceAB + distancepoint;
                                double elevationB;
                                double elevationA;
                                int xid = Convert.ToInt32((lng - ra_inputs.lblXmin) / ra_inputs.lblCsize);
                                int yid = Convert.ToInt32((ra_inputs.lblYmax - lat) / ra_inputs.lblCsize);
                                elevationA = elevationMT[xid, yid];
                                int xid2 = Convert.ToInt32((item.Longitude - ra_inputs.lblXmin) / ra_inputs.lblCsize);
                                int yid2 = Convert.ToInt32((ra_inputs.lblYmax - item.Latitude) / ra_inputs.lblCsize);
                                elevationB = elevationMT[xid2, yid2];
                                double slopeA = (elevationB - elevationA) / distancepoint;
                                foreach (var item2 in item.Tags)
                                {
                                    if (item2.Key == "highway")
                                    {
                                        maxvelo = maxVel(item2.Value, 1);
                                        maxvelo2 = maxVel(item2.Value, 0);
                                    }
                                }
                                foreach (var item2 in item.Tags)
                                {
                                    if (item2.Key == "maxspeed")
                                    {
                                        maxvelo = Convert.ToDouble(item2.Value);
                                        maxvelo2 = maxvelo;
                                    }
                                }
                                if (ra_inputs.CBT1 == true)
                                {
                                    double velocidadA = Velodidadacelerada(velocidad1, distancepoint, 0.86, 0.67, 6.80, 5000 + 4000 * ra_inputs.cargo_a_b, 0.7, slopeA, elevationB, 200);
                                    velocidad1 = Math.Min(velocidadA, maxvelo);
                                    double FC1 = Consumption(velocidad1, 1, slopeA, ra_inputs.cargo_a_b);
                                    CType1 = CType1 + FC1 / (1000 * 0.83) * (distancepoint / 1000);
                                    CO2Type1 = CO2Type1 + FC1 * 3.16 * (distancepoint / 1000);
                                }
                                if (ra_inputs.CBT2 == true)
                                {
                                    double velocidadB = Velodidadacelerada(velocidad2, distancepoint, 0.89, 0.80, 6.80, 10000 + 6000 * ra_inputs.cargo_a_b, 0.7, slopeA, elevationB, 250);
                                    velocidad2 = Math.Min(velocidadB, maxvelo);
                                    double FC2 = Consumption(velocidad2, 2, slopeA, ra_inputs.cargo_a_b);
                                    CType2 = CType2 + FC2 / (1000 * 0.83) * (distancepoint / 1000);
                                    CO2Type2 = CO2Type2 + FC2 * 3.16 * (distancepoint / 1000);
                                }
                                if (ra_inputs.CBT3 == true)
                                {
                                    double velocidadC = Velodidadacelerada(velocidad3, distancepoint, 0.89, 0.31, 10, 12000 + 6000 * ra_inputs.cargo_a_b, 0.7, slopeA, elevationB, 330);
                                    velocidad3 = Math.Min(velocidadC, maxvelo);
                                    double FC3 = Consumption(velocidad3, 3, slopeA, ra_inputs.cargo_a_b);
                                    CType3 = CType3 + FC3 / (1000 * 0.83) * (distancepoint / 1000);
                                    CO2Type3 = CO2Type3 + FC3 * 3.16 * (distancepoint / 1000);
                                }
                                if (ra_inputs.CBT4 == true)
                                {
                                    double velocidadD = Velodidadacelerada(velocidad4, distancepoint, 0.89, 0.435, 10.7, 20000 + 10000 * ra_inputs.cargo_a_b, 0.7, slopeA, elevationB, 500);
                                    velocidad4 = Math.Min(velocidadD, maxvelo);
                                    double FC4 = Consumption(velocidad4, 4, slopeA, ra_inputs.cargo_a_b);
                                    CType4 = CType4 + FC4 / (1000 * 0.83) * (distancepoint / 1000);
                                    CO2Type4 = CO2Type4 + FC4 * 3.16 * (distancepoint / 1000);
                                }
                                lat = item.Latitude;
                                lng = item.Longitude;
                            }

                            //puntos siguientes
                            if (counter > 1 && counter < route.Entries.Count() - 2)
                            {
                                double distancepoint = Math.Sqrt(Math.Pow(lat - item.Latitude, 2) + Math.Pow(lng - item.Longitude, 2)) * 100000;
                                distanceAB = distanceAB + distancepoint;
                                //Slope
                                double elevationB;
                                double elevationA;
                                int xid = Convert.ToInt32((lng - ra_inputs.lblXmin) / ra_inputs.lblCsize);
                                int yid = Convert.ToInt32((ra_inputs.lblYmax - lat) / ra_inputs.lblCsize);
                                elevationA = elevationMT[xid, yid];
                                int xid2 = Convert.ToInt32((item.Longitude - ra_inputs.lblXmin) / ra_inputs.lblCsize);
                                int yid2 = Convert.ToInt32((ra_inputs.lblYmax - item.Latitude) / ra_inputs.lblCsize);
                                elevationB = elevationMT[xid2, yid2];
                                double slopeA = (elevationB - elevationA) / distancepoint;
                                //Max Legal speed
                                foreach (var item2 in item.Tags)
                                {
                                    if (item2.Key == "highway")
                                    {
                                        maxvelo = maxVel(item2.Value, 1);
                                        maxvelo2 = maxVel(item2.Value, 0);
                                    }
                                }
                                foreach (var item2 in item.Tags)
                                {
                                    if (item2.Key == "maxspeed")
                                    {
                                        maxvelo = Convert.ToDouble(item2.Value);
                                        maxvelo2 = maxvelo;
                                    }
                                }
                                //Radio_curva
                                double Radio = Radio_curva(route.Entries.ElementAt(counter - 1).Longitude, route.Entries.ElementAt(counter - 1).Latitude, route.Entries.ElementAt(counter).Longitude, route.Entries.ElementAt(counter).Latitude, route.Entries.ElementAt(counter + 1).Longitude, route.Entries.ElementAt(counter + 1).Latitude);
                                if (Radio == 0) { Radio = 9999999999; }

                                //Vehicle Speed
                                if (ra_inputs.CBT1 == true)
                                {
                                    double part1 = 0.342857143;
                                    double velocityC1 = Math.Sqrt(part1 * 9.8 * 0.6 * Radio);
                                    double velocidadA = Velodidadacelerada(velocidad1, distancepoint, 0.86, 0.67, 6.80, 5000 + 4000 * ra_inputs.cargo_a_b, 0.7, slopeA, elevationB, 200);
                                    List<double> velocities1 = new List<double>();
                                    velocities1.Add(velocidadA);
                                    velocities1.Add(maxvelo);
                                    velocities1.Add(velocityC1);
                                    velocidad1 = velocities1.Min();
                                    double FC1 = Consumption(velocidad1, 1, slopeA, ra_inputs.cargo_a_b);
                                    CType1 = CType1 + FC1 / (1000 * 0.83) * (distancepoint / 1000);
                                    CO2Type1 = CO2Type1 + FC1 * 3.16 * (distancepoint / 1000);
                                }
                                if (ra_inputs.CBT2 == true)
                                {
                                    double part2 = 0.333333333;
                                    double velocityC2 = Math.Sqrt(part2 * 9.8 * 0.6 * Radio);
                                    double velocidadB = Velodidadacelerada(velocidad2, distancepoint, 0.89, 0.80, 6.80, 10000 + 6000 * ra_inputs.cargo_a_b, 0.7, slopeA, elevationB, 250);
                                    List<double> velocities2 = new List<double>();
                                    velocities2.Add(velocidadB);
                                    velocities2.Add(maxvelo);
                                    velocities2.Add(velocityC2);
                                    velocidad2 = velocities2.Min();
                                    double FC2 = Consumption(velocidad2, 2, slopeA, ra_inputs.cargo_a_b);
                                    CType2 = CType2 + FC2 / (1000 * 0.83) * (distancepoint / 1000);
                                    CO2Type2 = CO2Type2 + FC2 * 3.16 * (distancepoint / 1000);
                                }
                                if (ra_inputs.CBT3 == true)
                                {
                                    double part3 = 0.328289474;
                                    double velocityC3 = Math.Sqrt(part3 * 9.8 * 0.6 * Radio);
                                    double velocidadC = Velodidadacelerada(velocidad3, distancepoint, 0.89, 0.31, 10, 12000 + 6000 * ra_inputs.cargo_a_b, 0.7, slopeA, elevationB, 330);
                                    List<double> velocities3 = new List<double>();
                                    velocities3.Add(velocidadC);
                                    velocities3.Add(maxvelo);
                                    velocities3.Add(velocityC3);
                                    velocidad3 = velocities3.Min();
                                    double FC3 = Consumption(velocidad3, 3, slopeA, ra_inputs.cargo_a_b);
                                    CType3 = CType3 + FC3 / (1000 * 0.83) * (distancepoint / 1000);
                                    CO2Type3 = CO2Type3 + FC3 * 3.16 * (distancepoint / 1000);
                                }
                                if (ra_inputs.CBT4 == true)
                                {
                                    double part4 = 0.315822785;
                                    double velocityC4 = Math.Sqrt(part4 * 9.8 * 0.6 * Radio);
                                    double velocidadD = Velodidadacelerada(velocidad4, distancepoint, 0.89, 0.435, 10.7, 20000 + 10000 * ra_inputs.cargo_a_b, 0.7, slopeA, elevationB, 500);
                                    List<double> velocities4 = new List<double>();
                                    velocities4.Add(velocidadD);
                                    velocities4.Add(maxvelo);
                                    velocities4.Add(velocityC4);
                                    velocidad4 = velocities4.Min();
                                    double FC4 = Consumption(velocidad4, 4, slopeA, ra_inputs.cargo_a_b);
                                    CType4 = CType4 + FC4 / (1000 * 0.83) * (distancepoint / 1000);
                                    CO2Type4 = CO2Type4 + FC4 * 3.16 * (distancepoint / 1000);
                                }
                                lat = item.Latitude;
                                lng = item.Longitude;
                            }
                            counter++;
                        }

                        if (RB_A_B_A.Checked == true)
                        {
                            //Trayecto vuelta
                            resolved2 = router.Resolve(Vehicle.BigTruck, 1, new OsmSharp.Math.Geo.GeoCoordinate(start.Lat, start.Lng));
                            resolved1 = router.Resolve(Vehicle.BigTruck, 1, new OsmSharp.Math.Geo.GeoCoordinate(end.Lat, end.Lng));
                            // calculate route.
                            route = router.Calculate(Vehicle.BigTruck, resolved1, resolved2);
                            counter = 0;
                            velocidad1 = 0.1;
                            velocidad2 = 0.1;
                            velocidad3 = 0.1;
                            velocidad4 = 0.1;
                            lat = 0; lng = 0;
                            maxvelo = 0.1;
                            distanceBA = 0;
                            foreach (var item in route.Entries)
                            {
                                if (counter == 0)
                                {
                                    lat = item.Latitude;
                                    lng = item.Longitude;
                                    Lat_recogida = lat;
                                    Lon_recogida = lng;
                                }
                                if (counter == 1)
                                {
                                    //Distance
                                    double distancepoint = Math.Sqrt(Math.Pow(lat - item.Latitude, 2) + Math.Pow(lng - item.Longitude, 2)) * 100000;
                                    distanceBA = distanceBA + distancepoint;
                                    //Elevation
                                    double elevationB;
                                    double elevationA;
                                    int xid = Convert.ToInt32((lng - ra_inputs.lblXmin) / ra_inputs.lblCsize);
                                    int yid = Convert.ToInt32((ra_inputs.lblYmax - lat) / ra_inputs.lblCsize);
                                    elevationA = elevationMT[xid, yid];
                                    int xid2 = Convert.ToInt32((item.Longitude - ra_inputs.lblXmin) / ra_inputs.lblCsize);
                                    int yid2 = Convert.ToInt32((ra_inputs.lblYmax - item.Latitude) / ra_inputs.lblCsize);
                                    elevationB = elevationMT[xid2, yid2];
                                    //Slope
                                    double slopeA = (elevationB - elevationA) / distancepoint;
                                    //Max legal speed
                                    foreach (var item2 in item.Tags)
                                    {
                                        if (item2.Key == "highway")
                                        {
                                            maxvelo = maxVel(item2.Value, 1);
                                            maxvelo2 = maxVel(item2.Value, 0);
                                            Type_road = item2.Value;
                                        }
                                    }
                                    foreach (var item2 in item.Tags)
                                    {
                                        if (item2.Key == "maxspeed")
                                        {
                                            maxvelo = Convert.ToDouble(item2.Value);
                                            maxvelo2 = maxvelo;
                                        }
                                    }
                                    //Vehicle speed
                                    if (ra_inputs.CBT1 == true)
                                    {
                                        double velocidadA = Velodidadacelerada(velocidad1, distancepoint, 0.86, 0.67, 6.80, 5000 + 4000 * ra_inputs.cargo_b_a, 0.7, slopeA, elevationB, 200);
                                        velocidad1 = Math.Min(velocidadA, maxvelo);
                                        double FC1 = Consumption(velocidad1, 1, slopeA, ra_inputs.cargo_b_a);
                                        CType1 = CType1 + FC1 / (1000 * 0.83) * (distancepoint / 1000);
                                        CO2Type1 = CO2Type1 + FC1 * 3.16 * (distancepoint / 1000);
                                    }
                                    if (ra_inputs.CBT2 == true)
                                    {
                                        double velocidadB = Velodidadacelerada(velocidad2, distancepoint, 0.89, 0.80, 6.80, 10000 + 6000 * ra_inputs.cargo_b_a, 0.7, slopeA, elevationB, 250);
                                        velocidad2 = Math.Min(velocidadB, maxvelo);
                                        double FC2 = Consumption(velocidad2, 2, slopeA, ra_inputs.cargo_b_a);
                                        CType2 = CType2 + FC2 / (1000 * 0.83) * (distancepoint / 1000);
                                        CO2Type2 = CO2Type2 + FC2 * 3.16 * (distancepoint / 1000);
                                    }
                                    if (ra_inputs.CBT3 == true)
                                    {
                                        double velocidadC = Velodidadacelerada(velocidad3, distancepoint, 0.89, 0.31, 10, 12000 + 6000 * ra_inputs.cargo_b_a, 0.7, slopeA, elevationB, 330);
                                        velocidad3 = Math.Min(velocidadC, maxvelo);
                                        double FC3 = Consumption(velocidad3, 3, slopeA, ra_inputs.cargo_b_a);
                                        CType3 = CType3 + FC3 / (1000 * 0.83) * (distancepoint / 1000);
                                        CO2Type3 = CO2Type3 + FC3 * 3.16 * (distancepoint / 1000);
                                    }
                                    if (ra_inputs.CBT4 == true)
                                    {
                                        double velocidadD = Velodidadacelerada(velocidad4, distancepoint, 0.89, 0.435, 10.7, 20000 + 10000 * ra_inputs.cargo_b_a, 0.7, slopeA, elevationB, 500);
                                        velocidad4 = Math.Min(velocidadD, maxvelo);
                                        double FC4 = Consumption(velocidad4, 4, slopeA, ra_inputs.cargo_b_a);
                                        CType4 = CType4 + FC4 / (1000 * 0.83) * (distancepoint / 1000);
                                        CO2Type4 = CO2Type4 + FC4 * 3.16 * (distancepoint / 1000);
                                    }
                                    lat = item.Latitude;
                                    lng = item.Longitude;
                                }
                                if (counter > 1 && counter < route.Entries.Count() - 2)
                                {
                                    //Distance
                                    double distancepoint = Math.Sqrt(Math.Pow(lat - item.Latitude, 2) + Math.Pow(lng - item.Longitude, 2)) * 100000;
                                    distanceBA = distanceBA + distancepoint;
                                    ////curvature
                                    double curvaTure = Radio_curva(route.Entries.ElementAt(counter - 1).Longitude, route.Entries.ElementAt(counter - 1).Latitude, route.Entries.ElementAt(counter).Longitude, route.Entries.ElementAt(counter).Latitude, route.Entries.ElementAt(counter + 1).Longitude, route.Entries.ElementAt(counter + 1).Latitude);
                                    if (curvaTure == 0) { curvaTure = 9999999999; }
                                    //Max legal speed
                                    foreach (var item2 in item.Tags)
                                    {
                                        if (item2.Key == "highway")
                                        {
                                            maxvelo = maxVel(item2.Value, 1);
                                            maxvelo2 = maxVel(item2.Value, 0);
                                        }
                                    }
                                    foreach (var item2 in item.Tags)
                                    {
                                        if (item2.Key == "maxspeed")
                                        {
                                            maxvelo = Convert.ToDouble(item2.Value);
                                            maxvelo2 = maxvelo;
                                        }
                                    }
                                    //Elevation
                                    double elevationB;
                                    double elevationA;
                                    int xid = Convert.ToInt32((lng - ra_inputs.lblXmin) / ra_inputs.lblCsize);
                                    int yid = Convert.ToInt32((ra_inputs.lblYmax - lat) / ra_inputs.lblCsize);
                                    elevationA = elevationMT[xid, yid];
                                    int xid2 = Convert.ToInt32((item.Longitude - ra_inputs.lblXmin) / ra_inputs.lblCsize);
                                    int yid2 = Convert.ToInt32((ra_inputs.lblYmax - item.Latitude) / ra_inputs.lblCsize);
                                    elevationB = elevationMT[xid2, yid2];
                                    //Slope
                                    double slopeA = (elevationB - elevationA) / distancepoint;
                                    //Vehicle speed
                                    if (ra_inputs.CBT1 == true)
                                    {
                                        double part1 = 0.342857143;

                                        double velocityC1 = Math.Sqrt(part1 * 9.8 * 0.6 * curvaTure);
                                        double velocidadA = Velodidadacelerada(velocidad1, distancepoint, 0.86, 0.67, 6.80, 5000 + 4000 * ra_inputs.cargo_b_a, 0.7, slopeA, elevationB, 200);
                                        List<double> velocities1 = new List<double>();
                                        velocities1.Add(velocidadA);
                                        velocities1.Add(maxvelo);
                                        velocities1.Add(velocityC1);
                                        velocidad1 = velocities1.Min();
                                        double FC1 = Consumption(velocidad1, 1, slopeA, ra_inputs.cargo_b_a);
                                        CType1 = CType1 + FC1 / (1000 * 0.83) * (distancepoint / 1000);
                                        CO2Type1 = CO2Type1 + FC1 * 3.16 * (distancepoint / 1000);

                                    }
                                    if (ra_inputs.CBT2 == true)
                                    {
                                        double part2 = 0.333333333;

                                        double velocityC2 = Math.Sqrt(part2 * 9.8 * 0.6 * curvaTure);
                                        double velocidadB = Velodidadacelerada(velocidad2, distancepoint, 0.89, 0.80, 6.80, 10000 + 6000 * ra_inputs.cargo_b_a, 0.7, slopeA, elevationB, 250);
                                        List<double> velocities2 = new List<double>();
                                        velocities2.Add(velocidadB);
                                        velocities2.Add(maxvelo);
                                        velocities2.Add(velocityC2);
                                        velocidad2 = velocities2.Min();
                                        double FC2 = Consumption(velocidad2, 2, slopeA, ra_inputs.cargo_b_a);
                                        CType2 = CType2 + FC2 / (1000 * 0.83) * (distancepoint / 1000);
                                        CO2Type2 = CO2Type2 + FC2 * 3.16 * (distancepoint / 1000);
                                    }
                                    if (ra_inputs.CBT3 == true)
                                    {
                                        double part3 = 0.328289474;

                                        double velocityC3 = Math.Sqrt(part3 * 9.8 * 0.6 * curvaTure);
                                        double velocidadC = Velodidadacelerada(velocidad3, distancepoint, 0.89, 0.31, 10, 12000 + 6000 * ra_inputs.cargo_b_a, 0.7, slopeA, elevationB, 330);
                                        List<double> velocities3 = new List<double>();
                                        velocities3.Add(velocidadC);
                                        velocities3.Add(maxvelo);
                                        velocities3.Add(velocityC3);
                                        velocidad3 = velocities3.Min();
                                        double FC3 = Consumption(velocidad3, 3, slopeA, ra_inputs.cargo_b_a);
                                        CType3 = CType3 + FC3 / (1000 * 0.83) * (distancepoint / 1000);
                                        CO2Type3 = CO2Type3 + FC3 * 3.16 * (distancepoint / 1000);
                                    }
                                    if (ra_inputs.CBT4 == true)
                                    {
                                        double part4 = 0.315822785;
                                        double velocityC4 = Math.Sqrt(part4 * 9.8 * 0.6 * curvaTure);
                                        double velocidadD = Velodidadacelerada(velocidad4, distancepoint, 0.89, 0.435, 10.7, 20000 + 10000 * ra_inputs.cargo_b_a, 0.7, slopeA, elevationB, 500);
                                        List<double> velocities4 = new List<double>();
                                        velocities4.Add(velocidadD);
                                        velocities4.Add(maxvelo);
                                        velocities4.Add(velocityC4);
                                        velocidad4 = velocities4.Min();
                                        double FC4 = Consumption(velocidad4, 4, slopeA, ra_inputs.cargo_b_a);
                                        CType4 = CType4 + FC4 / (1000 * 0.83) * (distancepoint / 1000);
                                        CO2Type4 = CO2Type4 + FC4 * 3.16 * (distancepoint / 1000);
                                    }
                                    lat = item.Latitude;
                                    lng = item.Longitude;
                                }
                                counter++;
                            }
                        }

                        Lista_points_reg.Add(new PointLatLng(end.Lat, end.Lng));
                        BK_regional_assessment.ReportProgress(contador_ext);
                        contador_ext++;

                        DataRow row = data_set.Tables["data"].NewRow();
                        row["lat"] = end.Lat;
                        row["long"] = end.Lng;
                        row["Dist A-B"] = distanceAB;
                        row["Dist B-A"] = distanceBA;
                        row["Dist beeline"] = Math.Round(Math.Sqrt(Math.Pow((end.Lng - start.Lng), 2) + Math.Pow((end.Lat - start.Lat), 2)) * 100, 3);
                        row["FC T1"] = CType1;
                        row["CO2 T1"] = CO2Type1;
                        row["FC T2"] = CType2;
                        row["CO2 T2"] = CO2Type2;
                        row["FC T3"] = CType3;
                        row["CO2 T3"] = CO2Type3;
                        row["FC T4"] = CType4;
                        row["CO2 T4"] = CO2Type4;
                        data_set.Tables["data"].Rows.Add(row);
                        a = new PointLatLng();
                        a.Lat = end_lat;
                        a.Lng = end_lon;
                    }
                    catch
                    { }
                }
            }

        }

        private void BK_region_assessment_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            overlayOne.Markers.Add(new GMap.NET.WindowsForms.Markers.GMapMarkerCross(Lista_points_reg.ElementAt(e.ProgressPercentage)));
        }

        private void BK_region_assessment_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DataView dv = new DataView(data_set.Tables["data"], "", "", DataViewRowState.CurrentRows);
            DTV_analysis.DataSource = dv;
            Timer.Stop();
            ProgressBar_1.Visible = false;
        }

        private void Load_OSM_Data_DoWork(object sender, DoWorkEventArgs e)
        {
            router = Router.CreateLiveFrom(new PBFOsmStreamSource(new FileInfo("portugal-latest.osm.pbf").OpenRead()), new OsmRoutingInterpreter());
        }

        private void Load_OSM_Data_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            LBL_OSM_DATA_LOAD.Visible = false;
            Timer.Stop();
            PB_Inic.Visible = false;
        }



        private void MainMap_mousedown(object sender, MouseEventArgs e)
        {

            if (set_region == true)
            {

                points_lat.Add(MainMap.FromLocalToLatLng(e.X, e.Y).Lat);
                points_long.Add(MainMap.FromLocalToLatLng(e.X, e.Y).Lng);
            }
            else
            {
                if (e.Button.ToString().Equals(MouseButtons.Left.ToString()))
                {
                    flag_move = true;
                    MainMap.Cursor = Cursors.Hand;
                    move_x = e.X;
                    move_y = e.Y;
                }
            }
        }

        private void MainMap_mouseup(object sender, MouseEventArgs e)
        {
            if (set_region == true && points_lat.Count > 0)
            {
                points_lat.Add(MainMap.FromLocalToLatLng(e.X, e.Y).Lat);
                points_long.Add(MainMap.FromLocalToLatLng(e.X, e.Y).Lng);
                set_region = false;
            }
            flag_move = false;
            MainMap.Cursor = Cursors.Arrow;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (Load_OSM_Data.IsBusy == true)
            {
                switch (time)
                {
                    case 1:
                        LBL_OSM_DATA_LOAD.Text = "Loading OSM data in memory";
                        time = 2;
                        break;
                    case 2:
                        LBL_OSM_DATA_LOAD.Text = "Loading OSM data in memory.";
                        time = 3;
                        break;
                    case 3:
                        LBL_OSM_DATA_LOAD.Text = "Loading OSM data in memory..";
                        time = 4;
                        break;
                    case 4:
                        LBL_OSM_DATA_LOAD.Text = "Loading OSM data in memory...";
                        time = 1;
                        break;
                }
            }
            if (BK_regional_assessment.IsBusy == true)
            {
                switch (time)
                {
                    case 1:
                        time = 2;
                        break;
                    case 2:
                        time = 3;
                        break;
                    case 3:
                        time = 4;
                        break;
                    case 4:
                        time = 1;
                        break;
                }

            }
        }

        private void BK_route_assessment_DoWork(object sender, DoWorkEventArgs e)
        {
            double FCt = 0;
            double CO2 = 0;
            poligones = new List<GMapPolygon>();
            Output_graph = new List<output_graph>();
            try
            {
                var Start_point = router.Resolve(Vehicle.BigTruck, 1, new GeoCoordinate(start.Lat, start.Lng));
                var Finish_point = router.Resolve(Vehicle.BigTruck, 1, new GeoCoordinate(finish.Lat, finish.Lng));
                var route = router.Calculate(Vehicle.BigTruck, Start_point, Finish_point);
                int counter = 0;
                double velocidad0 = 0.1;
                double lat = 0, lng = 0;
                double maxvelo = 0.1;
                double distance = 0;
                int vel = 0;
                foreach (var item in route.Entries)
                {
                    if (counter == 0)
                    {
                        lat = item.Latitude;
                        lng = item.Longitude;
                        velocidad0 = 0.1;
                    }
                    if (counter == 1)
                    {
                        double distancepoint = Math.Sqrt(Math.Pow(lat - item.Latitude, 2) + Math.Pow(lng - item.Longitude, 2)) * 100000;
                        distance = distance + distancepoint;
                        double elevationB;
                        double elevationA;
                        elevationA = elevation(lng, lat, or_inputs.lblXmax, or_inputs.lblXmin, or_inputs.lblYmax, or_inputs.lblYmin, or_inputs.lblCsize, or_inputs.lblRoute);
                        elevationB = elevation(item.Longitude, item.Latitude, or_inputs.lblXmax, or_inputs.lblXmin, or_inputs.lblYmax, or_inputs.lblYmin, or_inputs.lblCsize, or_inputs.lblRoute);
                        double slopeA = (elevationB - elevationA) / distancepoint;
                        if (or_inputs.tType == 1)
                        {
                            double velocidadA = Velodidadacelerada(velocidad0, distancepoint, 0.86, 0.67, 6.80, 5000 + 4000 * or_inputs.cargo, 0.7, slopeA, elevationB, 200);
                            if (velocidad0 > Math.Min(velocidadA, maxvelo))
                            {
                                vel = 0;
                            }
                            else if (velocidad0 > Math.Min(velocidadA, maxvelo))
                            {
                                vel = 2;
                            }
                            else
                            {
                                vel = 1;
                            }
                            velocidad0 = Math.Min(velocidadA, maxvelo);

                            double FC1 = Consumption(velocidad0, 1, slopeA, or_inputs.cargo);
                            FCt = FCt + FC1 / (1000 * 0.83) * (distancepoint / 1000);
                            CO2 = CO2 + (FC1 * 3.16 * (distancepoint / 1000)/1000);
                        }
                        if (or_inputs.tType == 2)
                        {
                            double velocidadB = Velodidadacelerada(velocidad0, distancepoint, 0.89, 0.80, 6.80, 10000 + 6000 * or_inputs.cargo, 0.7, slopeA, elevationB, 250);
                            if (velocidad0 > Math.Min(velocidadB, maxvelo))
                            {
                                vel = 0;
                            }
                            else if (velocidad0 > Math.Min(velocidadB, maxvelo))
                            {
                                vel = 2;
                            }
                            else
                            {
                                vel = 1;
                            }
                            velocidad0 = Math.Min(velocidadB, maxvelo);
                            double FC2 = Consumption(velocidad0, 2, slopeA, or_inputs.cargo);
                            FCt = FCt + FC2 / (1000 * 0.83) * (distancepoint / 1000);
                            CO2 = CO2 + (FC2 / 3.17 * (distancepoint / 1000)/1000);
                        }
                        if (or_inputs.tType == 3)
                        {
                            double velocidadC = Velodidadacelerada(velocidad0, distancepoint, 0.89, 0.31, 10, 12000 + 6000 * or_inputs.cargo, 0.7, slopeA, elevationB, 330);
                            if (velocidad0 > Math.Min(velocidadC, maxvelo))
                            {
                                vel = 0;
                            }
                            else if (velocidad0 > Math.Min(velocidadC, maxvelo))
                            {
                                vel = 2;
                            }
                            else
                            {
                                vel = 1;
                            }
                            velocidad0 = Math.Min(velocidadC, maxvelo);
                            double FC3 = Consumption(velocidad0, 3, slopeA, or_inputs.cargo);
                            FCt = FCt + FC3 / (1000 * 0.83) * (distancepoint / 1000);
                            CO2 = CO2 + (FC3 * 3.16 * (distancepoint / 1000)/1000);
                        }
                        if (or_inputs.tType == 4)
                        {
                            double velocidadD = Velodidadacelerada(velocidad0, distancepoint, 0.89, 0.435, 10.7, 20000 + 10000 * or_inputs.cargo, 0.7, slopeA, elevationB, 500);
                            if (velocidad0 > Math.Min(velocidadD, maxvelo))
                            {
                                vel = 0;
                            }
                            else if (velocidad0 > Math.Min(velocidadD, maxvelo))
                            {
                                vel = 2;
                            }
                            else
                            {
                                vel = 1;
                            }
                            velocidad0 = Math.Min(velocidadD, maxvelo);
                            double FC4 = Consumption(velocidad0, 4, slopeA, or_inputs.cargo);
                            FCt = FCt + FC4 / (1000 * 0.83) * (distancepoint / 1000);
                            CO2 = CO2 + (FC4 * 3.16 * (distancepoint / 1000)/1000);
                        }
                        foreach (var item2 in item.Tags)
                        {
                            if (item2.Key == "highway") { maxvelo = maxVel(item2.Value, 1); }
                        }
                        foreach (var item2 in item.Tags)
                        {
                            if (item2.Key == "maxspeed")
                            {
                                maxvelo = Convert.ToDouble(item2.Value);
                            }
                        }
                        lat = item.Latitude;
                        lng = item.Longitude;
                    }
                    if (counter > 1 && counter < route.Entries.Count() - 2)
                    {
                        double distancepoint = Math.Sqrt(Math.Pow(lat - item.Latitude, 2) + Math.Pow(lng - item.Longitude, 2)) * 100000;
                        distance = distance + distancepoint;
                        double curvaTure = Radio_curva(route.Entries.ElementAt(counter - 1).Longitude, route.Entries.ElementAt(counter - 1).Latitude, route.Entries.ElementAt(counter).Longitude, route.Entries.ElementAt(counter).Latitude, route.Entries.ElementAt(counter + 1).Longitude, route.Entries.ElementAt(counter + 1).Latitude);
                        if (curvaTure == 0) { curvaTure = 9999999999; }
                        foreach (var item2 in item.Tags)
                        {
                            maxvelo = maxVel(item2.Value, 1);
                        }
                        foreach (var item2 in item.Tags)
                        {
                            if (item2.Key == "maxspeed")
                            {
                                if (maxvelo > Convert.ToDouble(item2.Value))
                                {
                                    maxvelo = Convert.ToDouble(item2.Value);
                                }
                            }
                        }
                        double elevationB;
                        double elevationA;
                        elevationA = elevation(lng, lat, or_inputs.lblXmax, or_inputs.lblXmin, or_inputs.lblYmax, or_inputs.lblYmin, or_inputs.lblCsize, or_inputs.lblRoute);
                        elevationB = elevation(item.Longitude, item.Latitude, or_inputs.lblXmax, or_inputs.lblXmin, or_inputs.lblYmax, or_inputs.lblYmin, or_inputs.lblCsize, or_inputs.lblRoute);
                        double elevationAvg = (elevationA + elevationB) / 2;
                        if (elevationAvg < 0)
                        {
                            elevationAvg = 0;
                        }
                        //chart1.Series.ElementAt(0).Points.AddXY(distance / 1000, elevationB);
                        double slopeA = (elevationB - elevationA) / distancepoint;
                        List<double> velocities = new List<double>();
                        if (or_inputs.tType == 1)
                        {
                            double part1 = 0.342857143 * 2;
                            double velocityC1 = Math.Sqrt(part1 * 9.8 * 0.7 * curvaTure);
                            double velocidadA = Velodidadacelerada(velocidad0, distancepoint, 0.86, 0.67, 6.80, 5000 + 4000 * or_inputs.cargo, 0.7, slopeA, elevationB, 200);
                            velocities.Add(velocidadA);
                            velocities.Add(maxvelo);
                            velocities.Add(velocityC1);
                            if (velocidad0 > velocities.Min())
                            {
                                vel = 0;
                            }
                            else if (velocidad0 == velocities.Min())
                            {
                                vel = 2;
                            }
                            else
                            {
                                vel = 1;
                            }
                            velocidad0 = velocities.Min();
                            double FC1 = Consumption(velocidad0, 1, slopeA, or_inputs.cargo);
                            FCt = FCt + FC1 / (1000 * 0.83) * (distancepoint / 1000);
                            CO2 = CO2 + (FC1 * 3.16 * (distancepoint / 1000)/1000);

                        }
                        if (or_inputs.tType == 2)
                        {
                            double part2 = 0.333333333 * 2;
                            double velocityC2 = Math.Sqrt(part2 * 9.8 * 0.7 * curvaTure);
                            double velocidadB = Velodidadacelerada(velocidad0, distancepoint, 0.89, 0.80, 6.80, 10000 + 6000 * or_inputs.cargo, 0.7, slopeA, elevationB, 250);
                            velocities.Add(velocidadB);
                            velocities.Add(maxvelo);
                            velocities.Add(velocityC2);
                            if (velocidad0 > velocities.Min())
                            {
                                vel = 0;
                            }
                            else if (velocidad0 == velocities.Min())
                            {
                                vel = 2;
                            }
                            else
                            {
                                vel = 1;
                            }
                            velocidad0 = velocities.Min();
                            double FC2 = Consumption(velocidad0, 2, slopeA, or_inputs.cargo);
                            FCt = FCt + FC2 / (1000 * 0.83) * (distancepoint / 1000);
                            CO2 = CO2 + (FC2 * 3.16 * (distancepoint / 1000)/1000);
                        }
                        if (or_inputs.tType == 3)
                        {
                            double part3 = 0.328289474 * 2;
                            double velocityC3 = Math.Sqrt(part3 * 9.8 * 0.7 * curvaTure);
                            double velocidadC = Velodidadacelerada(velocidad0, distancepoint, 0.89, 0.31, 10, 12000 + 6000 * or_inputs.cargo, 0.7, slopeA, elevationB, 330);
                            velocities.Add(velocidadC);
                            velocities.Add(maxvelo);
                            velocities.Add(velocityC3);
                            if (velocidad0 > velocities.Min())
                            {
                                vel = 0;
                            }
                            else if (velocidad0 == velocities.Min())
                            {
                                vel = 2;
                            }
                            else
                            {
                                vel = 1;
                            }
                            velocidad0 = velocities.Min();
                            double FC3 = Consumption(velocidad0, 3, slopeA, or_inputs.cargo);
                            FCt = FCt + FC3 / (1000 * 0.83) * (distancepoint / 1000);
                            CO2 = CO2 + (FC3 * 3.16 * (distancepoint / 1000)/1000);
                        }
                        if (or_inputs.tType == 4)
                        {
                            double part4 = 0.315822785 * 2;
                            double velocityC4 = Math.Sqrt(part4 * 9.8 * 0.7 * curvaTure);
                            double velocidadD = Velodidadacelerada(velocidad0, distancepoint, 0.89, 0.435, 10.7, 20000 + 10000 * or_inputs.cargo, 0.7, slopeA, elevationB, 500);
                            velocities.Add(velocidadD);
                            velocities.Add(maxvelo);
                            velocities.Add(velocityC4);
                            if (velocidad0 > velocities.Min())
                            {
                                vel = 0;
                            }
                            else if (velocidad0 == velocities.Min())
                            {
                                vel = 2;
                            }
                            else
                            {
                                vel = 1;
                            }
                            velocidad0 = velocities.Min();
                            double FC4 = Consumption(velocidad0, 4, slopeA, or_inputs.cargo);
                            FCt = FCt + FC4 / (1000 * 0.83) * (distancepoint / 1000);
                            CO2 = CO2 + (FC4 * 3.16 * (distancepoint / 1000)/1000);
                        }
                        List<PointLatLng> poligone = new List<PointLatLng>();
                        PointLatLng point = new PointLatLng();
                        point.Lat = item.Latitude;
                        point.Lng = item.Longitude;
                        poligone.Add(point);
                        point = new PointLatLng();
                        point.Lat = lat;
                        point.Lng = lng;
                        poligone.Add(point);
                        GMapPolygon rst = new GMapPolygon(poligone, "id" + counter);
                        rst.Stroke.Width = 4;
                        if (vel == 1)
                        {
                            rst.Stroke.Color = Color.Green;
                        }
                        else if (vel == 2)
                        {
                            rst.Stroke.Color = Color.Blue;
                        }
                        else
                        {
                            rst.Stroke.Color = Color.Red;
                        }
                        poligones.Add(rst);
                        output_graph out_graph_ = new output_graph();
                        out_graph_.distance = distance / 1000;
                        out_graph_.elevation = elevationAvg;
                        out_graph_.Fct = FCt;
                        out_graph_.velocity = velocidad0;
                        out_graph_.EmCO2 = CO2;
                        Output_graph.Add(out_graph_);
                        BK_route_assessment.ReportProgress(counter - 2);
                        //routesOverlay3.Polygons.Add(rst);
                        lat = item.Latitude;
                        lng = item.Longitude;
                    }
                    counter++;
                }
                List<GeoCoordinate> points = route.GetPoints();
                List<PointLatLng> latlng = new List<PointLatLng>();
                foreach (var item in points)
                {
                    PointLatLng point = new PointLatLng(item.Latitude, item.Longitude);
                    latlng.Add(point);
                }
            }
            catch
            {

            }
        }

        private void BK_route_assessment_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            routesOverlay3.Polygons.Add(poligones.ElementAt(e.ProgressPercentage));
            El_Chart.Series.ElementAt(0).Points.AddXY(Output_graph.ElementAt(e.ProgressPercentage).distance, Output_graph.ElementAt(e.ProgressPercentage).elevation);
            FC_Chart.Series.ElementAt(0).Points.AddXY(Output_graph.ElementAt(e.ProgressPercentage).distance, Output_graph.ElementAt(e.ProgressPercentage).Fct);
            VE_Chart.Series.ElementAt(0).Points.AddXY(Output_graph.ElementAt(e.ProgressPercentage).distance, Output_graph.ElementAt(e.ProgressPercentage).velocity);
            EM_Chart.Series.ElementAt(0).Points.AddXY(Output_graph.ElementAt(e.ProgressPercentage).distance, Output_graph.ElementAt(e.ProgressPercentage).EmCO2);
        }

        private void TSM_clear_points_Click(object sender, EventArgs e)
        {
            clear_tool();
        }

        private void clear_tool()
        {
            routesOverlay3.Polygons.Clear();
            overlayOne.Markers.Clear();
            start = new PointLatLng();
            finish = new PointLatLng();
            TSM_position_f.Checked = false;
            TSM_position_i.Checked = false;
            BK_route_assessment.CancelAsync();
            El_Chart.Series.ElementAt(0).Points.Clear();
            FC_Chart.Series.ElementAt(0).Points.Clear();
            VE_Chart.Series.ElementAt(0).Points.Clear();
            EM_Chart.Series.ElementAt(0).Points.Clear();
        }

        private void TSM_Load_mdt_Click(object sender, EventArgs e)
        {
            Load_Mdt(1);
        }

        private void TSM_one_route_Click(object sender, EventArgs e)
        {
            clear_tool();
            SP_routes_analysis.Panel1Collapsed = false;
            SP_routes_analysis.Panel2Collapsed = true;
            SP_routes_analysis.Visible = true;
        }

        private void TSM_regional_Click(object sender, EventArgs e)
        {
            clear_tool();
            SP_routes_analysis.Panel1Collapsed = true;
            SP_routes_analysis.Panel2Collapsed = false;
            SP_routes_analysis.Visible = true;
        }

        private void BTN_close_R_setup_Click(object sender, EventArgs e)
        {
            SP_routes_analysis.Visible = false;
        }

        private void BTN_Close_Region_Setup_Click(object sender, EventArgs e)
        {
            SP_routes_analysis.Visible = false;
        }

        private void BTN_collapse_analysis_Click(object sender, EventArgs e)
        {
            if (SP_Main.Panel2Collapsed == true)
            {
                SP_Main.Panel2Collapsed = false;
                BTN_collapse_analysis.Text = "˅˅";
            }
            else
            {
                SP_Main.Panel2Collapsed = true;
                BTN_collapse_analysis.Text = "˄˄";
            }
        }

        private void TSM_Set_Region_Click(object sender, EventArgs e)
        {
            points_lat = new List<double>();
            points_long = new List<double>();
            set_region = true;
        }

        private void TSM_About_Click(object sender, EventArgs e)
        {
            MessageBox.Show("WRoute 1.0 was developed by Fernando Perez-Rodriguez, Luis Nunes, Angelo Sil and João Azevedo. All rights reserved", "About", MessageBoxButtons.OK);
        }
    }
}
