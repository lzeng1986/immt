using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using LazyBones.Log.Config;
using LazyBones.Log.Core;
using LazyBones.Log.Renderers;

namespace LazyBones.Log.Layouts
{
    static class LayoutParser
    {
        static Regex rendererRegex = new Regex(@"{(?<name>/w+)(:(?<value>)/w+)?}");

        //分析渲染器的配置文本，生成对应的渲染器集合
        public static Renderer[] Parse(TextReader reader, LogConfigItemFactory logItemFactory)
        {
            //渲染器的配置文本格式如下
            //'{rendererName:param = value|param = value|...}'
            //'{rendererName}'
            //其它所有非本格式的文本均自动转化成LabelRenderer            
            var renderers = new List<Renderer>();
            var textBuf = new StringBuilder();
            while (reader.Peek() >= 0)
            {
                var ch = reader.Read();
                if (ch == '{')//'{rendererName}'
                {
                    if (textBuf.Length > 0)
                    {
                        renderers.Add(new LabelRenderer { Text = textBuf.ToString() });
                        textBuf.Length = 0;
                    }
                    var rendererName = ParseRendererName(reader);
                    var renderer = logItemFactory.Renderers.GetInstance(rendererName);
                    Helper.ResetPropertyValue(renderer, logItemFactory);
                    ParseRendererProperty(reader, renderer, logItemFactory);
                    renderers.Add(renderer);
                    if (reader.Peek() == '}')
                    {
                        reader.Read();//跳过‘}’
                    }
                }
                else
                {
                    textBuf.Append((char)ch);
                }
            }
            if (textBuf.Length > 0)
            {
                renderers.Add(new LabelRenderer { Text = textBuf.ToString() });
                textBuf.Length = 0;
            }
            MergeLabelRenderer(renderers);
            return renderers.ToArray();
        }
        static string ParseRendererName(TextReader reader)
        {
            //'rendererName'
            var sb = new StringBuilder();
            var ch = reader.Peek();
            while (ch != -1 && ch != '}' && ch != ':')
            {
                sb.Append((char)reader.Read());
                ch = reader.Peek();
            }
            return sb.ToString().Trim();
        }
        //从configItemFactory创建一个渲染器对象，并根据配置文件对其属性进行赋值
        static void ParseRendererProperty(TextReader reader, Renderer renderer, LogConfigItemFactory logItemFactory)
        {
            var ch = reader.Peek();
            if (ch == ':')
            {
                reader.Read();//跳过":"
                while ((ch = reader.Peek()) != -1 && ch != '}')
                {
                    var param = ParseParam(reader);
                    var value = ParseValue(reader);
                    PropertyInfo pi = Helper.GetPropertyInfo(renderer, param);
                    if (pi != null)
                        Helper.SetPropertyFromString(renderer, param, value, logItemFactory);
                }
            }
        }
        static string ParseParam(TextReader reader)
        {
            var param = new StringBuilder();            
            while (reader.Peek() >= 0)
            {
                var ch = reader.Read();
                if (ch == '=')
                    break;
                param.Append((char)ch);
            }
            return param.ToString().Trim();
        }
        static string ParseValue(TextReader reader)
        {
            var value = new StringBuilder();
            var ch = reader.Peek();
            while (ch != -1 && ch != '}')
            {
                ch = reader.Read();
                if (ch == '|')
                    break;
                value.Append((char)ch);
                ch = reader.Peek();
            }
            return value.ToString();
        }
        static void MergeLabelRenderer(IList<Renderer> renderers) //将相邻的LabelRenderer合并为一个
        {
            for (var i = 0; i + 1 < renderers.Count; )
            {
                var current = renderers[i] as LabelRenderer;
                if (current != null)
                {
                    var next = renderers[i + 1] as LabelRenderer;
                    if (next != null)
                    {
                        current.Text += next.Text;
                        renderers.RemoveAt(i + 1);
                    }
                    else
                    {
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }
        }
    }
}
