using PZ2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace PZ2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<SubstationEntity> listSubstations = new List<SubstationEntity>(); 
        private List<NodeEntity> listNodes = new List<NodeEntity>(); 
        private List<SwitchEntity> listSwitches = new List<SwitchEntity>(); 
        private List<LineEntity> listLines = new List<LineEntity>();
        private List<PZ2.Models.Point> listPoint = new List<PZ2.Models.Point>(); 
        private List<Model3D> listaSvihVodovaZaBrisanje = new List<Model3D>();
        private List<Model3D> prviNodoviZaBrisanjeLista = new List<Model3D>();
        private List<Model3D> drugiNodoviZaBrisanjeLista = new List<Model3D>();
        private List<GeometryModel3D> switchClosedList = new List<GeometryModel3D>();
        private List<GeometryModel3D> switchOpenList = new List<GeometryModel3D>();
        private List<GeometryModel3D> vodoviIspod1List = new List<GeometryModel3D>();
        private List<GeometryModel3D> vodoviIzmedju1I2List = new List<GeometryModel3D>();
        private List<GeometryModel3D> vodoviIznad2List = new List<GeometryModel3D>();
        private List<GeometryModel3D> sakrijVodoveCelik = new List<GeometryModel3D>();
        private List<GeometryModel3D> sakrijVodoveAluminijum = new List<GeometryModel3D>();
        private List<GeometryModel3D> sakrijVodoveBakar = new List<GeometryModel3D>();
        private List<GeometryModel3D> endList = new List<GeometryModel3D>();

        private System.Windows.Point original = new System.Windows.Point();
        private System.Windows.Point pocetnaTackaRotacije = new System.Windows.Point();
        private System.Windows.Point pocetnaTacka = new System.Windows.Point();

        private ToolTip opis = new ToolTip() { IsOpen = true };

        public Int32Collection idk = new Int32Collection() { 2, 3, 1, 2, 1, 0, 7, 1, 3, 7, 5, 1, 6, 5, 7, 6, 4, 5, 6, 2, 4, 2, 0, 4, 2, 7, 3, 2, 6, 7, 0, 1, 5, 0, 5, 4 }; //vezbe

        //donji levi ugao
        public static readonly double donjiLeviLon = 19.793909; //minlon
        public static readonly double donjiLeviLat = 45.2325; //minlat

        //gornji desni ugao
        public static readonly double gornjiDesniLon = 19.894459; //maxlon
        public static readonly double gornjiDesniLat = 45.277031;  //minlat

        public static readonly double sirinaIVisinaLinije = 0.0015;
        public static readonly double velicinaKocke = 0.008;

        public static int zoomMax = 50;
        public static int zoomTrenutni = 1;
        public static int zoomMin = -100;

        public bool sakrijNeaktivne = false;
        public bool promeniSwitch = false;
        public bool promeniVod = false;

        public bool sakrijCelik = false;
        public bool sakrijAluminijum = false;
        public bool sakrijBakar = false;

        public GeometryModel3D prviNode;
        public GeometryModel3D drugiNode;

        public MainWindow()
        {

            InitializeComponent();
            LoadXmls();
            DrawSubstations();
            DrawNodes();
            DrawSwitches();
            DrawLines();
            this.SakrijNeaktivan.Background = new SolidColorBrush(Colors.LightGray);
            this.PromeniSwitch.Background = new SolidColorBrush(Colors.LightGray);
            this.PromeniVod.Background = new SolidColorBrush(Colors.LightGray);
            this.SakrijVodove.Background = new SolidColorBrush(Colors.LightGray);
            this.SakrijCelik.Background = new SolidColorBrush(Colors.LightGray);
            this.SakrijAluminijum.Background = new SolidColorBrush(Colors.LightGray);
            this.SakrijBakar.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void LoadXmls()  //provera da se svi objekti stave u tacno dimenzije mape
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Geographic.xml");

            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");

            foreach (XmlNode node in nodeList)
            {
                SubstationEntity subEntity = new SubstationEntity();
                subEntity.id = long.Parse(node.SelectSingleNode("Id").InnerText);
                subEntity.name = node.SelectSingleNode("Name").InnerText;
                subEntity.x = double.Parse(node.SelectSingleNode("X").InnerText);
                subEntity.y = double.Parse(node.SelectSingleNode("Y").InnerText);
                subEntity.tooltip = "Substation\nID: " + subEntity.id + "  Name: " + subEntity.name;

                ToLatLon(subEntity.x, subEntity.y, 34, out double newX, out double newY);

                subEntity.x = newX;
                subEntity.y = newY;
                //ako objekat, njegove konvertovane koordinate padaju unutar geografskog podrucja, tada se
                //dodaju u listu
                if (newX >= donjiLeviLat && newX <= gornjiDesniLat && newY >= donjiLeviLon && newY <= gornjiDesniLon)
                {
                    listPoint.Add(new PZ2.Models.Point(newY, newY));

                    listSubstations.Add(subEntity);
                }
            }

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");

            foreach (XmlNode node in nodeList)
            {
                NodeEntity nodeEntity = new NodeEntity();
                nodeEntity.id = long.Parse(node.SelectSingleNode("Id").InnerText);
                nodeEntity.name = node.SelectSingleNode("Name").InnerText;
                nodeEntity.x = double.Parse(node.SelectSingleNode("X").InnerText);
                nodeEntity.y = double.Parse(node.SelectSingleNode("Y").InnerText);
                nodeEntity.tooltip = "Node\nID: " + nodeEntity.id + "  Name: " + nodeEntity.name;

                ToLatLon(nodeEntity.x, nodeEntity.y, 34, out double newX, out double newY);

                nodeEntity.x = newX;
                nodeEntity.y = newY;

                if (newX >= donjiLeviLat && newX <= gornjiDesniLat && newY >= donjiLeviLon && newY <= gornjiDesniLon)
                {
                    listPoint.Add(new PZ2.Models.Point(newY, newY));

                    listNodes.Add(nodeEntity);
                }
            }

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");

            foreach (XmlNode node in nodeList)
            {
                SwitchEntity switchEntity = new SwitchEntity();
                switchEntity.id = long.Parse(node.SelectSingleNode("Id").InnerText);
                switchEntity.name = node.SelectSingleNode("Name").InnerText;
                switchEntity.status = node.SelectSingleNode("Status").InnerText;
                switchEntity.x = double.Parse(node.SelectSingleNode("X").InnerText);
                switchEntity.y = double.Parse(node.SelectSingleNode("Y").InnerText);
                switchEntity.tooltip = "Switch\nID: " + switchEntity.id + "  Name: " + switchEntity.name;

                ToLatLon(switchEntity.x, switchEntity.y, 34, out double noviX, out double noviY);

                switchEntity.x = noviX;
                switchEntity.y = noviY;
                //ako objekat, njegove konvertovane koordinate padaju unutar geografskog podrucja, tada se
                //dodaju u listu
                if (noviX >= donjiLeviLat && noviX <= gornjiDesniLat && noviY >= donjiLeviLon && noviY <= gornjiDesniLon)
                {
                    listPoint.Add(new PZ2.Models.Point(noviY, noviY));

                    listSwitches.Add(switchEntity);
                }
            }

            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
            foreach (XmlNode node in nodeList)
            {
                LineEntity lineEntity = new LineEntity();
                lineEntity.id = long.Parse(node.SelectSingleNode("Id").InnerText);
                lineEntity.name = node.SelectSingleNode("Name").InnerText;
                lineEntity.isUnderground = Convert.ToBoolean(node["IsUnderground"].InnerText); ;
                lineEntity.r = float.Parse(node.SelectSingleNode("R").InnerText);
                lineEntity.conductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
                lineEntity.lineType = node.SelectSingleNode("LineType").InnerText;
                lineEntity.thermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
                lineEntity.firstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
                lineEntity.secondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);

                XmlNodeList listChild = node.ChildNodes;
                XmlNodeList listaTacaka = listChild[9].ChildNodes;

                foreach (XmlNode temp in listaTacaka)
                {
                    PZ2.Models.Point tacka = new PZ2.Models.Point(double.Parse(temp.SelectSingleNode("X").InnerText), double.Parse(temp.SelectSingleNode("Y").InnerText));
                    ToLatLon(tacka.x, tacka.y, 34, out double newX, out double newY);
                    if (newX >= donjiLeviLat && newX <= gornjiDesniLat && newY >= donjiLeviLon && newY <= gornjiDesniLon)
                    {
                        lineEntity.points.Add(tacka);
                    }
                }

                if (listaTacaka.Count == lineEntity.points.Count)
                    listLines.Add(lineEntity);
            }
        }

        private GeometryModel3D DrawCube(double tempX, double tempY, SolidColorBrush color)
        {
            //skaliranje x i y koordinata (tempX i tempY) na
            //odgovarajući opseg u rasponu od 0 do 1, kako bi se kocka pravilno prikazala na mapi
            double X = (tempY - donjiLeviLon) / (gornjiDesniLon - donjiLeviLon) * (1.0 - velicinaKocke); //skaliranje na mapu
            double Y = (tempX - donjiLeviLat) / (gornjiDesniLat - donjiLeviLat) * (1.0 - velicinaKocke);
            double Z = 0;

            //kolekcija tačaka position koja predstavlja osam vrhova kocke
            //Svaka tačka se dodaje na osnovu izračunatih koordinata X, Y i Z
            Point3DCollection position = new Point3DCollection 
            {
                new Point3D(X, Y, Z),
                new Point3D(X + velicinaKocke, Y, Z),
                new Point3D(X, Y + velicinaKocke, Z),
                new Point3D(X + velicinaKocke, Y + velicinaKocke, Z),
                new Point3D(X, Y, Z + velicinaKocke),
                new Point3D(X + velicinaKocke, Y, Z + velicinaKocke),
                new Point3D(X, Y + velicinaKocke, Z + velicinaKocke),
                new Point3D(X + velicinaKocke, Y + velicinaKocke, Z + velicinaKocke)
            };

            MeshGeometry3D mreza = new MeshGeometry3D 
            {
                Positions = position,
                TriangleIndices = idk
            };

            foreach (var temp in mapa.Children)
            {
                //Za svaku kocku se proverava da li se poklapa sa novom kockom
                //Ako da, vrši se pomeranje novih tačaka kocke za veličinu kocke
                //duž Z-ose kako bi se izbeglo preklapanje.
                if (Math.Abs(mreza.Bounds.X - temp.Bounds.X) < velicinaKocke &&  //provera da li se poklapaju kocke
                    Math.Abs(mreza.Bounds.Y - temp.Bounds.Y) < velicinaKocke &&
                    Math.Abs(mreza.Bounds.Z - temp.Bounds.Z) < velicinaKocke)
                {
                    for (var i = 0; i < mreza.Positions.Count; i++)  //ako se poklapaju, idu jedna na drugu
                    {
                        mreza.Positions[i] = new Point3D(mreza.Positions[i].X,
                                                         mreza.Positions[i].Y,
                                                         mreza.Positions[i].Z + velicinaKocke);
                    }
                }
            }

            GeometryModel3D gm3D = new GeometryModel3D
            {
                Material = new DiffuseMaterial(color),
                Geometry = mreza
            };
            return gm3D;
        }

        private void DrawSubstations()
        {
            foreach (var temp in listSubstations)
            {
                var v = DrawCube(temp.x, temp.y, temp.boja);
                v.SetValue(FrameworkElement.TagProperty, temp);
                mapa.Children.Add(v);
            }
        }
        private void DrawNodes() 
        {
            foreach (var temp in listNodes)
            {
                var v = DrawCube(temp.x, temp.y, temp.boja);
                v.SetValue(FrameworkElement.TagProperty, temp); //omogućava povezivanje objekta podstanice sa vizuelnim prikazom kocke na mapi
                mapa.Children.Add(v);
            }
        }
        private void DrawSwitches() 
        {
            foreach (var temp in listSwitches)
            {
                var v = DrawCube(temp.x, temp.y, temp.boja);
                v.SetValue(FrameworkElement.TagProperty, temp);
                mapa.Children.Add(v);
            }
        }

        private void DrawLines() 
        {
            foreach (var item in listLines)
            {
                double x;
                double y;
                List<System.Windows.Point> pointsList = new List<System.Windows.Point>();

                foreach (var item2 in item.points) //idemo po svim tackama da napravimo liniju
                {
                    ToLatLon(item2.x, item2.y, 34, out x, out y);
                    double newY = (x - donjiLeviLat) / (gornjiDesniLat - donjiLeviLat) * (1.0 - velicinaKocke); //skaliranje na mapu 
                    double newX = (y - donjiLeviLon) / (gornjiDesniLon - donjiLeviLon) * (1.0 - velicinaKocke);

                    System.Windows.Point point = new System.Windows.Point(newX, newY);
                    pointsList.Add(point);
                }

                for (int i = 0; i < pointsList.Count - 1; i++) //crtanje vodova
                { 
                    Point3DCollection Positions = new Point3DCollection();
                    Positions.Add(new Point3D(pointsList[i].X, pointsList[i].Y, 0));
                    Positions.Add(new Point3D(pointsList[i].X + sirinaIVisinaLinije, pointsList[i].Y, 0));
                    Positions.Add(new Point3D(pointsList[i].X, pointsList[i].Y + sirinaIVisinaLinije, 0));
                    Positions.Add(new Point3D(pointsList[i].X + sirinaIVisinaLinije, pointsList[i].Y + sirinaIVisinaLinije, 0));
                    Positions.Add(new Point3D(pointsList[i + 1].X, pointsList[i + 1].Y, sirinaIVisinaLinije));
                    Positions.Add(new Point3D(pointsList[i + 1].X + sirinaIVisinaLinije, pointsList[i + 1].Y, sirinaIVisinaLinije));
                    Positions.Add(new Point3D(pointsList[i + 1].X, pointsList[i + 1].Y + sirinaIVisinaLinije, sirinaIVisinaLinije));
                    Positions.Add(new Point3D(pointsList[i + 1].X + sirinaIVisinaLinije, pointsList[i + 1].Y + sirinaIVisinaLinije, sirinaIVisinaLinije));

                    GeometryModel3D obj = new GeometryModel3D();
                    if (item.conductorMaterial == "Steel")
                    {
                        obj.Material = new DiffuseMaterial(Brushes.Black);
                    }
                    else if (item.conductorMaterial == "Acsr")
                    {
                        obj.Material = new DiffuseMaterial(Brushes.Red);
                    }
                    else if (item.conductorMaterial == "Copper")
                    {
                        obj.Material = new DiffuseMaterial(Brushes.Orange);
                    }
                    else if (item.conductorMaterial == "Other")
                    {
                        obj.Material = new DiffuseMaterial(Brushes.Gray);
                    }
                    obj.Geometry = new MeshGeometry3D() { Positions = Positions, TriangleIndices = idk };
                    obj.SetValue(FrameworkElement.TagProperty, item);

                    mapa.Children.Add(obj);
                }
            }
        }

        private void Viewport3d_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) //levi klik
        {
            viewport3d.ReleaseMouseCapture();
            opis.IsOpen = false; 
        }

        private void Viewport3d_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewport3d.CaptureMouse();
            pocetnaTacka = e.GetPosition(this); 

            //stavi offset na 0 na tacku koja je kliknuta
            original.X = translate.OffsetX;
            original.Y = translate.OffsetY;

            PointHitTestParameters pointparams = new PointHitTestParameters(pocetnaTacka);
            VisualTreeHelper.HitTest(this, null, HTResult, pointparams);
        }

        private void Viewport3d_MouseMove(object sender, MouseEventArgs e)
        {
            if (viewport3d.IsMouseCaptured) //translate
            {
                System.Windows.Point tacka = e.MouseDevice.GetPosition(this);  //pozicija gde smo kliknuli
                double poXOsi = tacka.X - pocetnaTacka.X; //razlika trenutne pozicije i pocetne pozicije misa
                double poYOsi = tacka.Y - pocetnaTacka.Y;

                double sirina = this.Width;
                double visina = this.Height;

                double translacijaPoX = (poXOsi * 100) / sirina;  //transliranje po x i y osi
                double translacijaPoY = -(poYOsi * 100) / visina;

                translate.OffsetX = original.X + (translacijaPoX / (100 * scale.ScaleX)); //nova vrednost pomaka 
                translate.OffsetY = original.Y + (translacijaPoY / (100 * scale.ScaleY));
            }
            else if (e.MiddleButton == MouseButtonState.Pressed) //rotate
            {
                System.Windows.Point tacka = e.GetPosition(viewport3d);
                double poXOsi = (tacka.X - pocetnaTackaRotacije.X) + ugao1.Angle; //pomak misa plus ugao
                double poYOsi = (tacka.Y - pocetnaTackaRotacije.Y) + ugao2.Angle;

                if (-90 <= poXOsi && poXOsi <= 90)
                {
                    ugao1.Angle = poXOsi;
                }
                if (-90 <= poYOsi && poYOsi <= 90)
                {
                    ugao2.Angle = poYOsi;
                }
                pocetnaTackaRotacije = tacka;
            }
        }

        private void Viewport3d_MouseWheel(object sender, MouseWheelEventArgs e) //scale
        {
            System.Windows.Point p = e.MouseDevice.GetPosition(this);
            double skaliranjePoX = 1; 
            double skaliranjePoY = 1;
            double skaliranjePoZ = 1;
            if (e.Delta > 0 && zoomTrenutni < zoomMax)
            {
                skaliranjePoX = scale.ScaleX + 0.025;
                skaliranjePoY = scale.ScaleY + 0.025;
                skaliranjePoZ = scale.ScaleZ + 0.025;
                zoomTrenutni++;
                scale.ScaleX = skaliranjePoX;
                scale.ScaleY = skaliranjePoY;
                scale.ScaleZ = skaliranjePoZ;

            }
            else if (e.Delta <= 0 && zoomTrenutni > zoomMin)
            {
                skaliranjePoX = scale.ScaleX - 0.025;
                skaliranjePoY = scale.ScaleY - 0.025;
                skaliranjePoZ = scale.ScaleZ - 0.025;
                zoomTrenutni--;
                scale.ScaleX = skaliranjePoX;
                scale.ScaleY = skaliranjePoY;
                scale.ScaleZ = skaliranjePoZ;
            }
        }

        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rezultat)
        {
            var hitResult = rezultat as RayHitTestResult;
            var value = hitResult?.ModelHit.GetValue(FrameworkElement.TagProperty);
            Model3D originalModel = hitResult?.ModelHit;

            if (value is NodeEntity || value is SwitchEntity || value is SubstationEntity)
            {
                opis.Content = ((PowerEntity)value).tooltip; 
                opis.IsOpen = true; 
            }
            else if (value is LineEntity) //klik na liniju, i spajanje 2 entiteta
            {
                LineEntity line = value as LineEntity;
                

                foreach(GeometryModel3D end in endList)
                {
                    var endEntitet = end.GetValue(TagProperty);
                    if(endEntitet is PowerEntity)
                    {
                        PowerEntity powerEntity = (PowerEntity)endEntitet;
                        end.Material = new DiffuseMaterial(powerEntity.boja);
                    }
                }
                endList.Clear();

                foreach (Model3D model in mapa.Children)
                {
                    var entitet = model.GetValue(TagProperty);
                    if (entitet is PowerEntity) 
                    {
                        PowerEntity temp = entitet as PowerEntity;
                        if (temp.id == line.firstEnd || temp.id == line.secondEnd)
                        {

                            endList.Add(model as GeometryModel3D);
                        }
                    }
                    if (endList.Count == 2) //2 entiteta
                        break;
                }

                foreach (var v in endList)
                {
                    v.Material = new DiffuseMaterial(Brushes.Yellow);
                }
            }
            return HitTestResultBehavior.Stop;
        }

        private void SakrijNeaktivan_Click(object sender, RoutedEventArgs e) {
            if (sakrijNeaktivne)   //ako je vec obelezeno i hocemo da vratimo na inicijalno
            {
                foreach (Model3D model in listaSvihVodovaZaBrisanje)
                {
                    mapa.Children.Add(model); 
                }
                foreach (Model3D model in drugiNodoviZaBrisanjeLista)
                {
                    mapa.Children.Add(model);
                }
                listaSvihVodovaZaBrisanje.Clear();
                drugiNodoviZaBrisanjeLista.Clear();
                this.SakrijNeaktivan.Background = new SolidColorBrush(Colors.LightGray);
            } else
            { 

                foreach (Model3D model in mapa.Children)  //nije obelezeno
                {
                    var vod = model.GetValue(TagProperty);
                    if (vod is LineEntity)
                    {
                        LineEntity line = vod as LineEntity;

                        foreach (Model3D model1 in mapa.Children)
                        {
                            var switchEnt = model1.GetValue(TagProperty);
                            if (switchEnt is SwitchEntity) 
                            {
                                SwitchEntity swE = switchEnt as SwitchEntity;
                                if (swE.status == "Open" && swE.id == line.firstEnd) //koji je otvoren i izlazi iz switcha
                                {
                                    listaSvihVodovaZaBrisanje.Add(model);//smestam vod
                                }
                            }
                        }
                    }
                }
                foreach (Model3D model2 in listaSvihVodovaZaBrisanje) //second end
                {
                    var entitet = model2.GetValue(TagProperty);
                    if (entitet is LineEntity)
                    {
                        LineEntity temp = entitet as LineEntity;
                        foreach (Model3D model1 in mapa.Children)
                        {
                            var powerEntity = model1.GetValue(TagProperty);
                            if (powerEntity is PowerEntity) 
                            {
                                PowerEntity pwE = powerEntity as PowerEntity;
                                if (pwE.id == temp.secondEnd) 
                                {
                                    drugiNodoviZaBrisanjeLista.Add(model1);
                                }
                            }
                        }
                    }
                }
                foreach (Model3D modelZaBrisanje in listaSvihVodovaZaBrisanje)  //na kraju brisemo
                {
                    mapa.Children.Remove(modelZaBrisanje); 
                }
                foreach (Model3D modelZaBrisanje in drugiNodoviZaBrisanjeLista)
                {
                    mapa.Children.Remove(modelZaBrisanje); 
                }
                this.SakrijNeaktivan.Background = new SolidColorBrush(Colors.Green);
            }
            sakrijNeaktivne = !sakrijNeaktivne;
        }

        private void PromeniBojuSwitch_Click(object sender, RoutedEventArgs e) {
            if (promeniSwitch)   //ako je vec klinuto pa da vratimo na inicijalno
            {
                if (switchClosedList.Count > 0)
                {
                    foreach (var v in switchClosedList)
                    {
                        v.Material = new DiffuseMaterial(Brushes.Blue); //plava inicijalna
                    }
                    switchClosedList.Clear();
                }
                if (switchOpenList.Count > 0)
                {
                    foreach (var v in switchOpenList)
                    {
                        v.Material = new DiffuseMaterial(Brushes.Blue);
                    }
                    switchOpenList.Clear();
                }
                this.PromeniSwitch.Background = new SolidColorBrush(Colors.LightGray);
            }
            else
            {
                foreach (Model3D model in mapa.Children)   //nije obelezeno
                {
                    var entitet = model.GetValue(TagProperty);
                    if (entitet is SwitchEntity)
                    {
                        SwitchEntity temp = entitet as SwitchEntity;
                        if (temp.status == "Closed")
                        {
                            switchClosedList.Add(model as GeometryModel3D);
                        }
                        if (temp.status == "Open")
                        {
                            switchOpenList.Add(model as GeometryModel3D);
                        }
                    }
                }
                foreach (var v in switchClosedList)
                {
                    v.Material = new DiffuseMaterial(Brushes.Red);
                }
                foreach (var v in switchOpenList)
                {
                    v.Material = new DiffuseMaterial(Brushes.Green); 
                }
                this.PromeniSwitch.Background = new SolidColorBrush(Colors.Green);
            }
            promeniSwitch = !promeniSwitch;
        }

        private void PromeniBojuVod_Click(object sender, RoutedEventArgs e) {
            if (promeniVod)
            {
                VratiBojuVodova(vodoviIspod1List);
                VratiBojuVodova(vodoviIzmedju1I2List);
                VratiBojuVodova(vodoviIznad2List);

                vodoviIspod1List.Clear();
                vodoviIzmedju1I2List.Clear();
                vodoviIznad2List.Clear();
                this.PromeniVod.Background = new SolidColorBrush(Colors.LightGray);
            }
            else
            {
                foreach (Model3D model in mapa.Children) 
                {
                    var entitet = model.GetValue(TagProperty);
                    if (entitet is LineEntity)
                    {
                        LineEntity temp = entitet as LineEntity;   //otpornost
                        if (temp.r < 1)
                        {
                            vodoviIspod1List.Add(model as GeometryModel3D);
                        }
                        else if (temp.r >= 1 && temp.r <= 2)
                        {
                            vodoviIzmedju1I2List.Add(model as GeometryModel3D); 
                        }
                        else if (temp.r > 2)
                        {
                            vodoviIznad2List.Add(model as GeometryModel3D); 
                        }
                    }
                }
                foreach (var v in vodoviIspod1List)
                {
                    v.Material = new DiffuseMaterial(Brushes.Red); 
                }
                foreach (var v in vodoviIzmedju1I2List)
                {
                    v.Material = new DiffuseMaterial(Brushes.Orange);
                }
                foreach (var v in vodoviIznad2List)
                {
                    v.Material = new DiffuseMaterial(Brushes.Yellow); 
                }
                this.PromeniVod.Background = new SolidColorBrush(Colors.Green);
            }
            promeniVod = !promeniVod;
        }

         
        public void VratiBojuVodova(List<GeometryModel3D> vodLista)   //po vrsti materijala
        { 
            foreach (var v in vodLista)
            {
                var entitet = v.GetValue(TagProperty);
                if (entitet is LineEntity)
                {
                    LineEntity vod = entitet as LineEntity;

                    if (vod.conductorMaterial == "Steel")
                    {
                        v.Material = new DiffuseMaterial(Brushes.Black);
                    }
                    else if (vod.conductorMaterial == "Acsr")
                    {
                        v.Material = new DiffuseMaterial(Brushes.Red);
                    }
                    else if (vod.conductorMaterial == "Copper")
                    {
                        v.Material = new DiffuseMaterial(Brushes.Orange);
                    }
                    else if (vod.conductorMaterial == "Other")
                    {
                        v.Material = new DiffuseMaterial(Brushes.Gray);
                    }
                }
            }
        }

        public void SakrijCelik_Click(object sender, RoutedEventArgs e)
        {
            if (sakrijCelik)  //vec obelezeno
            {
                if (sakrijVodoveCelik.Count > 0)
                {
                    foreach (var v in sakrijVodoveCelik)
                    {
                        mapa.Children.Add(v);
                    }
                    sakrijVodoveCelik.Clear();
                }
                this.SakrijCelik.Background = new SolidColorBrush(Colors.LightGray); //dugme
            }
            else
            {
                foreach (Model3D model in mapa.Children)  //ako kliknemo da se obelezi
                {
                    var entitet = model.GetValue(TagProperty);
                    if (entitet is LineEntity)
                    {
                        LineEntity temp = entitet as LineEntity;
                        if (temp.conductorMaterial == "Steel")
                        {
                            sakrijVodoveCelik.Add(model as GeometryModel3D);
                        }
                    }
                }
                foreach (var v in sakrijVodoveCelik)
                {
                    mapa.Children.Remove(v);
                }              
                this.SakrijCelik.Background = new SolidColorBrush(Colors.Green); //dugme
            }
            sakrijCelik = !sakrijCelik;
        }

        public void SakrijAluminijum_Click(object sender, RoutedEventArgs e)
        {
            if (sakrijAluminijum)
            {
                if (sakrijVodoveAluminijum.Count > 0)
                {
                    foreach (var v in sakrijVodoveAluminijum)
                    {
                        mapa.Children.Add(v);
                    }
                    sakrijVodoveAluminijum.Clear();
                }
                this.SakrijAluminijum.Background = new SolidColorBrush(Colors.LightGray);
            }
            else
            {
                foreach (Model3D model in mapa.Children)
                {
                    var entitet = model.GetValue(TagProperty);
                    if (entitet is LineEntity)
                    {
                        LineEntity temp = entitet as LineEntity;
                        if (temp.conductorMaterial == "Acsr")
                        {
                            sakrijVodoveAluminijum.Add(model as GeometryModel3D);
                        }
                    }
                }
                foreach (var v in sakrijVodoveAluminijum)
                {
                    mapa.Children.Remove(v);
                }
                this.SakrijAluminijum.Background = new SolidColorBrush(Colors.Green);
            }
            sakrijAluminijum = !sakrijAluminijum;
        }
        public void SakrijBakar_Click(object sender, RoutedEventArgs e)
        {
            if (sakrijBakar)
            {
                if (sakrijVodoveBakar.Count > 0)
                {
                    foreach (var v in sakrijVodoveBakar)
                    {
                        mapa.Children.Add(v);
                    }
                    sakrijVodoveBakar.Clear();
                }
                this.SakrijBakar.Background = new SolidColorBrush(Colors.LightGray);
            }
            else
            {
                foreach (Model3D model in mapa.Children)
                {
                    var entitet = model.GetValue(TagProperty);
                    if (entitet is LineEntity)
                    {
                        LineEntity temp = entitet as LineEntity;
                        if (temp.conductorMaterial == "Copper")
                        {
                            sakrijVodoveBakar.Add(model as GeometryModel3D);
                        }
                    }
                }
                foreach (var v in sakrijVodoveBakar)
                {
                    mapa.Children.Remove(v);
                }
                this.SakrijBakar.Background = new SolidColorBrush(Colors.Green);
            }
            sakrijBakar = !sakrijBakar;
        }
        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }
    }
}
