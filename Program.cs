using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Data.SqlClient;

namespace XMLParser
{
    class DataDesigner
    {

        const string PosOrgLevelsViewName  = "PositionOrgLevels";

        static void Main(string[] args)
        {

            string datafileName = "..\\..\\HR.datadesign";

            DataDesigner designer = new DataDesigner();

            designer.RemovePositionOrgLevelsView(datafileName);

            designer.AddPositionOrgLevelsView(datafileName);


                Console.ReadLine();
        }

        public bool RemovePositionOrgLevelsView(string datafileName)
        {
            XDocument xmlDoc = XDocument.Load(datafileName);
            XNamespace ns = "http://www.eclipse.org/birt/2005/design";

            var descs = from item in xmlDoc.Descendants(ns + "oda-data-set")
                        where (string)item.Attribute("name") == PosOrgLevelsViewName
                        select item;


            var lstProp = from lstProps in descs.Elements(ns + "list-property")
                          where (string)lstProps.Attribute("name") == "columnHints"
                          select lstProps.Elements(ns + "structure");
                        

            foreach (var lst in lstProp)                            
                lst.Remove();
            
            // Remove cachedMetaData section

            descs = from item in xmlDoc.Descendants(ns + "oda-data-set")
                        .Where(item => (string)item.Attribute("name") == PosOrgLevelsViewName)
                        .Elements(ns + "structure").Where(item => item.Attribute("name").Value == "cachedMetaData")            
                        select item;

            
            lstProp = from lstProps in descs.Elements(ns + "list-property")
                      where (string)lstProps.Attribute("name") == "resultSet"
                      select lstProps.Elements(ns + "structure");

            foreach (var lst in lstProp)
                lst.Remove();

            //Remove resultSet section

            descs = from item in xmlDoc.Descendants(ns + "oda-data-set")
                        where (string)item.Attribute("name") == PosOrgLevelsViewName
                        select item;

            
            lstProp = from lstProps in descs.Elements(ns + "list-property")
                      where (string)lstProps.Attribute("name") == "resultSet"
                          select lstProps.Elements(ns + "structure");

            foreach (var lst in lstProp)
                lst.Remove();

            xmlDoc.Save(datafileName);

            return true;
        }

