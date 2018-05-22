using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MicroGalaxy.Utility
{
    /// <summary>
    /// 反射工具--（暂时未写类构造入参的外部方法，但是支持类有不定数量参数构造）
    /// </summary>
    public static class AssemblyUtility
    {
        /// <summary>
        /// 反射调用Dll中继承自TInterface的第一个方法
        /// </summary>
        /// <typeparam name="TInterface">目标接口</typeparam>
        /// <typeparam name="Tresult">接口返回类型</typeparam>
        /// <param name="dllFileName">DLL文件名（不含路径， 路径默认拼接Web站点Bin目录路径）</param>
        /// <param name="param">方法入参</param>
        /// <returns>接口返回值</returns>
        public static Tresult BindingInterface<TInterface, Tresult>(string dllFileName, params object[] param) where Tresult : class
        {
            return BindingInterface<TInterface, Tresult>(dllFileName, string.Empty, string.Empty, param);
        }
        /// <summary>
        /// 反射调用Dll中继承自TInterface的className类第一个方法
        /// </summary>
        /// <typeparam name="TInterface">目标接口</typeparam>
        /// <typeparam name="Tresult">接口返回类型</typeparam>
        /// <param name="dllFileName">DLL文件名（不含路径， 路径默认拼接Web站点Bin目录路径）</param>
        /// <param name="className">类名称</param>
        /// <param name="param">方法入参</param>
        /// <returns>接口返回值</returns>
        public static Tresult BindingInterface<TInterface, Tresult>(string dllFileName, string className, params object[] param) where Tresult : class
        {
            return BindingInterface<TInterface, Tresult>(dllFileName, className, string.Empty, param);
        }
        /// <summary>
        /// 反射调用Dll中继承自TInterface的className类methodName方法
        /// </summary>
        /// <typeparam name="TInterface">目标接口</typeparam>
        /// <typeparam name="Tresult">接口返回类型</typeparam>
        /// <param name="dllFileName">DLL文件名（不含路径， 路径默认拼接Web站点Bin目录路径）</param>
        /// <param name="className">类名称</param>
        /// <param name="methodName">方法名</param>
        /// <param name="param">方法入参</param>
        /// <returns>接口返回值</returns>
        public static Tresult BindingInterface<TInterface, Tresult>(string dllFileName, string className, string methodName, params object[] param) where Tresult : class
        {
            Assembly dll = JuniorMemoryCache.GetCacheItem<Assembly>(string.Format("{0}_dll_Assembly", dllFileName), () => { return Assembly.LoadFile(GetDllFilePath(dllFileName)); });
            if (string.IsNullOrEmpty(className))
            {
                Type[] dllTypeList = JuniorMemoryCache.GetCacheItem<Type[]>(string.Format("{0}_dll_TypeList", dllFileName), () => { return dll.GetTypes(); });
                foreach (var item in dllTypeList)
                {
                    #region MyRegion
                    Type[] existsInterface = item.FindInterfaces((typeObj, criteriaObj) => { return typeObj.ToString() == criteriaObj.ToString(); }, typeof(TInterface));
                    if (existsInterface != null && existsInterface.Length > 0)
                    {
                        className = item.FullName;

                        if (string.IsNullOrEmpty(className))
                        {
                            return default(Tresult);
                        }

                        return call<TInterface, Tresult>(className, ref methodName, dll, param);
                    }
                    #endregion
                }
                return default(Tresult);

            }
            else
            {
                return call<TInterface, Tresult>(className, ref methodName, dll, param);
            }
        }
        /// <summary>
        /// 反射调用Dll中继承自TInterface的className类methodName方法
        /// </summary>
        /// <typeparam name="TInterface">目标接口</typeparam>
        /// <typeparam name="Tresult">接口返回类型</typeparam>
        /// <param name="className">类名称</param>
        /// <param name="methodName">方法名</param>
        /// <param name="dll">加载Dll的反射对象</param>
        /// <param name="param">接口返回值</param>
        /// <returns>接口返回值</returns>
        private static Tresult call<TInterface, Tresult>(string className, ref string methodName, Assembly dll, params object[] param) where Tresult : class
        {
            object result = null;
            // object handle = dll.CreateInstance(className);//老方法，反射创建实例
            var classType = JuniorMemoryCache.GetCacheItem<Type>(string.Format("{0}_dll_{1}_ClassTypeList", dll.FullName, className), () => { return dll.GetType(className); });
            object handle = classType.CreateInstance(); //新方法，表达式树创建实例(添加缓存)，是反射调用时间的一半。
            if (handle != null && handle is TInterface)
            {
                if (string.IsNullOrEmpty(methodName))
                {
                    Type interfaceType = ((TInterface)handle).GetType().GetInterface(typeof(TInterface).FullName);
                    if (interfaceType != null)
                    {
                        var methodList = JuniorMemoryCache.GetCacheItem<MethodInfo[]>(string.Format("{0}_InterfaceMethods", interfaceType.FullName), () => { return interfaceType.GetMethods(); });
                        if (methodList != null && methodList.Length > 0)
                        {
                            methodName = methodList.FirstOrDefault().Name;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(methodName))
                {
                    try
                    {
                        result = ((TInterface)handle).GetType().InvokeMember(methodName, BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, (TInterface)handle, param);
                    }
                    catch (Exception ex)
                    {

                    }
                    return (Tresult)result;
                }
                else
                {
                    return default(Tresult);
                }
            }
            return default(Tresult);
        }

        public static TInterface CreateInstance<TInterface>(string dllFileName,string className, params object[] args) 
        {
#if DEBUG
            Assembly dll = Assembly.LoadFile(GetDllFilePath(dllFileName));
            var classType = dll.GetType(className);
            object handle = classType.CreateInstance(args);
            if (handle is TInterface)
            {
                return (TInterface)handle;
            }
            else
            {
                return default(TInterface);
            }
#else
            Assembly dll = JuniorMemoryCache.GetCacheItem<Assembly>(string.Format("{0}_dll_Assembly", dllFileName), () => { return Assembly.LoadFile(GetDllFilePath(dllFileName)); });
            var classType = JuniorMemoryCache.GetCacheItem<Type>(string.Format("{0}_dll_{1}_ClassTypeList", dll.FullName, className), () => { return dll.GetType(className); });
            object handle = classType.CreateInstance(args);
            if (handle is TInterface)
            {
                return (TInterface)handle;
            }
            else
            {
                try
                {
                    LogUtility.Info($"dllFileName>{dllFileName}className{className}>args:{string.Join(",", args)}>dllfilepath>{GetDllFilePath(dllFileName)}");
                }
                catch (Exception)
                { 
                }
                return default(TInterface);
            }
#endif
        }

        /// <summary>
        /// 获取Web根目录Bin的地址再拼接Dll文件的地址
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetDllFilePath(string fileName)
        {
            return System.IO.Path.Combine(System.Web.HttpContext.Current.Request.PhysicalApplicationPath, "bin", fileName);
        }


#region CreateInstanceByDelegateOnCache

        /// <summary>
        /// 创建用来返回构造函数的委托
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="parameterTypes">构造函数的参数类型数组</param>
        /// <returns></returns>
        private static Func<object[], object> CreateInstanceDelegate(this Type type, Type[] parameterTypes)
        {
            //根据参数类型数组来获取构造函数
            var constructor = type.GetConstructor(parameterTypes);

            //创建lambda表达式的参数
            var lambdaParam = Expression.Parameter(typeof(object[]), "_args");

            //创建构造函数的参数表达式数组
            var constructorParam = buildParameters(parameterTypes, lambdaParam);

            //创建构造函数表达式
            NewExpression newExp = Expression.New(constructor, constructorParam);

            //创建lambda表达式，返回构造函数
            Expression<Func<object[], object>> lambdaExp =
                Expression.Lambda<Func<object[], object>>(newExp, lambdaParam);

            return lambdaExp.Compile();
        }

        /// <summary>
        /// 根据类型数组和lambda表达式的参数，转化参数成相应类型 
        /// </summary>
        /// <param name="parameterTypes">类型数组</param>
        /// <param name="paramExp">lambda表达式的参数表达式（参数是：object[]）</param>
        /// <returns>构造函数的参数表达式数组</returns>
        private static Expression[] buildParameters(Type[] parameterTypes, ParameterExpression paramExp)
        {
            List<Expression> list = new List<Expression>();
            for (int i = 0; i < parameterTypes.Length; i++)
            {
                //从参数表达式（参数是：object[]）中取出参数
                var arg = BinaryExpression.ArrayIndex(paramExp, Expression.Constant(i));
                //把参数转化成指定类型
                var argCast = Expression.Convert(arg, parameterTypes[i]);

                list.Add(argCast);
            }
            return list.ToArray();
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="args">构造函数的参数列表</param>
        /// <returns></returns>
        public static object CreateInstance(this Type type, params object[] args)
        {
#if DEBUG
            return Activator.CreateInstance(type, args);
#endif
            //从缓存中获取构造函数的委托（注：key是 type 和 parameterTypes）
            return JuniorMemoryCache.GetCacheItem<Func<object[], object>>(string.Format("{0}_TypeCreateInstanceDelegate", type.FullName), () =>
            {
                //根据参数列表返回参数类型数组
                var parameterTypes = args.Select(c => c.GetType()).ToArray();
                return type.CreateInstanceDelegate(parameterTypes);
            })(args);
            //return  type.CreateInstanceDelegate(parameterTypes)(args);

        }


#endregion

        /// <summary>
        /// 获取类型所有属性的Display特性的Name属性值
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetPropertyDisplayAttributeName(this Type type)
        {
            return JuniorMemoryCache.GetCacheItem<Func<Type, Dictionary<string, string>>>(string.Format("{0}_TypeGetPropertyDisplayAttributeName", type.FullName), (t) =>
            {
                Dictionary<string, string> result = new Dictionary<string, string>();
                var properties = t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Instance);
                if (properties != null && properties.Length > 0)
                {
                    foreach (var prop in properties)
                    {
                        var displayName = prop.Name;
                        var propattr = prop.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), true);
                        if (propattr != null && propattr.Length > 0)
                        {
                            displayName = (propattr[0] as System.ComponentModel.DataAnnotations.DisplayAttribute).Name;
                        }
                        result.Add(prop.Name, displayName);
                    }
                }
                return result;
            })(type);
        }
        /// <summary>
        /// 获取类型所有属性的Display特性的Name属性值
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetPropertyDisplayAttributeName(this string typeFullName)
        {
            Type type = Type.GetType(typeFullName);
            return GetPropertyDisplayAttributeName(type);
        }

        /// <summary>
        /// 获取类型的Display特性的Name属性值
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public static string GetClassDisplayAttributeName(this Type type)
        {
            return JuniorMemoryCache.GetCacheItem<Func<Type, string>>(string.Format("{0}_TypeGetClassDisplayAttributeName", type.FullName), (t) =>
            {
                var displayName = type.Name;
                var propattr = type.GetCustomAttributes(typeof(System.ComponentModel.DisplayNameAttribute), true);
                if (propattr != null && propattr.Length > 0)
                {
                    displayName = (propattr[0] as System.ComponentModel.DisplayNameAttribute).DisplayName;
                }
                return displayName;
            })(type);
        }
        /// <summary>
        /// 获取类型的Display特性的Name属性值
        /// </summary>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        public static string GetClassDisplayAttributeName(this string typeFullName)
        {
            Type type = Type.GetType(typeFullName);
            return GetClassDisplayAttributeName(type);
        }

    }
}
