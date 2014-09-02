using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using LazyBones.Extensions;

namespace LazyBones
{
    /// <summary>
    /// 用于读取ini文件，并且可以将ini文件转换为相应的xml文件
    /// </summary>
    [System.Security.SuppressUnmanagedCodeSecurity]
    [Author("曾樑")]
    public class IniFile
    {
        public string IniFilePath { get { return filePath; } }
        public const int MaxSectionSize = 32767; // 32 KB
        private readonly string filePath;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetPrivateProfileSectionNames(IntPtr lpReturnedString, uint nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, IntPtr lpReturnedString, int nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        /// <summary>
        /// 构造函数，根据ini文件路径构建IniFile
        /// </summary>
        /// <param name="iniFilePath">ini文件路径</param>
        public IniFile(string iniFilePath)
        {
            filePath = System.IO.Path.GetFullPath(iniFilePath);
        }

        string GetValue(string sectionName, string keyName, string defaultValue)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");
            if (keyName == null)
                throw new ArgumentNullException("keyName");
            var value = new StringBuilder(IniFile.MaxSectionSize);
            GetPrivateProfileString(sectionName, keyName, defaultValue, value, IniFile.MaxSectionSize, filePath);
            return value.ToString();
        }

        public string GetString(string sectionName, string keyName, string defaultValue)
        {
            return GetValue(sectionName, keyName, defaultValue);
        }
        public Int16 GetInt16(string sectionName, string keyName, Int16 defaultValue)
        {
            var value = GetValue(sectionName, keyName, "");
            if (!value.IsInt16())
            {
                return defaultValue;
            }
            return value.ToInt16();
        }
        public int GetInt32(string sectionName, string keyName, int defaultValue)
        {
            var value = GetValue(sectionName, keyName, "");
            if (!value.IsInt32())
            {
                return defaultValue;
            }
            return value.ToInt32();
        }
        public double GetDouble(string sectionName, string keyName, double defaultValue)
        {
            var value = GetValue(sectionName, keyName, "");
            if (!value.IsDouble())
            {
                return defaultValue;
            }
            return value.ToDouble();
        }
        /// <summary>
        /// 获取指定Section内的值
        /// </summary>
        /// <param name="sectionName">Section名称</param>
        /// <returns></returns>
        public Dictionary<string, string> GetSectionValues(string sectionName)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");
            var ptr = Marshal.AllocCoTaskMem(IniFile.MaxSectionSize);
            var dict = new Dictionary<string, string>();
            try
            {
                var len = GetPrivateProfileSection(sectionName, ptr, MaxSectionSize, filePath);
                var keyValuePairs = ToStringArray(ptr, len);
                foreach (var p in keyValuePairs)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(p, @"^(?<key>.*)=(?<value>.*)$");
                    dict[match.Groups["key"].Value.Trim()] = match.Groups["value"].Value.Trim();
                }
                return dict;
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }
        public string[] GetKeyNames(string sectionName)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");
            var ptr = Marshal.AllocCoTaskMem(IniFile.MaxSectionSize);
            try
            {
                var len = GetPrivateProfileString(sectionName, null, null, ptr, IniFile.MaxSectionSize, filePath);
                return ToStringArray(ptr, len);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }
        public string[] GetSectionNames()
        {
            var ptr = Marshal.AllocCoTaskMem(IniFile.MaxSectionSize);
            try
            {
                var len = GetPrivateProfileSectionNames(ptr, IniFile.MaxSectionSize, filePath);
                return ToStringArray(ptr, len);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }
        private static string[] ToStringArray(IntPtr ptr, int len)
        {
            if (len == 0)
            {
                return new string[0];
            }
            else
            {
                var str = Marshal.PtrToStringAuto(ptr, len - 1);
                return str.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
        private void WriteValueInternal(string sectionName, string keyName, string value)
        {
            if (!WritePrivateProfileString(sectionName, keyName, value, filePath))
            {
                throw new System.ComponentModel.Win32Exception();
            }
        }
        public void WriteValue(string sectionName, string keyName, string value)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");
            if (keyName == null)
                throw new ArgumentNullException("keyName");
            if (value == null)
                throw new ArgumentNullException("value");
            WriteValueInternal(sectionName, keyName, value);
        }

        public void WriteValue(string sectionName, string keyName, short value)
        {
            WriteValue(sectionName, keyName, (int)value);
        }

        public void WriteValue(string sectionName, string keyName, int value)
        {
            WriteValue(sectionName, keyName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteValue(string sectionName, string keyName, float value)
        {
            WriteValue(sectionName, keyName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteValue(string sectionName, string keyName, double value)
        {
            WriteValue(sectionName, keyName, value.ToString(CultureInfo.InvariantCulture));
        }

        public void DeleteKey(string sectionName, string keyName)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");
            if (keyName == null)
                throw new ArgumentNullException("keyName");
            WriteValueInternal(sectionName, keyName, null);
        }
        public void DeleteSection(string sectionName)
        {
            if (sectionName == null)
                throw new ArgumentNullException("sectionName");
            WriteValueInternal(sectionName, null, null);
        }
        /// <summary>
        /// 将ini文件保存为同路径下同名的xml文件
        /// </summary>
        public void SaveAsXml()
        {
            if (filePath.IsNotEmpty())
            {
                var xmlFileName = System.IO.Path.ChangeExtension(filePath, ".xml");
                SaveAsXml(xmlFileName);
            }
        }
        /// <summary>
        /// 将ini文件保存为指定名称的xml文件
        /// </summary>
        public void SaveAsXml(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (!System.IO.Path.GetExtension(path).Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException("保存文件后缀名应为 .xml", "path");
            var xDoc = ToXml();
            xDoc.Save(path);
        }
        /// <summary>
        /// 将ini文件内容转换为对应的xml
        /// </summary>
        public XDocument ToXml()
        {
            var xe = new XElement(System.IO.Path.GetFileNameWithoutExtension(filePath));
            var xDoc = new XDocument(xe);
            var sections = GetSectionNames();
            foreach (var section in sections)
            {
                var sectionXE = new XElement(section);
                var values = GetSectionValues(section).Select(v => new XElement(v.Key, v.Value));
                sectionXE.Add(values.ToArray());
                xe.Add(sectionXE);
            }
            return xDoc;
        }
        public override string ToString()
        {
            return filePath;
        }
    }
}
