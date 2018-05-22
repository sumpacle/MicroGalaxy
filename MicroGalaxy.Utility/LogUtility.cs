using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MicroGalaxy.Utility
{
    /// <summary>
    /// 静态日志方法
    /// </summary>
    public static class LogUtility
    {
        static LogUtility()
        {
            try
            {
                jsonSetting.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore;
                jsonSetting.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                jsonSetting.DateFormatString = "yyyy-MM-dd HH:mm:ss:fff";
            }
            catch (Exception)
            {
                 
            }
        }
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Newtonsoft.Json.JsonSerializerSettings jsonSetting = new Newtonsoft.Json.JsonSerializerSettings();


        public static void Trace(string message ,params object[] args)
        {
            try
            {
                logger.Trace(message, args);
            }
            catch (Exception )
            { 
            }
        }
        public static void Trace(string message)
        {
            try
            {
                logger.Trace(message);
            }
            catch (Exception )
            {
            }
        }

        public static void Trace<T>(T obj)
        {
            try
            {
                Newtonsoft.Json.JsonSerializerSettings jsonSetting = new Newtonsoft.Json.JsonSerializerSettings();
                var objString = Newtonsoft.Json.JsonConvert.SerializeObject(obj, typeof(T), jsonSetting);
                logger.Trace(objString);
            }
            catch (Exception)
            { 
            }
        }
        public static void Trace(Exception ex)
        {
            try
            {
                logger.Trace(ex);
            }
            catch (Exception)
            {
            }
        }



        public static void Debug(string message, params object[] args)
        {
            try
            {
                logger.Debug(message, args);
            }
            catch (Exception)
            {
            }
        }
        public static void Debug(string message)
        {
            try
            {
                logger.Debug(message);
            }
            catch (Exception)
            { 
            }
        }

        public static void Debug<T>(T obj)
        {
            try
            {
                Newtonsoft.Json.JsonSerializerSettings jsonSetting = new Newtonsoft.Json.JsonSerializerSettings();
                var objString = Newtonsoft.Json.JsonConvert.SerializeObject(obj, typeof(T), jsonSetting);
                logger.Debug(objString); 
            }
            catch (Exception)
            { 
            }
        }
        public static void Debug(Exception ex)
        {
            try
            {
                logger.Debug(ex);
            }
            catch (Exception)
            {
            }
        }


        public static void Info(string message, params object[] args)
        {
            try
            {
                logger.Info(message, args);
            }
            catch (Exception)
            {
            }
        }
        public static void Info(string message)
        {
            try
            {
                logger.Info(message);
            }
            catch (Exception)
            {
            }
        }

        public static void Info<T>(T obj)
        {
            try
            {
                Newtonsoft.Json.JsonSerializerSettings jsonSetting = new Newtonsoft.Json.JsonSerializerSettings();
                var objString = Newtonsoft.Json.JsonConvert.SerializeObject(obj, typeof(T), jsonSetting);
                logger.Info(objString);
            }
            catch (Exception)
            {
            }
        }
        public static void Info(Exception ex)
        {
            try
            {
                logger.Info(ex);
            }
            catch (Exception)
            {
            }
        }


        public static void Warn(string message, params object[] args)
        {
            try
            {
                logger.Warn(message, args);
            }
            catch (Exception)
            {
            }
        }
        public static void Warn(string message)
        {
            try
            {
                logger.Warn(message);
            }
            catch (Exception)
            {
            }
        }

        public static void Warn<T>(T obj)
        {
            try
            {
                Newtonsoft.Json.JsonSerializerSettings jsonSetting = new Newtonsoft.Json.JsonSerializerSettings();
                var objString = Newtonsoft.Json.JsonConvert.SerializeObject(obj, typeof(T), jsonSetting);
                logger.Warn(objString);
            }
            catch (Exception)
            {
            }
        }

        public static void Warn(Exception ex)
        {
            try
            {
                logger.Warn(ex);
            }
            catch (Exception)
            {
            }
        }






        public static void Error(string message, params object[] args)
        {
            try
            {
                logger.Error(message, args);
            }
            catch (Exception)
            {
            }
        }
        public static void Error(string message)
        {
            try
            {
                logger.Error(message);
            }
            catch (Exception)
            {
            }
        }

        public static void Error<T>(T obj)
        {
            try
            {
                Newtonsoft.Json.JsonSerializerSettings jsonSetting = new Newtonsoft.Json.JsonSerializerSettings();
                var objString = Newtonsoft.Json.JsonConvert.SerializeObject(obj, typeof(T), jsonSetting);
                logger.Error(objString);
            }
            catch (Exception)
            {
            }
        }

        public static void Error(Exception ex)
        {
            try
            {
                logger.Info("1");
                logger.Error(ex);
                if (ex!=null && ex.InnerException!=null)
                {
                    logger.Info("2.0");
                    logger.Error(ex.InnerException);
                    logger.Info("2");
                }
                logger.Info("3");
                logger.Info(ex.StackTrace);
                logger.Info("4");
                if (ex!=null && ex.InnerException!=null)
                {
                    logger.Info("5.0");
                    logger.Info(ex.InnerException.StackTrace);
                    logger.Info("5");
                }
                if (ex!=null)
                {
                    logger.Info("6.0");
                    logger.Info(ex.ToJson());
                    logger.Info("6");
                }
                logger.Info("7");
            }
            catch (Exception)
            {
            }
        }










        public static void Fatal(string message, params object[] args)
        {
            try
            {
                logger.Fatal(message, args);
            }
            catch (Exception)
            {
            }
        }
        public static void Fatal(string message)
        {
            try
            {
                logger.Fatal(message);
            }
            catch (Exception)
            {
            }
        }

        public static void Fatal<T>(T obj)
        {
            try
            {
                Newtonsoft.Json.JsonSerializerSettings jsonSetting = new Newtonsoft.Json.JsonSerializerSettings();
                var objString = Newtonsoft.Json.JsonConvert.SerializeObject(obj, typeof(T), jsonSetting);
                logger.Fatal(objString);
            }
            catch (Exception)
            {
            }
        }
        public static void Fatal(Exception ex)
        {
            try
            {
                logger.Fatal(ex);
            }
            catch (Exception)
            {
            }
        }
        


    }



    /// <summary>
    /// 异常日志扩展方法
    /// </summary>
    public static class ExceptionLogUtility
    {
        public static void LogError(this Exception ex)
        {
            LogUtility.Error(ex);
        }

        public static void LogInfo(this Exception ex)
        {
            LogUtility.Info(ex);
        }


        public static void LogWarn(this Exception ex)
        {
            LogUtility.Warn(ex);
        }
        public static void LogFatal(this Exception ex)
        {
            LogUtility.Fatal(ex);
        }
    }


    /// <summary>
    /// 字符串日志扩展方法
    /// </summary>
    public static class StringLogUtility
    {
        public static void LogError(this string ex)
        {
            LogUtility.Error(ex);
        }
        public static void LogDebug(this string ex)
        {
            LogUtility.Debug(ex);
        }


        public static void LogWarn(this string ex)
        {
            LogUtility.Warn(ex);
        }
        public static void LogInfo(this string ex)
        {
            LogUtility.Info(ex);
        }


        public static void LogTrace(this string ex)
        {
            LogUtility.Trace(ex);
        }
    }

    /// <summary>
    /// 结构化数据日志扩展方法
    /// </summary>
    public static class ObjectLogUtility
    {
        public static void LogError(this object ex)
        {
            LogUtility.Error(ex);
        }
        public static void LogDebug(this object ex)
        {
            LogUtility.Debug(ex);
        }


        public static void LogWarn(this object ex)
        {
            LogUtility.Warn(ex);
        }
        public static void LogInfo(this object ex)
        {
            LogUtility.Info(ex);
        }
        public static void LogTrace(this object ex)
        {
            LogUtility.Trace(ex);
        }
    }
}
