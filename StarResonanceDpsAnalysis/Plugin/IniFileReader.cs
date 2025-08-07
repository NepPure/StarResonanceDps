using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Plugin
{
    public class IniFileReader
    {
        public Dictionary<string, Dictionary<string, string>> sections;

        public IniFileReader()
        {
            sections = new Dictionary<string, Dictionary<string, string>>();
        }

        public void Load(string filePath)
        {
            sections.Clear();

            string currentSection = "";
            Dictionary<string, string> currentSectionData = null;
            if (!System.IO.File.Exists(filePath))
            {
                return;
            }
            foreach (string line in File.ReadLines(filePath))
            {
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                {
                    // This line is a comment, ignore it
                    continue;
                }
                else if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    // This line defines a new section
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    currentSectionData = new Dictionary<string, string>();
                    sections[currentSection] = currentSectionData;
                }
                else if (currentSectionData != null && trimmedLine.Contains("="))
                {
                    // This line contains a key-value pair
                    int separatorIndex = trimmedLine.IndexOf('=');
                    string key = trimmedLine.Substring(0, separatorIndex).Trim();
                    string value = trimmedLine.Substring(separatorIndex + 1).Trim();
                    currentSectionData[key] = value;
                }
            }
        }

        public List<string> GetSections()
        {
            List<string> sectionList = new List<string>(sections.Keys);
            return sectionList;
        }
        public Dictionary<string, string> GetSectionData(string section)
        {
            if (sections.ContainsKey(section))
            {
                return sections[section];
            }

            return null;
        }

        public string GetValue(string section, string key)
        {
            if (sections.ContainsKey(section))
            {
                Dictionary<string, string> sectionData = sections[section];
                if (sectionData.ContainsKey(key))
                {
                    return sectionData[key];
                }
            }

            return null;
        }
        public void SaveValue(string section, string key, string value)
        {
            if (sections.ContainsKey(section))
            {
                Dictionary<string, string> sectionData = sections[section];
                sectionData[key] = value;
            }
            else
            {
                Dictionary<string, string> newSectionData = new Dictionary<string, string>();
                newSectionData[key] = value;
                sections[section] = newSectionData;
            }
        }

        public void Save(string filePath)
        {
            // 检查文件是否存在，如果不存在则创建文件
            if (!File.Exists(filePath))
            {
                try
                {
                    using (FileStream fs = File.Create(filePath))
                    {
                        Console.WriteLine("文件已创建。");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"创建文件时出错: {ex.Message}");
                }
            }
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> section in sections)
                {
                    writer.WriteLine($"[{section.Key}]");
                    foreach (KeyValuePair<string, string> keyValue in section.Value)
                    {
                        writer.WriteLine($"{keyValue.Key}={keyValue.Value}");
                    }
                    writer.WriteLine();
                }
            }
        }

    }
}
