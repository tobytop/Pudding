using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pudding.Core.Tool
{
    public static class ExcelExtension
    {
        /// <summary>
        /// list导出excel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">数据</param>
        /// <param name="header">导出的头部</param>
        /// <param name="dir">导出的物理目录</param>
        /// <returns></returns>
        public static string Export<T>(this IEnumerable<T> data, IDictionary<string, string> header, string dir) where T : class
        {
            HSSFWorkbook workbook = new HSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("sheet1");

            T t = (T)Activator.CreateInstance(typeof(T));
            PropertyInfo[] propertys = t.GetType().GetProperties();

            int index = 0, commonIndex = 0;

            IRow hsTitleRow = sheet.CreateRow(0);
            ICellStyle headStyle = workbook.CreateCellStyle();
            IFont font = workbook.CreateFont();
            font.FontHeightInPoints = 10;
            font.Boldweight = 700;
            headStyle.SetFont(font);
            headStyle.Alignment = HorizontalAlignment.Center;

            foreach (KeyValuePair<string, string> item in header)
            {
                hsTitleRow.CreateCell(index).SetCellValue(item.Key);
                hsTitleRow.GetCell(index++).CellStyle = headStyle;
            }

            index = 1;
            foreach (T item in data)
            {
                IRow hsBodyRow = sheet.CreateRow(index++);
                foreach (KeyValuePair<string, string> title in header)
                {
                    PropertyInfo pi = propertys.First(o => o.Name.Equals(title.Value, StringComparison.CurrentCultureIgnoreCase));
                    string value = (pi.GetValue(item, null) ?? "").ToString();
                    hsBodyRow.CreateCell(commonIndex).SetCellValue(value);

                    int length = (Encoding.GetEncoding(936).GetBytes(value).Length + 1) * 256;
                    if (sheet.GetColumnWidth(commonIndex++) < length)
                    {
                        sheet.SetColumnWidth(commonIndex - 1, length);
                    }
                }
                commonIndex = 0;
            }

            string filename = Guid.NewGuid().ToString() + ".xlsx";
            string path = AppDomain.CurrentDomain.BaseDirectory + dir;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            using (FileStream myFs = new FileStream(path + "\\" + filename, FileMode.Create))
            {
                workbook.Write(myFs);
                return dir.Replace("\\", "/") + "/" + filename;
            }
        }
    }
}
