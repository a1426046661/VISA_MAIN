using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;
using static VISAInstrument.AutoTest.PowerTest;
using static VISAInstrument.MCU_Calib;

namespace VISAInstrument
{
    public static class ClearMemoryInfo
    {
        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);


        /// <summary>
        /// 强制清理内存
        /// </summary>
        public static void FlushMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            //System.Diagnostics.Process.GetCurrentProcess().MinWorkingSet = new System.IntPtr(5);
        }
    }

    public class Excel
    {
        public static void ADD_ONE_EXCEL_DAC(string file_name, string sheet_name, string MENU_string, float[] Line1, float[] Line2, float[] Line3)
        {
            XSSFWorkbook workBook = new XSSFWorkbook();
            XSSFSheet newSheet = null;
            if (File.Exists(file_name))
            {
                workBook = Open_xlsx(file_name);
                newSheet = (XSSFSheet)workBook.GetSheet(sheet_name) == null ? (XSSFSheet)workBook.CreateSheet(sheet_name) : (XSSFSheet)workBook.GetSheet(sheet_name);
            }
            else
            {
                workBook = Creat_xlsx(file_name);
                newSheet = (XSSFSheet)workBook.CreateSheet(sheet_name);
            }
            //把ROW算出来
            int ROW = 0;
            while (true)
            {
                if (newSheet.GetRow(1) == null)
                {
                    newSheet.CreateRow(1);
                }
                if (newSheet.GetRow(1).GetCell(ROW) == null)
                {
                    break;
                }
                ROW = ROW + 5;

            }
            //单元格颜色
            ICellStyle cellStyle_data = workBook.CreateCellStyle();  //首先建单元格格式
            style_Data(cellStyle_data);
            ICellStyle cellStyle1_data_menu = workBook.CreateCellStyle();  //首先建单元格格式 
            style_Data_Menu(cellStyle1_data_menu);
            ICellStyle cellStyle1_menu = workBook.CreateCellStyle();  //首先建单元格格式 
            style_Menu(cellStyle1_menu);

            //计算Line4和 MAX MIN AVG
            float[] Line4 = new float[Line3.Length];
            float MAX, MIN, AVG, MAX_MIN;
            for (int i = 0; i < Line4.Length; i++)
            {
                Line4[i] = Line3[i] - Line2[i];
            }
            MAX = Line4.Max();
            MIN = Line4.Min();
            AVG = Line4.Average();
            MAX_MIN = MAX - MIN;

            //line1
            for (int i = 0; i < Line1.Length; i++)
            {

                if (newSheet.GetRow(5 + i) == null)
                {
                    newSheet.CreateRow(5 + i);
                }
                newSheet.GetRow(5 + i).CreateCell(ROW).CellStyle = cellStyle_data;
                newSheet.GetRow(5 + i).GetCell(ROW).SetCellValue(Line1[i]);
            }
            //line2
            for (int i = 0; i < Line2.Length; i++)
            {

                if (newSheet.GetRow(5 + i) == null)
                {
                    newSheet.CreateRow(5 + i);
                }
                newSheet.GetRow(5 + i).CreateCell(ROW + 1).CellStyle = cellStyle_data;
                newSheet.GetRow(5 + i).GetCell(ROW + 1).SetCellValue(Line2[i]);
            }
            //line3
            for (int i = 0; i < Line3.Length; i++)
            {

                if (newSheet.GetRow(5 + i) == null)
                {
                    newSheet.CreateRow(5 + i);
                }
                newSheet.GetRow(5 + i).CreateCell(ROW + 2).CellStyle = cellStyle_data;
                newSheet.GetRow(5 + i).GetCell(ROW + 2).SetCellValue(Line3[i]);
            }
            //line4
            for (int i = 0; i < Line3.Length; i++)
            {

                if (newSheet.GetRow(5 + i) == null)
                {
                    newSheet.CreateRow(5 + i);
                }
                newSheet.GetRow(5 + i).CreateCell(ROW + 3).CellStyle = cellStyle_data;
                newSheet.GetRow(5 + i).GetCell(ROW + 3).SetCellValue((Line3[i] - Line2[i]) * 1000);
            }
            //固定标题1
            if (newSheet.GetRow(1) == null)
            {
                newSheet.CreateRow(1);
            }
            if (newSheet.GetRow(2) == null)
            {
                newSheet.CreateRow(2);
            }
            newSheet.GetRow(1).CreateCell(ROW + 0).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 0).SetCellValue("MIN");
            newSheet.GetRow(1).CreateCell(ROW + 1).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 1).SetCellValue("MAX");
            newSheet.GetRow(1).CreateCell(ROW + 2).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 2).SetCellValue("AVG");
            newSheet.GetRow(1).CreateCell(ROW + 3).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 3).SetCellValue("MAX-MIN");

            newSheet.GetRow(2).CreateCell(ROW + 0).CellStyle = cellStyle_data;
            newSheet.GetRow(2).GetCell(ROW + 0).SetCellValue(MIN);
            newSheet.GetRow(2).CreateCell(ROW + 1).CellStyle = cellStyle_data;
            newSheet.GetRow(2).GetCell(ROW + 1).SetCellValue(MAX);
            newSheet.GetRow(2).CreateCell(ROW + 2).CellStyle = cellStyle_data;
            newSheet.GetRow(2).GetCell(ROW + 2).SetCellValue(AVG);
            newSheet.GetRow(2).CreateCell(ROW + 3).CellStyle = cellStyle_data;
            newSheet.GetRow(2).GetCell(ROW + 3).SetCellValue(MAX_MIN);
            //固定标题2
            if (newSheet.GetRow(4) == null)
            {
                newSheet.CreateRow(4);
            }
            newSheet.GetRow(4).CreateCell(ROW + 0).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(4).GetCell(ROW + 0).SetCellValue("DAC Set");
            newSheet.GetRow(4).CreateCell(ROW + 1).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(4).GetCell(ROW + 1).SetCellValue("Expected");
            newSheet.GetRow(4).CreateCell(ROW + 2).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(4).GetCell(ROW + 2).SetCellValue("DAC Out");
            newSheet.GetRow(4).CreateCell(ROW + 3).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(4).GetCell(ROW + 3).SetCellValue("△(mV)");
            //计算总表

            //合并单元格(表头)
            if (newSheet.GetRow(0) == null)
            {
                newSheet.CreateRow(0);
            }
            ICell cell = newSheet.GetRow(0).CreateCell(ROW);
            CellRangeAddress mergedCell1 = new CellRangeAddress(0, 0, ROW, ROW + 3);
            newSheet.AddMergedRegion(mergedCell1);
            cell.CellStyle = cellStyle1_menu;
            cell.SetCellValue(MENU_string);
            // 写入文件
            Save_xlsx(file_name, workBook);
            ClearMemoryInfo.FlushMemory();
        }

        public static void Gonghao_Test(string file_name, string sheet_name, string MENU_string, List<waishe_struct> lines)
        {
            XSSFWorkbook workBook = new XSSFWorkbook();
            XSSFSheet newSheet = null;
            if (File.Exists(file_name))
            {
                workBook = Open_xlsx(file_name);
                newSheet = (XSSFSheet)workBook.GetSheet(sheet_name) == null ? (XSSFSheet)workBook.CreateSheet(sheet_name) : (XSSFSheet)workBook.GetSheet(sheet_name);
            }
            else
            {
                workBook = Creat_xlsx(file_name);
                newSheet = (XSSFSheet)workBook.CreateSheet(sheet_name);
            }
            //把ROW算出来
            int ROW = 0;
            while (true)
            {
                if (newSheet.GetRow(1) == null)
                {
                    newSheet.CreateRow(1);
                }
                if (newSheet.GetRow(1).GetCell(ROW) == null)
                {
                    break;
                }
                ROW = ROW + 5;

            }
            //单元格颜色
            ICellStyle cellStyle_data = workBook.CreateCellStyle();  //首先建单元格格式
            style_Data(cellStyle_data);
            ICellStyle cellStyle1_data_menu = workBook.CreateCellStyle();  //首先建单元格格式 
            style_Data_Menu(cellStyle1_data_menu);
            ICellStyle cellStyle1_menu = workBook.CreateCellStyle();  //首先建单元格格式 
            style_Menu(cellStyle1_menu);

            //line1
            for (int i = 0; i < lines.Count; i++)
            {

                if (newSheet.GetRow(2 + i) == null)
                {
                    newSheet.CreateRow(2 + i);
                }
                newSheet.GetRow(2 + i).CreateCell(ROW).CellStyle = cellStyle_data;
                newSheet.GetRow(2 + i).GetCell(ROW).SetCellValue(i.ToString());
            }
            //line2
            for (int i = 0; i < lines.Count; i++)
            {

                if (newSheet.GetRow(2 + i) == null)
                {
                    newSheet.CreateRow(2 + i);
                }
                newSheet.GetRow(2 + i).CreateCell(ROW + 1).CellStyle = cellStyle_data;
                newSheet.GetRow(2 + i).GetCell(ROW + 1).SetCellValue(lines[i].waishe);
            }
            //line3
            for (int i = 0; i < lines.Count; i++)
            {

                if (newSheet.GetRow(2 + i) == null)
                {
                    newSheet.CreateRow(2 + i);
                }
                newSheet.GetRow(2 + i).CreateCell(ROW + 2).CellStyle = cellStyle_data;
                newSheet.GetRow(2 + i).GetCell(ROW + 2).SetCellValue(lines[i].current_all.ToString());
            }
            //line4
            for (int i = 0; i < lines.Count; i++)
            {

                if (newSheet.GetRow(2 + i) == null)
                {
                    newSheet.CreateRow(2 + i);
                }
                newSheet.GetRow(2 + i).CreateCell(ROW + 3).CellStyle = cellStyle_data;
                newSheet.GetRow(2 + i).GetCell(ROW + 3).SetCellValue(lines[i].waishe.ToString());
            }
            //固定标题1
            if (newSheet.GetRow(1) == null)
            {
                newSheet.CreateRow(1);
            }
            newSheet.GetRow(1).CreateCell(ROW + 0).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 0).SetCellValue("Index");
            newSheet.GetRow(1).CreateCell(ROW + 1).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 1).SetCellValue("外设");
            newSheet.GetRow(1).CreateCell(ROW + 2).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 2).SetCellValue("总功耗");
            newSheet.GetRow(1).CreateCell(ROW + 3).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 3).SetCellValue("外设功耗");

            //合并单元格(表头)
            if (newSheet.GetRow(0) == null)
            {
                newSheet.CreateRow(0);
            }
            ICell cell = newSheet.GetRow(0).CreateCell(ROW);
            CellRangeAddress mergedCell1 = new CellRangeAddress(0, 0, ROW, ROW + 3);
            newSheet.AddMergedRegion(mergedCell1);
            cell.CellStyle = cellStyle1_menu;
            cell.SetCellValue(MENU_string);
            // 写入文件
            Save_xlsx(file_name, workBook);
            ClearMemoryInfo.FlushMemory();
        }


        public static void ADD_ONE_EXCEL_ADC(string file_name, string sheet_name, string MENU_string, float[] Line1, float[] Line2, float[] Line3)
        {
            XSSFWorkbook workBook = new XSSFWorkbook();
            XSSFSheet newSheet = null;
            if (File.Exists(file_name))
            {
                workBook = Open_xlsx(file_name);
                newSheet = (XSSFSheet)workBook.GetSheet(sheet_name) == null ? (XSSFSheet)workBook.CreateSheet(sheet_name) : (XSSFSheet)workBook.GetSheet(sheet_name);
            }
            else
            {
                workBook = Creat_xlsx(file_name);
                newSheet = (XSSFSheet)workBook.CreateSheet(sheet_name);
            }
            //把ROW算出来
            int ROW = 0;
            while (true)
            {
                if (newSheet.GetRow(1) == null)
                {
                    newSheet.CreateRow(1);
                }
                if (newSheet.GetRow(1).GetCell(ROW) == null)
                {
                    break;
                }
                ROW = ROW + 5;
            }
            //单元格颜色
            ICellStyle cellStyle_data = workBook.CreateCellStyle();  //首先建单元格格式
            style_Data(cellStyle_data);
            ICellStyle cellStyle1_data_menu = workBook.CreateCellStyle();  //首先建单元格格式 
            style_Data_Menu(cellStyle1_data_menu);
            ICellStyle cellStyle1_menu = workBook.CreateCellStyle();  //首先建单元格格式 
            style_Menu(cellStyle1_menu);

            //计算Line4和 MAX MIN AVG
            float[] Line4 = new float[Line3.Length];
            float MAX, MIN, AVG, MAX_MIN;
            for (int i = 0; i < Line4.Length; i++)
            {
                Line4[i] = (Line2[i] - Line1[i]) * 1000;
            }
            MAX = Line4.Max();
            MIN = Line4.Min();
            AVG = Line4.Average();
            MAX_MIN = MAX - MIN;

            //line1
            for (int i = 0; i < Line1.Length; i++)
            {

                if (newSheet.GetRow(5 + i) == null)
                {
                    newSheet.CreateRow(5 + i);
                }
                newSheet.GetRow(5 + i).CreateCell(ROW).CellStyle = cellStyle_data;
                newSheet.GetRow(5 + i).GetCell(ROW).SetCellValue(Line1[i]);
            }
            //line2
            for (int i = 0; i < Line2.Length; i++)
            {

                if (newSheet.GetRow(5 + i) == null)
                {
                    newSheet.CreateRow(5 + i);
                }
                newSheet.GetRow(5 + i).CreateCell(ROW + 1).CellStyle = cellStyle_data;
                newSheet.GetRow(5 + i).GetCell(ROW + 1).SetCellValue(Line2[i]);
            }
            //line3
            for (int i = 0; i < Line3.Length; i++)
            {

                if (newSheet.GetRow(5 + i) == null)
                {
                    newSheet.CreateRow(5 + i);
                }
                newSheet.GetRow(5 + i).CreateCell(ROW + 2).CellStyle = cellStyle_data;
                newSheet.GetRow(5 + i).GetCell(ROW + 2).SetCellValue(Line3[i]);
            }
            //line4
            for (int i = 0; i < Line3.Length; i++)
            {

                if (newSheet.GetRow(5 + i) == null)
                {
                    newSheet.CreateRow(5 + i);
                }
                newSheet.GetRow(5 + i).CreateCell(ROW + 3).CellStyle = cellStyle_data;
                newSheet.GetRow(5 + i).GetCell(ROW + 3).SetCellValue((Line4[i]));
            }
            //固定标题1
            if (newSheet.GetRow(1) == null)
            {
                newSheet.CreateRow(1);
            }
            if (newSheet.GetRow(2) == null)
            {
                newSheet.CreateRow(2);
            }
            newSheet.GetRow(1).CreateCell(ROW + 0).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 0).SetCellValue("MIN");
            newSheet.GetRow(1).CreateCell(ROW + 1).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 1).SetCellValue("MAX");
            newSheet.GetRow(1).CreateCell(ROW + 2).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 2).SetCellValue("AVG");
            newSheet.GetRow(1).CreateCell(ROW + 3).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(1).GetCell(ROW + 3).SetCellValue("△");

            newSheet.GetRow(2).CreateCell(ROW + 0).CellStyle = cellStyle_data;
            newSheet.GetRow(2).GetCell(ROW + 0).SetCellValue(MIN);
            newSheet.GetRow(2).CreateCell(ROW + 1).CellStyle = cellStyle_data;
            newSheet.GetRow(2).GetCell(ROW + 1).SetCellValue(MAX);
            newSheet.GetRow(2).CreateCell(ROW + 2).CellStyle = cellStyle_data;
            newSheet.GetRow(2).GetCell(ROW + 2).SetCellValue(AVG);
            newSheet.GetRow(2).CreateCell(ROW + 3).CellStyle = cellStyle_data;
            newSheet.GetRow(2).GetCell(ROW + 3).SetCellValue(MAX_MIN);
            //固定标题2
            if (newSheet.GetRow(4) == null)
            {
                newSheet.CreateRow(4);
            }
            newSheet.GetRow(4).CreateCell(ROW + 0).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(4).GetCell(ROW + 0).SetCellValue("期望电压值(V)");
            newSheet.GetRow(4).CreateCell(ROW + 1).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(4).GetCell(ROW + 1).SetCellValue("量化电压值(V)");
            newSheet.GetRow(4).CreateCell(ROW + 2).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(4).GetCell(ROW + 2).SetCellValue("ADC Code");
            newSheet.GetRow(4).CreateCell(ROW + 3).CellStyle = cellStyle1_data_menu;
            newSheet.GetRow(4).GetCell(ROW + 3).SetCellValue("△(mV)");
            //计算总表

            //合并单元格(表头)
            if (newSheet.GetRow(0) == null)
            {
                newSheet.CreateRow(0);
            }
            ICell cell = newSheet.GetRow(0).CreateCell(ROW);
            CellRangeAddress mergedCell1 = new CellRangeAddress(0, 0, ROW, ROW + 3);
            newSheet.AddMergedRegion(mergedCell1);
            cell.CellStyle = cellStyle1_menu;
            cell.SetCellValue(MENU_string);
            // 写入文件
            Save_xlsx(file_name, workBook);
            ClearMemoryInfo.FlushMemory();
        }
        public static bool ADD_ONE_EXCEL_Calib_C27(string file_name, C27_Calib_struct struct_in, string MENU_string)
        {
            try
            {
                string sheet_name = "Calib_1";
                string sheet_name2 = "Calib_2";
                XSSFWorkbook workBook = new XSSFWorkbook();
                XSSFSheet newSheet = null;
                XSSFSheet newSheet2 = null;
                if (File.Exists(file_name))
                {
                    workBook = Open_xlsx(file_name);
                    newSheet = (XSSFSheet)workBook.GetSheet(sheet_name) == null ? (XSSFSheet)workBook.CreateSheet(sheet_name) : (XSSFSheet)workBook.GetSheet(sheet_name);
                    newSheet2 = (XSSFSheet)workBook.GetSheet(sheet_name2) == null ? (XSSFSheet)workBook.CreateSheet(sheet_name2) : (XSSFSheet)workBook.GetSheet(sheet_name2);
                }
                else
                {
                    workBook = Creat_xlsx(file_name);
                    newSheet = (XSSFSheet)workBook.CreateSheet(sheet_name);
                    newSheet2 = (XSSFSheet)workBook.CreateSheet(sheet_name2);
                }
                if (newSheet.GetRow(1) == null)
                {
                    newSheet.CreateRow(1);
                }
                if (newSheet2.GetRow(1) == null)
                {
                    newSheet2.CreateRow(1);
                }


                //把ROW算出来
                int ROW = 2;
                while (true)
                {
                    if (newSheet.GetRow(ROW) == null)
                    {
                        newSheet.CreateRow(ROW);
                    }
                    if (newSheet.GetRow(ROW).GetCell(0) == null)
                    {
                        break;
                    }
                    ROW = ROW + 1;
                }
                //把ROW2算出来
                int ROW2 = 2;
                while (true)
                {
                    if (newSheet2.GetRow(ROW2) == null)
                    {
                        newSheet2.CreateRow(ROW2);
                    }
                    if (newSheet2.GetRow(ROW2).GetCell(0) == null)
                    {
                        break;
                    }
                    ROW2 = ROW2 + 1;
                }

                //单元格颜色
                ICellStyle cellStyle_data = workBook.CreateCellStyle();  //首先建单元格格式
                style_Data(cellStyle_data);

                ICellStyle cellStyle1_data_menu = workBook.CreateCellStyle();  //首先建单元格格式 
                style_Data_Menu(cellStyle1_data_menu);

                ICellStyle cellStyle1_menu = workBook.CreateCellStyle();  //首先建单元格格式 
                style_Menu(cellStyle1_menu);
                //Sheet1的格式
                newSheet.GetRow(1).CreateCell(0).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(0).SetCellValue("编号");
                newSheet.GetRow(1).CreateCell(1).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(1).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(2).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(2).SetCellValue("1P1V");
                newSheet.GetRow(1).CreateCell(3).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(3).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(4).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(4).SetCellValue("LPLDO输出电压");
                newSheet.GetRow(1).CreateCell(5).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(5).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(6).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(6).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(7).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(7).SetCellValue("HSI频率");
                newSheet.GetRow(1).CreateCell(8).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(8).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(9).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(9).SetCellValue("HSI14频率");
                newSheet.GetRow(1).CreateCell(10).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(10).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(11).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(11).SetCellValue("LSI频率");
                //Sheet2的格式
                newSheet2.GetRow(1).CreateCell(0).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(0).SetCellValue("编号");
                newSheet2.GetRow(1).CreateCell(1).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(1).SetCellValue("校准数据");
                newSheet2.GetRow(1).CreateCell(2).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(2).SetCellValue("ADC量化");
                newSheet2.GetRow(1).CreateCell(3).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(3).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(4).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(4).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(5).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(5).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(6).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(6).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(7).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(7).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(8).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(8).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(9).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(9).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(10).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(10).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(11).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(11).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(12).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(12).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(13).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(13).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(14).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(14).SetCellValue("PMOS");

                for (int i = 0; i < 12; i++)
                {
                    newSheet.GetRow(ROW).CreateCell(i).CellStyle = cellStyle_data;
                }
                for (int i = 0; i < 15; i++)
                {
                    newSheet2.GetRow(ROW2).CreateCell(i).CellStyle = cellStyle_data;
                }

                //插入数据Sheet1
                newSheet.GetRow(ROW).GetCell(0).SetCellValue(MENU_string);
                newSheet.GetRow(ROW).GetCell(1).SetCellValue(struct_in.BGO);
                newSheet.GetRow(ROW).GetCell(2).SetCellValue(struct_in.LPLV_M);
                newSheet.GetRow(ROW).GetCell(3).SetCellValue(struct_in.LPLDO);
                newSheet.GetRow(ROW).GetCell(4).SetCellValue(struct_in.LPLDO_M);
                newSheet.GetRow(ROW).GetCell(5).SetCellValue(struct_in.TEMP);
                newSheet.GetRow(ROW).GetCell(6).SetCellValue(struct_in.HSI);
                newSheet.GetRow(ROW).GetCell(7).SetCellValue(struct_in.HSI_M);
                newSheet.GetRow(ROW).GetCell(8).SetCellValue(struct_in.HSI14);
                newSheet.GetRow(ROW).GetCell(9).SetCellValue(struct_in.HSI14_M);
                newSheet.GetRow(ROW).GetCell(10).SetCellValue(struct_in.LSI);
                newSheet.GetRow(ROW).GetCell(11).SetCellValue(struct_in.LSI_M);
                //插入数据Sheet2
                newSheet2.GetRow(ROW2).GetCell(0).SetCellValue(MENU_string);
                newSheet2.GetRow(ROW2).GetCell(1).SetCellValue(struct_in.ADC);
                newSheet2.GetRow(ROW2).GetCell(2).SetCellValue(struct_in.ADC_CODE);
                newSheet2.GetRow(ROW2).GetCell(3).SetCellValue(struct_in.OPA[0]);
                newSheet2.GetRow(ROW2).GetCell(4).SetCellValue(struct_in.OPA[1]);
                newSheet2.GetRow(ROW2).GetCell(5).SetCellValue(struct_in.OPA[2]);
                newSheet2.GetRow(ROW2).GetCell(6).SetCellValue(struct_in.OPA[3]);
                newSheet2.GetRow(ROW2).GetCell(7).SetCellValue(struct_in.OPA[4]);
                newSheet2.GetRow(ROW2).GetCell(8).SetCellValue(struct_in.OPA[5]);
                newSheet2.GetRow(ROW2).GetCell(9).SetCellValue(struct_in.OPA[6]);
                newSheet2.GetRow(ROW2).GetCell(10).SetCellValue(struct_in.OPA[7]);
                newSheet2.GetRow(ROW2).GetCell(11).SetCellValue(struct_in.OPA[8]);
                newSheet2.GetRow(ROW2).GetCell(12).SetCellValue(struct_in.OPA[9]);
                newSheet2.GetRow(ROW2).GetCell(13).SetCellValue(struct_in.OPA[10]);
                newSheet2.GetRow(ROW2).GetCell(14).SetCellValue(struct_in.OPA[11]);
                Save_xlsx(file_name, workBook);
                ClearMemoryInfo.FlushMemory();
            }
            catch (Exception ex)
            {

                return false;
            }
            return true;
        }
        public static bool ADD_ONE_EXCEL_Calib_D16(string file_name, D16_Calib_struct struct_in, string MENU_string)
        {
            try
            {
                string sheet_name = "Calib_1";
                string sheet_name2 = "Calib_2";
                XSSFWorkbook workBook = new XSSFWorkbook();
                XSSFSheet newSheet = null;
                XSSFSheet newSheet2 = null;
                if (File.Exists(file_name))
                {
                    workBook = Open_xlsx(file_name);
                    newSheet = (XSSFSheet)workBook.GetSheet(sheet_name) == null ? (XSSFSheet)workBook.CreateSheet(sheet_name) : (XSSFSheet)workBook.GetSheet(sheet_name);
                    newSheet2 = (XSSFSheet)workBook.GetSheet(sheet_name2) == null ? (XSSFSheet)workBook.CreateSheet(sheet_name2) : (XSSFSheet)workBook.GetSheet(sheet_name2);
                }
                else
                {
                    workBook = Creat_xlsx(file_name);
                    newSheet = (XSSFSheet)workBook.CreateSheet(sheet_name);
                    newSheet2 = (XSSFSheet)workBook.CreateSheet(sheet_name2);
                }
                if (newSheet.GetRow(1) == null)
                {
                    newSheet.CreateRow(1);
                }
                if (newSheet2.GetRow(1) == null)
                {
                    newSheet2.CreateRow(1);
                }


                //把ROW算出来
                int ROW = 2;
                while (true)
                {
                    if (newSheet.GetRow(ROW) == null)
                    {
                        newSheet.CreateRow(ROW);
                    }
                    if (newSheet.GetRow(ROW).GetCell(0) == null)
                    {
                        break;
                    }
                    ROW = ROW + 1;
                }
                //把ROW2算出来
                int ROW2 = 2;
                while (true)
                {
                    if (newSheet2.GetRow(ROW2) == null)
                    {
                        newSheet2.CreateRow(ROW2);
                    }
                    if (newSheet2.GetRow(ROW2).GetCell(0) == null)
                    {
                        break;
                    }
                    ROW2 = ROW2 + 1;
                }

                //单元格颜色
                ICellStyle cellStyle_data = workBook.CreateCellStyle();  //首先建单元格格式
                style_Data(cellStyle_data);

                ICellStyle cellStyle1_data_menu = workBook.CreateCellStyle();  //首先建单元格格式 
                style_Data_Menu(cellStyle1_data_menu);

                ICellStyle cellStyle1_menu = workBook.CreateCellStyle();  //首先建单元格格式 
                style_Menu(cellStyle1_menu);
                //Sheet1的格式
                newSheet.GetRow(1).CreateCell(0).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(0).SetCellValue("编号");
                newSheet.GetRow(1).CreateCell(1).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(1).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(2).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(2).SetCellValue("LDO11电压");
                newSheet.GetRow(1).CreateCell(3).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(3).SetCellValue("室温ADC量化");
                newSheet.GetRow(1).CreateCell(4).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(4).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(5).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(5).SetCellValue("LPLDO电压");
                newSheet.GetRow(1).CreateCell(6).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(6).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(7).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(7).SetCellValue("HSI频率");
                newSheet.GetRow(1).CreateCell(8).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(8).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(9).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(9).SetCellValue("HSI14频率");
                newSheet.GetRow(1).CreateCell(10).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(10).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(11).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(11).SetCellValue("LSI频率");
                newSheet.GetRow(1).CreateCell(12).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(12).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(13).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(13).SetCellValue("LSI频率");
                newSheet.GetRow(1).CreateCell(14).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(14).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(15).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(15).SetCellValue("LSI频率");
                newSheet.GetRow(1).CreateCell(16).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(16).SetCellValue("校准数据");
                newSheet.GetRow(1).CreateCell(17).CellStyle = cellStyle1_data_menu;
                newSheet.GetRow(1).GetCell(17).SetCellValue("LSI频率");
                //Sheet2的格式
                newSheet2.GetRow(1).CreateCell(0).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(0).SetCellValue("编号");
                newSheet2.GetRow(1).CreateCell(1).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(1).SetCellValue("校准数据");
                newSheet2.GetRow(1).CreateCell(2).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(2).SetCellValue("ADC量化");
                newSheet2.GetRow(1).CreateCell(3).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(3).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(4).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(4).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(5).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(5).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(6).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(6).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(7).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(7).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(8).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(8).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(9).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(9).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(10).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(10).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(11).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(11).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(12).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(12).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(13).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(13).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(14).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(14).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(15).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(15).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(16).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(16).SetCellValue("NMOS");
                newSheet2.GetRow(1).CreateCell(17).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(17).SetCellValue("PMOS");
                newSheet2.GetRow(1).CreateCell(18).CellStyle = cellStyle1_data_menu;
                newSheet2.GetRow(1).GetCell(18).SetCellValue("PMOS");

                for (int i = 0; i < 12; i++)
                {
                    newSheet.GetRow(ROW).CreateCell(i).CellStyle = cellStyle_data;
                }
                for (int i = 0; i < 15; i++)
                {
                    newSheet2.GetRow(ROW2).CreateCell(i).CellStyle = cellStyle_data;
                }

                //插入数据Sheet1
                newSheet.GetRow(ROW).GetCell(0).SetCellValue(MENU_string);
                newSheet.GetRow(ROW).GetCell(1).SetCellValue(struct_in.BGO);
                newSheet.GetRow(ROW).GetCell(2).SetCellValue(struct_in.LPLV_M);
                newSheet.GetRow(ROW).GetCell(3).SetCellValue(struct_in.LPLDO);
                newSheet.GetRow(ROW).GetCell(4).SetCellValue(struct_in.LPLDO_M);
                newSheet.GetRow(ROW).GetCell(5).SetCellValue(struct_in.TEMP);
                newSheet.GetRow(ROW).GetCell(6).SetCellValue(struct_in.HSI);
                newSheet.GetRow(ROW).GetCell(7).SetCellValue(struct_in.HSI_M);
                newSheet.GetRow(ROW).GetCell(8).SetCellValue(struct_in.HSI14);
                newSheet.GetRow(ROW).GetCell(9).SetCellValue(struct_in.HSI14_M);
                newSheet.GetRow(ROW).GetCell(10).SetCellValue(struct_in.LSI);
                newSheet.GetRow(ROW).GetCell(11).SetCellValue(struct_in.LSI_M);
                //插入数据Sheet2
                newSheet2.GetRow(ROW2).GetCell(0).SetCellValue(MENU_string);
                newSheet2.GetRow(ROW2).GetCell(1).SetCellValue(struct_in.ADC);
                newSheet2.GetRow(ROW2).GetCell(2).SetCellValue(struct_in.ADC_CODE);
                newSheet2.GetRow(ROW2).GetCell(3).SetCellValue(struct_in.OPA[0]);
                newSheet2.GetRow(ROW2).GetCell(4).SetCellValue(struct_in.OPA[1]);
                newSheet2.GetRow(ROW2).GetCell(5).SetCellValue(struct_in.OPA[2]);
                newSheet2.GetRow(ROW2).GetCell(6).SetCellValue(struct_in.OPA[3]);
                newSheet2.GetRow(ROW2).GetCell(7).SetCellValue(struct_in.OPA[4]);
                newSheet2.GetRow(ROW2).GetCell(8).SetCellValue(struct_in.OPA[5]);
                newSheet2.GetRow(ROW2).GetCell(9).SetCellValue(struct_in.OPA[6]);
                newSheet2.GetRow(ROW2).GetCell(10).SetCellValue(struct_in.OPA[7]);
                newSheet2.GetRow(ROW2).GetCell(11).SetCellValue(struct_in.OPA[8]);
                newSheet2.GetRow(ROW2).GetCell(12).SetCellValue(struct_in.OPA[9]);
                newSheet2.GetRow(ROW2).GetCell(13).SetCellValue(struct_in.OPA[10]);
                newSheet2.GetRow(ROW2).GetCell(14).SetCellValue(struct_in.OPA[11]);
                Save_xlsx(file_name, workBook);
                ClearMemoryInfo.FlushMemory();
            }
            catch (Exception ex)
            {

                return false;
            }
            return true;
        }

        //Create file
        public static XSSFWorkbook Creat_xlsx(string File_name)
        {
            XSSFWorkbook workBook1 = new XSSFWorkbook();
            FileStream stream1 = new FileStream(File_name, FileMode.OpenOrCreate);
            workBook1.Write(stream1);
            stream1.Close();
            return workBook1;
        }
        //open file
        public static XSSFWorkbook Open_xlsx(string File_name)
        {
            FileStream stream1 = File.OpenRead(File_name);
            stream1.Position = 0;
            XSSFWorkbook workbook1 = new XSSFWorkbook(stream1);
            stream1.Close();
            return workbook1;
        }
        //save file
        public static void Save_xlsx(string File_name, XSSFWorkbook workBook)
        {
            FileStream stream1 = File.OpenWrite(File_name);
            stream1.Position = 0;
            SXSSFWorkbook workBook2 = new SXSSFWorkbook(workBook, 5000);
            workBook2.Write(stream1);
            workBook2.Close();
            workBook.Close();
            stream1.Close();
        }
        //获取这一列某个cell
        public ICell GetCell(IRow Row, int index)
        {
            ICell cell = Row.FirstOrDefault(n => n.ColumnIndex == index);
            if (cell == null)
            {
                cell = Row.CreateCell(index);
            }
            return cell;
        }
        //数据单元格常规格式
        public static ICellStyle style_Data(ICellStyle cellStyle)
        {
            //设置单元格上下左右边框线
            cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;  //虚线
            cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;//粗线  
            cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;//双线  
            cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;//细线
            //文字水平和垂直对齐方式  
            cellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            cellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Top;
            //是否换行  
            //cellStyle.WrapText = true;  //若字符串过大换行填入单元格
            //缩小字体填充  
            /* cellStyle.ShrinkToFit = true;//若字符串过大缩小字体后填入单元*/
            // 新建工作簿对象
            //cellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.PaleBlue.Index;
            //cellStyle.FillPattern = FillPattern.SolidForeground;
            return cellStyle;
        }

        //数据标题常规格式
        public static ICellStyle style_Data_Menu(ICellStyle cellStyle)
        {

            //设置单元格上下左右边框线
            cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;  //虚线
            cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;//粗线  
            cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;//双线  
            cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;//细线                                                               
            //文字水平和垂直对齐方式  
            cellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            cellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Top;
            //是否换行  
            //cellStyle.WrapText = true;  //若字符串过大换行填入单元格
            //cellStyle.FillPattern
            //缩小字体填充  
            //cellStyle.ShrinkToFit = true;//若字符串过大缩小字体后填入单元格
            // 新建工作簿对象

            cellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.PaleBlue.Index;
            cellStyle.FillPattern = FillPattern.SolidForeground;
            return cellStyle;
        }
        //主标题常规格式
        public static ICellStyle style_Menu(ICellStyle cellStyle)
        {

            //设置单元格上下左右边框线
            cellStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;  //虚线
            cellStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;//粗线  
            cellStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;//双线  
            cellStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;//细线                                                               
            //文字水平和垂直对齐方式  
            cellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            cellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Top;
            //是否换行  
            //cellStyle.WrapText = true;  //若字符串过大换行填入单元格
            //缩小字体填充  
            //cellStyle.ShrinkToFit = true;//若字符串过大缩小字体后填入单元格
            // 新建工作簿对象
            cellStyle.FillForegroundColor = 22;
            cellStyle.FillPattern = FillPattern.SolidForeground;
            return cellStyle;
        }

    }
}
