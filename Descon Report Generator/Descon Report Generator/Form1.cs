using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using Descon.Data;
using System.Collections;
using System.Collections.Generic;


namespace Descon_Report_Generator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnGetAllCombinations_Click(object sender, EventArgs e)
        {
            GetfullReports();
        }

        private void btnGenTheReports_Click(object sender, EventArgs e)
        {
            GetTheReports();
        }

        private void GetTheReports()
        {
            var loadData = new LoadDataFromXML();
            var folderBrowser = new FolderBrowserDialog();

            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                var files = Directory.GetFiles(folderBrowser.SelectedPath);
                foreach (var path in files)
                {
                    if (path.EndsWith(ConstString.FILE_EXTENSION_DRAWING))
                    {
                        
                           Preferences preference= loadData.LoadPreferencesForUnity();
                           CommonDataStatic.Preferences = preference;
                        var file = loadData.LoadDesconDrawing(path);
                        if (file != null)
                            CreateFourCopiesOfReport(file, path);
                    }
                }

                MessageBox.Show("All the reports have been generated", "All Reports Saved");
            }
        }

        /* TODO: 
         * Change all shapes in Detail Data objects to metric for metric files using the logic below. Angles, Tee's, etc
         * Change other values to metric equivalents
         * With the * 1.5 ASD and LRFD should match exactly
         * Metric values must match US values through conversion. For example anything is 1 inch must be 25.4 mm
         * Add more options to change later, such as shapes
         */

        /// <summary>
        /// Creates the four copies of report.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="path">The path.</param>
       private void CreateFourCopiesOfReport(SaveFileStructure files, string path)
	    {
            SaveFileStructure file;
            if (files.CalcMode == ECalcMode.ASD && files.UnitSystem == EUnit.US)  // THIS WILL MAKE THE COPIES OF ASD TO US IN LRFD-METRIC, LRFD TO US,LRFD TO METRIC
            {
                //LRFD TO US
                file = files;
                file.CalcMode = ECalcMode.LRFD;
                file.UnitSystem = EUnit.US;
                foreach (var detailData in file.DetailDataList)
                {
                    detailData.Shape = CommonDataStatic.AllShapes.First(s => s.Value.Code == detailData.Shape.Code ).Value;
                   // detailData.ShearForce /= 4.43;
                }
                
                SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_LRFD_US" + ConstString.FILE_EXTENSION_DRAWING, file);
               
                //ASD TO METRIC
                file = files;
                file.CalcMode = ECalcMode.ASD;
                #region     
//                List<string> values = ConvertUnits.GenerateReportGoToList();

//                var list = new List<string>();

//                foreach (var line in CommonDataStatic.DetailReportLineList)
//                {
//                    switch (line.LineType)
//                    {
//                        case EReportLineType.MainHeader:
//                            list.Add(line.LineString.TrimEnd(':'));
//                            break;
//                        case EReportLineType.GoToHeader:
//                            list.Add("     " + line.LineString.TrimEnd(':'));
//                            break;
//                    }
//                }
#endregion
                file.UnitSystem = CommonDataStatic.Preferences.Units = EUnit.Metric;
                foreach (var detailData in file.DetailDataList)
                {
                    CommonDataStatic.DetailDataDict[detailData.MemberType] = detailData;
                   //detailData.ShearForce *= 4.43794964028777;
                                 
                }
               
                
                ConvertUnits.UnitsChanged();
                file.DetailDataList.Clear();
                file.DetailDataList.AddRange(CommonDataStatic.DetailDataDict.Values);
          
             SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_ASD_Metric" + ConstString.FILE_EXTENSION_DRAWING, file);


                //LRFD TO METRIC
                file = files;
                file.CalcMode = ECalcMode.LRFD;
                file.UnitSystem = CommonDataStatic.Preferences.Units = EUnit.Metric;
                foreach (var detailData in file.DetailDataList)
                {
                    CommonDataStatic.DetailDataDict[detailData.MemberType] = detailData;
                    detailData.ShearForce /= 4.445;
                    detailData.Moment /= 113.09; 
                }
                ConvertUnits.UnitsChanged();
                file.DetailDataList.Clear();
                file.DetailDataList.AddRange(CommonDataStatic.DetailDataDict.Values);
                SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_LRFD_Metric" + ConstString.FILE_EXTENSION_DRAWING, file);
            }
            else if (files.CalcMode == ECalcMode.LRFD && files.UnitSystem == EUnit.US) // THIS WILL MAKE THE COPIES OF LRFD TO US IN LRFD-METRIC, ASD TO US,LRFD TO METRIC
            {
                //ASD TO US
                file = files;
               
                file.CalcMode = ECalcMode.ASD;
                file.UnitSystem = EUnit.US;
                foreach (var detailData in file.DetailDataList)
                {
                    detailData.Shape = CommonDataStatic.AllShapes.First(s => s.Value.Code == detailData.Shape.Code).Value;
                    // detailData.ShearForce *= 4.4247787610619469026548672566372;
                }
                SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_ASD_US" + ConstString.FILE_EXTENSION_DRAWING, file);

                //ASD TO METRIC
                file = files;
                file.CalcMode = ECalcMode.ASD;
                file.UnitSystem = CommonDataStatic.Preferences.Units = EUnit.Metric;
                foreach (var detailData in file.DetailDataList)
                {
                    CommonDataStatic.DetailDataDict[detailData.MemberType] = detailData;
                   // detailData.ShearForce /=4.432;
                } 
                ConvertUnits.UnitsChanged();
                file.DetailDataList.Clear();
                file.DetailDataList.AddRange(CommonDataStatic.DetailDataDict.Values);
                SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_ASD_Metric" + ConstString.FILE_EXTENSION_DRAWING, file);

                //LRFD TO METRIC
                file = files;
                file.CalcMode = ECalcMode.LRFD;
                file.UnitSystem = CommonDataStatic.Preferences.Units = EUnit.Metric;


                foreach (var detailData in file.DetailDataList)
                {
                    CommonDataStatic.DetailDataDict[detailData.MemberType] = detailData;
                    detailData.ShearForce /= 4.445;
                    detailData.Moment /= 113.09; 
                     //detailData.GageOnFlange /= 25.4;
                     //detailData.AngleY /= 25.4;
                     //detailData.AxialCompression /= 25.4;
                     //detailData.AxialTension /= 25.4;
                     //detailData.BraceY /= 25.4;
                     //detailData.Compression *= 25.4;//19.01530053
                     //detailData.EndSetback /=25.4;
                     //detailData.FrontX /= 25.4;
                     //detailData.FrontY /= 25.4;
                     //detailData.FxP /= 25.4;
                     //detailData.GageOnColumn /= 25.4;
                     //detailData.GageOnFlange /= 25.45454545454545;
                     //detailData.MinThickness /= 25.4;
                     //detailData.PFlange /= 25.4;
                     //detailData.PWeb /= 25.4;
                     //detailData.Tension /= 25.4;
                     //detailData.TransferCompression /= 25.4;
                     //detailData.TransferTension /= 25.4;
                     //detailData.Wno /= 25.4;
                     //detailData.WorkPointX /= 25.4;
                     //detailData.WorkPointY /= 25.4;
                }
                ConvertUnits.UnitsChanged();
                file.DetailDataList.Clear();
                file.DetailDataList.AddRange(CommonDataStatic.DetailDataDict.Values);
                SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_LRFD_Metric" + ConstString.FILE_EXTENSION_DRAWING, file);
            }
            else if (files.CalcMode == ECalcMode.LRFD && files.UnitSystem == EUnit.Metric)     // THIS WILL MAKE THE COPIES OF LRFD TO METRIC IN ASD-METRIC, ASD TO US,LRFD TO US
            {   
                  //ASD TO METRIC
                  file = files;
                file.CalcMode = ECalcMode.ASD;
                file.UnitSystem = EUnit.Metric;
                ConvertUnits.ReloadAngleShapeLists();
                foreach (var detailData in file.DetailDataList)
                {
                    detailData.Shape = CommonDataStatic.AllShapes.First(s => s.Value.Code == detailData.Shape.Code && s.Value.UnitSystem == EUnit.Metric).Value;
                }

              //  ConvertUnits.UnitsChanged();
                
                SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_ASD_Metric" + ConstString.FILE_EXTENSION_DRAWING, file);

              
                //ASD TO US
                file = files;
                file.CalcMode = ECalcMode.ASD;
                file.UnitSystem = CommonDataStatic.Preferences.Units = EUnit.US;


                foreach (var detailData in file.DetailDataList)
                {
                    CommonDataStatic.DetailDataDict[detailData.MemberType] = detailData;
                    //detailData.ShearForce *= 4.44;
                }
                ConvertUnits.UnitsChanged();
                file.DetailDataList.Clear();
                file.DetailDataList.AddRange(CommonDataStatic.DetailDataDict.Values);
              
                SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_ASD_US" + ConstString.FILE_EXTENSION_DRAWING, file);

                //LRFD TO US
                file = files;
                file.CalcMode = ECalcMode.LRFD;
                file.UnitSystem = CommonDataStatic.Preferences.Units = EUnit.US;


                foreach (var detailData in file.DetailDataList)
                {
                    CommonDataStatic.DetailDataDict[detailData.MemberType] = detailData;
                    detailData.ShearForce *= 4.45;
                    detailData.Moment *= 113.09;
                }
                ConvertUnits.UnitsChanged();
                file.DetailDataList.Clear();
                file.DetailDataList.AddRange(CommonDataStatic.DetailDataDict.Values);
               
              SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_LRFD_US" + ConstString.FILE_EXTENSION_DRAWING, file);
            }
            else if (files.CalcMode == ECalcMode.ASD && files.UnitSystem == EUnit.Metric)    // THIS WILL MAKE THE COPIES OF ASD TO METRIC IN LRFD-METRIC, ASD TO US,LRFD TO US
            {
                //ASD TO METRIC
                file = files;
                // The following two need to be converted to metric
                file.CalcMode = ECalcMode.LRFD;
                file.UnitSystem = EUnit.Metric; 
                foreach (var detailData in file.DetailDataList)
                {
                    detailData.Shape = CommonDataStatic.AllShapes.First(s => s.Value.Code == detailData.Shape.Code && s.Value.UnitSystem == EUnit.Metric).Value;
                }
              
                SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_LRFD_Metric" + ConstString.FILE_EXTENSION_DRAWING, file);

                //ASD TO US
                file = files;
                file.CalcMode = ECalcMode.ASD;
                file.UnitSystem = CommonDataStatic.Preferences.Units = EUnit.US;


                foreach (var detailData in file.DetailDataList)
                {
                    CommonDataStatic.DetailDataDict[detailData.MemberType] = detailData;
                    //detailData.ShearForce /= 5.489795918367347;
                
                  
                }
                ConvertUnits.UnitsChanged();
                file.DetailDataList.Clear();
                file.DetailDataList.AddRange(CommonDataStatic.DetailDataDict.Values);  
                SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_ASD_US" + ConstString.FILE_EXTENSION_DRAWING, file);

               //LRFD TO US
                file = files;
                file.CalcMode = ECalcMode.LRFD;
                file.UnitSystem = CommonDataStatic.Preferences.Units = EUnit.US;


                foreach (var detailData in file.DetailDataList)
                {
                    CommonDataStatic.DetailDataDict[detailData.MemberType] = detailData;
                   detailData.ShearForce *= 4.45;
                   detailData.Moment *= 113.09;
                }
                ConvertUnits.UnitsChanged();
                file.DetailDataList.Clear();
                file.DetailDataList.AddRange(CommonDataStatic.DetailDataDict.Values);
                SaveFileToDisk(path.TrimEnd(ConstString.FILE_EXTENSION_DRAWING.ToCharArray()) + "_LRFD_US" + ConstString.FILE_EXTENSION_DRAWING, file);
            }


	    }

        #region copies of report
        private void GetfullReports()
        {
            var loadData = new LoadDataFromXML();

            var folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                var files = Directory.GetFiles(folderBrowser.SelectedPath);
                foreach (var path in files)
                {
                    if (path.EndsWith(ConstString.FILE_EXTENSION_DRAWING))
                    {

                        Preferences preference = loadData.LoadPreferencesForUnity();
                        CommonDataStatic.Preferences = preference;
                        var file = loadData.LoadDesconDrawing(path);
                        if (file != null)
                            CreateCopiesOfReport(file, path);
                    }
                }

                MessageBox.Show("Complete", "All Reports Saved");
            }
        }
        private void CreateCopiesOfReport(SaveFileStructure file, string path)
        {
            string FolderName = path.Substring(path.LastIndexOf("\\")+1, path.IndexOf('.') - (path.LastIndexOf("\\")+1));
            Directory.CreateDirectory(path.TrimEnd((FolderName + ConstString.FILE_EXTENSION_DRAWING).ToCharArray())+FolderName);
            string targetFolderPath = path.TrimEnd((FolderName + ConstString.FILE_EXTENSION_DRAWING).ToCharArray()) + FolderName;
            
            CommonLists listEnumData = new CommonLists();
            //Creating array of Configuration Types
            EJointConfiguration[] Configurations = { EJointConfiguration.BraceToColumn, EJointConfiguration.BraceVToBeam, EJointConfiguration.BraceToColumnBase, EJointConfiguration.ColumnSplice, EJointConfiguration.BeamToGirder, EJointConfiguration.BeamSplice };
            foreach (EJointConfiguration configuration in Configurations) //For each Configuration
            {
                string ConfigurationName = configuration.ToString()+"_";
                file.JointConfig = configuration;
                //Setting Value
                CommonDataStatic.JointConfig = configuration;
                
                
                SortedList MemberList = listEnumData.MemberList;
                foreach (var member in MemberList) //For Each member
                {
                    if((EMemberType)((DictionaryEntry)member).Key==EMemberType.LeftBeam || (EMemberType)((DictionaryEntry)member).Key==EMemberType.RightBeam)
                    {
                    DetailData detailDataObj = new DetailData();
                    CommonDataStatic.SelectedMember = detailDataObj;
                    //Setting Value
                    CommonDataStatic.SelectedMember.MemberType = (EMemberType)((DictionaryEntry)member).Key;
                    string MemberName= ConfigurationName+((DictionaryEntry)member).Value + "_";
                    
                    Dictionary<EShapeType, string> ShapeTypes = listEnumData.ShapeTypeList;
                    foreach (KeyValuePair<EShapeType, string> Shape in ShapeTypes) //For each Shape Type
                    {
                        //Setting Value
                        CommonDataStatic.SelectedMember.ShapeType = Shape.Key;
                        string ShapeTypeName =MemberName+ Shape.Value + "_";
                        Dictionary<string, Shape> selections = CommonDataStatic.AllShapes.Where(s => s.Value.Name == ConstString.NONE ||
                                                          (s.Value.UnitSystem == CommonDataStatic.Units &&
                                                           s.Value.TypeEnum == Shape.Key)).ToDictionary(s => s.Key, s => s.Value);

                        foreach (KeyValuePair<string, Shape> selection in selections) //for each Selection 
                        {
                            if (selection.Key=="None")
                                continue;
                            //Setting Value
                            CommonDataStatic.SelectedMember.Shape = selection.Value;
                            string SelectionName =ShapeTypeName+ selection.Key + "_";
                            Dictionary<string, Material> materials;
                            if (Shape.Key != EShapeType.HollowSteelSection)
                                materials = CommonDataStatic.MaterialDict.Where(m => m.Value.Name != ConstString.HSS_MATERIAL).ToDictionary(m => m.Key, m => m.Value);
                            else
                                materials = CommonDataStatic.MaterialDict;

                            foreach (KeyValuePair<string, Material> material in materials) // For each material
                            {
                                //Setting Value
                                CommonDataStatic.SelectedMember.Material = material.Value;
                               
                                string materialName = material.Key.Contains(@"/") ? SelectionName + material.Key.Replace(@"/","-") + "_" : SelectionName + material.Key + "_";
                                Dictionary<EShearCarriedBy, string> shareConnections = listEnumData.ShearCarriedByList;
                                foreach (KeyValuePair<EShearCarriedBy, string> shareConnection in shareConnections) // For each Shear Connection
                                {
                                    //Setting Value
                                    CommonDataStatic.SelectedMember.ShearConnection = shareConnection.Key;
                                    string ShareConenctionName =materialName+ shareConnection.Value + "_";
                                    Dictionary<EMomentCarriedBy, string> momentConnections = listEnumData.MomentCarriedByList();
                                    foreach (KeyValuePair<EMomentCarriedBy, string> moment in momentConnections)
                                    {
                                        //Setting Value
                                        CommonDataStatic.SelectedMember.MomentConnection = moment.Key;
                                        string targetFileName = ShareConenctionName + moment.Value + "_";
                                        
                                        file.JointConfig = CommonDataStatic.JointConfig;
                                        file.SteelCode = CommonDataStatic.Preferences.SteelCode;
                                        file.CalcMode = CommonDataStatic.Preferences.CalcMode;
                                        file.BracingType = CommonDataStatic.Preferences.BracingType;
                                        file.InputForceType = CommonDataStatic.Preferences.InputForceType;
                                        file.UnitSystem = CommonDataStatic.Units;
                                        file.ColumnSplice = CommonDataStatic.ColumnSplice;
                                        file.ColumnStiffener = CommonDataStatic.ColumnStiffener;
                                        file.Seismic = CommonDataStatic.Preferences.Seismic;
                                        file.SeismicSettings = CommonDataStatic.SeismicSettings;

                                        file.DetailDataList = CommonDataStatic.DetailDataDict.Select(detail => detail.Value).ToList();
                                        if (file.CalcMode == ECalcMode.ASD && file.UnitSystem == EUnit.US)
                                        {
                                            targetFileName += "ASD_US";
                                        }
                                        else if (file.CalcMode == ECalcMode.LRFD && file.UnitSystem == EUnit.US)
                                        {
                                            targetFileName += "LRFD_US";
                                            foreach (var detailData in file.DetailDataList)
                                            {
                                                detailData.ShearForce *= 1.5;
                                                detailData.Moment *= 1.5;
                                            }
                                        }
                                        else if (file.CalcMode == ECalcMode.ASD && file.UnitSystem == EUnit.Metric)
                                        {
                                            targetFileName += "ASD_Metric";
                                            foreach (var detailData in file.DetailDataList)
                                            {
                                                detailData.Shape = CommonDataStatic.AllShapes.First(s => s.Value.Code == detailData.Shape.Code && s.Value.UnitSystem == EUnit.Metric).Value;

                                                //Other Values
                                                detailData.SlopeRise *= 25.4; //Converting in to Metric
                                                detailData.AngleX *= 25.4;
                                                detailData.AngleY *= 25.4;
                                                detailData.AxialCompression *= 25.4;
                                                detailData.AxialTension *= 25.4;
                                                detailData.BraceX *= 25.4;
                                                detailData.BraceY *= 25.4;
                                                detailData.Compression *= 25.4;
                                                detailData.EndSetback *= 25.4;
                                                detailData.FrontX *= 25.4;
                                                detailData.FrontY *= 25.4;
                                                detailData.FxP *= 25.4;
                                                detailData.GageOnColumn *= 25.4;
                                                detailData.GageOnFlange *= 25.4;
                                                detailData.MinThickness *= 25.4;
                                                detailData.Moment *= 25.4;
                                                detailData.PFlange *= 25.4;
                                                detailData.PWeb *= 25.4;
                                                detailData.ShearForce *= 25.4;
                                                detailData.SlopeRun *= 25.4;
                                                detailData.Tension *= 25.4;
                                                detailData.TransferCompression *= 25.4;
                                                detailData.TransferTension *= 25.4;
                                                detailData.Wno *= 25.4;
                                                detailData.WorkPointX *= 25.4;
                                                detailData.WorkPointY *= 25.4;
                                            }
                                        }
                                        else if (file.CalcMode == ECalcMode.LRFD && file.UnitSystem == EUnit.Metric)
                                        {
                                            targetFileName += "LRFD_Metric";
                                            foreach (var detailData in file.DetailDataList)
                                            {
                                                detailData.Shape = CommonDataStatic.AllShapes.First(s => s.Value.Code == detailData.Shape.Code && s.Value.UnitSystem == EUnit.Metric).Value;
                                                detailData.ShearForce *= 38.1;
                                                detailData.Moment *= 38.1;
                                                //Other Values
                                                detailData.SlopeRise *= 25.4; //Converting in to Metric
                                                detailData.AngleX *= 25.4;
                                                detailData.AngleY *= 25.4;
                                                detailData.AxialCompression *= 25.4;
                                                detailData.AxialTension *= 25.4;
                                                detailData.BraceX *= 25.4;
                                                detailData.BraceY *= 25.4;
                                                detailData.Compression *= 25.4;
                                                detailData.EndSetback *= 25.4;
                                                detailData.FrontX *= 25.4;
                                                detailData.FrontY *= 25.4;
                                                detailData.FxP *= 25.4;
                                                detailData.GageOnColumn *= 25.4;
                                                detailData.GageOnFlange *= 25.4;
                                                detailData.MinThickness *= 25.4;
                                                detailData.Moment *= 25.4;
                                                detailData.PFlange *= 25.4;
                                                detailData.PWeb *= 25.4;
                                                detailData.ShearForce *= 25.4;
                                                detailData.SlopeRun *= 25.4;
                                                detailData.Tension *= 25.4;
                                                detailData.TransferCompression *= 25.4;
                                                detailData.TransferTension *= 25.4;
                                                detailData.Wno *= 25.4;
                                                detailData.WorkPointX *= 25.4;
                                                detailData.WorkPointY *= 25.4;
                                            }
                                        }
                                        SaveFileToDisk(targetFolderPath+"\\"  + targetFileName + ConstString.FILE_EXTENSION_DRAWING, file);
                                    }
                                }

                            }

                        }
                    }
                }
                }
                
            }

            
        }
#endregion
        private void SaveFileToDisk(string path, SaveFileStructure file)
        {
            var writer = new XmlSerializer(typeof(SaveFileStructure));

            using (var fileStream = new StreamWriter(path))
            {
                writer.Serialize(fileStream, file);
                fileStream.Close();
            }
        }
    }
}
