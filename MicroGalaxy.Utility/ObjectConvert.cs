using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroGalaxy.Utility
{
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;
    public static class ObjectConvert
    {
        /// <summary>
        /// 转换数据类型（有时候不稳定，会莫名其妙的抛异常，但是支持自定义的转换，需要的可以修改此方法内部）
        /// </summary>
        /// <typeparam name="T">目标数据类型</typeparam>
        /// <param name="param">变量自己</param>
        /// <returns>转换后的变量</returns>
        public static T To<T>(this object param)
        {
            if (param == null) return default(T);
            Type type = typeof(T);
            try
            {
                return (T)param.ToObject(type);
            }
            catch
            {

            }
            return default(T);
        }
        /// <summary>
        /// 代替Convert或者TryParser的数据类型转换
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type">目标数据类型</param>
        /// <returns></returns>
        public static object ToObject(this object value, Type type)
        {
            if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
            if (value == null) return null;
            if (type == value.GetType()) return value;
            if (type.IsEnum)
            {
                if (value is string)
                    return Enum.Parse(type, value as string);
                else
                    return Enum.ToObject(type, value);
            }
            if (!type.IsInterface && type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = ToObject(value, innerType);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }
            if (value is string && type == typeof(Guid)) return new Guid(value as string);
            if (value is string && type == typeof(Version)) return new Version(value as string);
            if (!(value is IConvertible)) return value;
            return Convert.ChangeType(value.ToString(), type);
        }

        public static DateTime SqlServerMinData(this DateTime val)
        {
            return new DateTime(1753, 1, 1);
        }

        #region 超级无敌小强
        public static T FillEntity<T>(this DataRow row)
        {
            T result = System.Activator.CreateInstance<T>();
            System.Reflection.PropertyInfo[] properties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Instance);
            if (properties != null && properties.Length > 0)
            {
                foreach (PropertyInfo prop in properties)
                {
                    foreach (DataColumn item in row.Table.Columns)
                    {
                        if (item.ColumnName.ToUpper() == prop.Name.ToUpper())
                        {
                            try
                            {
                                prop.SetValue(result, row[item.ColumnName].ToObject(prop.PropertyType), null);
                            }
                            catch (Exception ex)
                            {
                            }
                            continue;
                        }
                    }
                }
            }
            return result;
        }
        public static List<T> FillEntity<T>(this DataTable table) where T : new()
        {
            List<T> result = new List<T>();
            if (table != null && table.Rows.Count > 0)
            {
                foreach (DataRow row in table.Rows)
                {
                    T item = FillEntity<T>(row);
                    result.Add(item);
                }
            }
            return result;
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> obj, string tableName)
        {
            if (obj == null)
            {
                return null;
            }
            //创建属性的集合
            List<PropertyInfo> pList = new List<PropertyInfo>();
            //获得反射的入口
            Type type = typeof(T);
            DataTable dt = new DataTable(tableName);
            //把所有的public属性加入到集合 并添加DataTable的列
            Array.ForEach<PropertyInfo>(type.GetProperties(), p =>
            {
                pList.Add(p);
                Type columnType = p.PropertyType.Name.Equals("Nullable`1") ? p.PropertyType.GenericTypeArguments.FirstOrDefault() : p.PropertyType;
                dt.Columns.Add(p.Name, columnType);
            });
            //foreach (var p in type.GetProperties())
            //{
            //    pList.Add(p);
            //    dt.Columns.Add(p.Name, p.PropertyType);
            //}
            foreach (var item in obj)
            {
                //创建一个DataRow实例
                DataRow row = dt.NewRow();
                //给row 赋值

                pList.ForEach(p =>
                {
                    var value = p.GetValue(item, null);
                    if (value != null)
                    {
                        row[p.Name] = value;
                    }
                });
                //row[p.Name] = (value == null ?  DBNull.Value: value) ; });
                //加入到DataTable
                dt.Rows.Add(row);
            }
            return dt;
        }
        public static DataTable ToDataTable<T>(this IEnumerable<T> obj)
        {
            return obj.ToDataTable("Object");
        }
        #endregion

        #region Serialization
        public static string SerializeToXML(this object obj)
        {
            string result = string.Empty;
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializer xs = new XmlSerializer(obj.GetType());
                xs.Serialize(sw, obj);
                result = sw.ToString();
            }
            return result;
        }
        public static byte[] SerializeToXML(this object obj, Encoding encoding)
        {
            byte[] result = null;
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializer xs = new XmlSerializer(obj.GetType());
                xs.Serialize(sw, obj);
                var str = sw.ToString();
                result = encoding.GetBytes(str);
            }
            return result;
        }
        public static T DesSerializeTo<T>(this string xml) where T : class
        {
            using (MemoryStream ms = new MemoryStream(Encoding.Default.GetBytes(xml)))
            {
                return desSerializeTo<T>(ms);
            }
        }
        public static T DesSerializeTo<T>(this string xml, Encoding encoding) where T : class
        {
            using (MemoryStream ms = new MemoryStream(encoding.GetBytes(xml)))
            {
                return desSerializeTo<T>(ms);
            }
        }
        public static T DesSerializeTo<T>(this byte[] buffer) where T : class
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                return desSerializeTo<T>(ms);
            }
        }

        private static T desSerializeTo<T>(MemoryStream stream) where T : class
        {
            using (MemoryStream ms = stream)
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                T temp = xs.Deserialize(ms) as T;
                return temp;
            }
        }
        #endregion


        #region Json

        public static string ToJson<T>(this T obj)
        {
            var objString = string.Empty;
            try
            {
                Newtonsoft.Json.JsonSerializerSettings jsonSetting = new Newtonsoft.Json.JsonSerializerSettings();
                objString = Newtonsoft.Json.JsonConvert.SerializeObject(obj, typeof(T), jsonSetting);
            }
            catch (Exception ex)
            {

            }
            return objString;
        }

        public static T DesSerializeJson<T>(this string json)
        {
            T jsonobj = default(T);
            try
            {
                Newtonsoft.Json.JsonSerializerSettings jsonSetting = new Newtonsoft.Json.JsonSerializerSettings();
                jsonobj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {

            }
            return jsonobj;
        }
        #endregion
    }

}