        public bool AddPositionOrgLevelsView(string datafileName)
        {
            XDocument xmlDoc = XDocument.Load(datafileName);
            XNamespace ns = "http://www.eclipse.org/birt/2005/design";

            DataSet ds = getViewInfo();

            XElement dsStruct;

            for (int i = 0; i <= ds.Tables[0].Columns.Count - 1; i++)
            {
                dsStruct = new XElement(ns + "structure", new XElement(ns+"property", ds.Tables[0].Columns[i].ColumnName, new XAttribute("name", "columnName"))
                    ,new XElement(ns + "property", i == 0 ? "measure":"dimension", new XAttribute("name", "analysis"))
                    , new XElement(ns + "text-property", ds.Tables[0].Columns[i].ColumnName, new XAttribute("name", "displayName"))
                    , new XElement(ns + "text-property", ds.Tables[0].Columns[i].ColumnName, new XAttribute("name", "heading"))
                    ,new XElement(ns + "property", "false", new XAttribute("name", "indexColumn"))
                    ,new XElement(ns + "property", "false", new XAttribute("name", "compressed"))
                    );                
                
                xmlDoc.Descendants(ns + "oda-data-set")
                    .Where(item => item.Attribute("name").Value == PosOrgLevelsViewName).Elements(ns + "list-property")
                    .Where(item => item.Attribute("name").Value == "columnHints").First()
                    .Add(dsStruct);
            }

            for (int i = 0; i <= ds.Tables[0].Columns.Count - 1; i++)
            {                

                dsStruct = new XElement(ns + "structure", new XElement(ns + "property", i + 1, new XAttribute("name", "position"))
                    , new XElement(ns + "property", ds.Tables[0].Columns[i].ColumnName, new XAttribute("name", "name"))
                    , new XElement(ns + "property", i == 0 ? "integer" : "string", new XAttribute("name", "dataType"))                    
                    );                

                xmlDoc.Descendants(ns + "oda-data-set")
                    .Where(item => item.Attribute("name").Value == PosOrgLevelsViewName).Elements(ns + "structure")
                    .Where(item => item.Attribute("name").Value == "cachedMetaData").Elements(ns + "list-property").First()
                    .Add(dsStruct);
            }

            for (int i = 0; i <= ds.Tables[0].Columns.Count - 1; i++)
            {
                dsStruct = new XElement(ns + "structure", new XElement(ns + "property", i+1, new XAttribute("name", "position"))
                    , new XElement(ns + "property", ds.Tables[0].Columns[i].ColumnName, new XAttribute("name", "name"))
                    , new XElement(ns + "property", ds.Tables[0].Columns[i].ColumnName, new XAttribute("name", "nativeName"))
                    , new XElement(ns + "property", i == 0 ? "integer" : "string", new XAttribute("name", "dataType"))                    
                    , new XElement(ns + "property", i == 0 ? 4 : 12, new XAttribute("name", "nativeDataType"))
                    );                

                xmlDoc.Descendants(ns + "oda-data-set")
                    .Where(item => item.Attribute("name").Value == PosOrgLevelsViewName).Elements(ns + "list-property")
                    .Where(item => item.Attribute("name").Value == "resultSet").First()
                    .Add(dsStruct);
            }
            //Updata CData Section

            XNamespace nsdesign = "http://www.eclipse.org/datatools/connectivity/oda/design";
            
            XElement designerValues = xmlDoc.Descendants(ns + "oda-data-set").Where(item => item.Attribute("name").Value == PosOrgLevelsViewName).Elements(ns + "xml-property")
            .Where(item => item.Attribute("name").Value == "designerValues").FirstOrDefault();
            
            string cdata = designerValues.Value;

            XElement xmlCdata = XElement.Parse(cdata);
            
            XElement[] newsection= new XElement[ds.Tables[0].Columns.Count];

            for (int i = 0; i <= ds.Tables[0].Columns.Count - 1; i++)                        
            {        

                newsection[i] = new XElement(nsdesign + "resultColumnDefinitions",
                    new XElement(nsdesign + "attributes",
                        new XElement(nsdesign + "identifier",
                            new XElement(nsdesign + "name", ds.Tables[0].Columns[i].ColumnName),
                            new XElement(nsdesign + "position", i+1)),
                        new XElement(nsdesign + "nativeDataTypeCode", i == 0 ? 4 : 12),
                        new XElement(nsdesign + "precision", i == 0 ? 10 : 1000),
                        new XElement(nsdesign + "scale", 0),
                        new XElement(nsdesign + "nullability", "Nullable"),
                        new XElement(nsdesign + "uiHints",
                                new XElement(nsdesign + "displayName",ds.Tables[0].Columns[i].ColumnName))),
                        new XElement(nsdesign + "usageHints",
                        new XElement(nsdesign + "label", ds.Tables[0].Columns[i].ColumnName), new XElement(nsdesign + "formattingHints", new XElement(nsdesign + "displaySize", i == 0 ? 11 : 1000)))
                                );
            }
            xmlCdata.Descendants(nsdesign + "resultSetColumns").FirstOrDefault().ReplaceNodes(newsection);
            

            string newcData = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + xmlCdata.ToString(); 
                                    
            XCData xcdata = new XCData(newcData);

            xmlDoc.Descendants(ns + "oda-data-set").Where(item => item.Attribute("name").Value == PosOrgLevelsViewName).Elements(ns + "xml-property")
            .Where(item => item.Attribute("name").Value == "designerValues").FirstOrDefault().ReplaceNodes(xcdata);
                
            XElement category_column_pos = xmlDoc.Descendants(ns + "property").Where(item => item.Attribute("name").Value == "linkedDataModels").Descendants(ns + "category")
            .Where(item => item.Attribute("name").Value == PosOrgLevelsViewName).Elements(ns + "property")
            .Where(item => item.Attribute("name").Value == "categoryColumns").Elements(ns + "category-column").First();

            xmlDoc.Descendants(ns + "property").Where(item => item.Attribute("name").Value == "linkedDataModels").Descendants(ns + "category")
            .Where(item => item.Attribute("name").Value == PosOrgLevelsViewName).Elements(ns + "property")
            .Where(item => item.Attribute("name").Value == "categoryColumns").Elements(ns + "category-column").Remove();



            XElement[] category_columns = new XElement[ds.Tables[0].Columns.Count];
            category_columns[0] = category_column_pos;
            for (int i = 1; i <= ds.Tables[0].Columns.Count - 1; i++)
            {
                //category_columns[0] = category_column_pos;
                category_columns[i] = new XElement(ns + "category-column",
                new XElement(ns + "property", "Data Model/" + PosOrgLevelsViewName, new XAttribute("name", "aliasDataSet")),
                new XElement(ns + "property", ds.Tables[0].Columns[i].ColumnName, new XAttribute("name", "resultSetColumnName")),
                new XAttribute("name", ds.Tables[0].Columns[i].ColumnName), new XAttribute("id", Convert.ToInt32(category_columns[i - 1].Attribute("id").Value) + 1));
            }
                        
            
            xmlDoc.Descendants(ns + "property").Where(item => item.Attribute("name").Value == "linkedDataModels").Descendants(ns + "category")
            .Where(item => item.Attribute("name").Value == PosOrgLevelsViewName).Elements(ns + "property")
            .Where(item => item.Attribute("name").Value == "categoryColumns").FirstOrDefault().ReplaceNodes(category_columns);
                       
            
            xmlDoc.Save(datafileName);
            

            return true;
        }

        public DataSet getViewInfo()
        {
            string dbServer = "hrms-reporting";
            string tmpDb = "Warranty";
            string dbUsername = "sa";
            string dbPassword = "2OmniWay";

            string ConnectionString = "Server=" + dbServer + ";Database=" + tmpDb
               + ";uid=" + dbUsername + ";pwd=" + dbPassword + ";Connection Timeout=60";

            string sql = "Set fmtonly on;select * from AdhocB_PositionOrgLevels;set fmtonly off";

            SqlConnection con = new SqlConnection(ConnectionString);            
            SqlDataAdapter sqlAdapter = new SqlDataAdapter();
            sqlAdapter.SelectCommand = new SqlCommand(sql, con);
            DataSet ds=new DataSet();
            sqlAdapter.Fill(ds);

            return ds;
        }
    }
    
    
}

