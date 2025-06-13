/*
 * Copyright 2024 (c) Leia Inc.  All rights reserved.
 *
 * NOTICE:  All information contained herein is, and remains
 * the property of Leia Inc. and its suppliers, if any.  The
 * intellectual and technical concepts contained herein are
 * proprietary to Leia Inc. and its suppliers and may be covered
 * by U.S. and Foreign Patents, patents in process, and are
 * protected by trade secret or copyright law.  Dissemination of
 * this information or reproduction of this materials strictly
 * forbidden unless prior written permission is obtained from
 * Leia Inc.
 */
using UnityEngine;
using System;
using System.Globalization;

namespace LeiaUnity
{
    public enum LogLevel
    {
        Trace,  // finer than debug: show results, contents
        Debug,  // detailed events helpful for debugging
        Info,   // general information about stages of app and rare events
        Warning,// highlights dangerous cases or when attention is needed
        Error,  // not critical errors (app still runs)
        Fatal,  // critical failure, app can't run
        Disable // disable logs at all
    }

    public static class LogUtil
    {
        private static LogLevel _level;

        static LogUtil()
        {
            _level = LogLevel.Warning;

            #if LEIA_LOGLEVEL_TRACE
            _level = LogLevel.Trace;
            #elif LEIA_LOGLEVEL_DEBUG
            _level = LogLevel.Debug;
            #elif LEIA_LOGLEVEL_INFO
            _level = LogLevel.Info;
            #elif LEIA_LOGLEVEL_WARNING
            _level = LogLevel.Warning;
            #elif LEIA_LOGLEVEL_ERROR
            _level = LogLevel.Error;
            #elif LEIA_LOGLEVEL_FATAL
            _level = LogLevel.Fatal;
            #elif LEIA_LOGLEVEL_DISABLE
            _level = LogLevel.Disable;
            #endif
        }

        public static void Log(LogLevel level, string msg, params object[] objects)
        {
            if (_level <= level)
            {
                var str = "";

                CultureInfo culture = new CultureInfo("en-US");

                if (objects.Length > 0)
                {
                    str = level.ToString().ToUpper(culture) + "> [" + DateTime.UtcNow.ToString("d", culture) + ", " + DateTime.UtcNow.ToString("T", culture) + "." + DateTime.UtcNow.Millisecond.ToString(culture) + "] " + string.Format(culture, msg, objects);
                }
                else
                {
                    str = level.ToString().ToUpper(culture) + "> [" + DateTime.UtcNow.ToString("d", culture) + ", " + DateTime.UtcNow.ToString("T", culture) + "." + DateTime.UtcNow.Millisecond.ToString(culture) + "] " + msg;
                }


                if (level <= LogLevel.Info)
                {
                    UnityEngine.Debug.Log(str);
                }
                else if (level <= LogLevel.Warning)
                {
                    UnityEngine.Debug.LogWarning(str);
                }
                else
                {
                    UnityEngine.Debug.LogError(str);
                }
            }
        }

        public static void Trace(string msg, params object[] objects)
        {
            Log(LogLevel.Trace, msg, objects);
        }

        public static void Debug(string msg, params object[] objects)
        {
            Log(LogLevel.Debug, msg, objects);
        }

        public static void Info(string msg, params object[] objects)
        {
            Log(LogLevel.Info, msg, objects);
        }

        public static void Warning(string msg, params object[] objects)
        {
            Log(LogLevel.Warning, msg, objects);
        }

        public static void Error(string msg, params object[] objects)
        {
            Log(LogLevel.Error, msg, objects);
        }

        public static void Fatal(string msg, params object[] objects)
        {
            Log(LogLevel.Fatal, msg, objects);
        }

        public static void ObjLog(this object obj, LogLevel level, string msg, params object[] objects)
        {
            if (_level <= level)
            {
                Log(level, obj.GetType().Name + ": " + msg, objects);
            }
        }

        public static void Trace(this object obj, string msg, params object[] objects)
        {
            ObjLog(obj, LogLevel.Trace, msg, objects);
        }

        public static void Debug(this object obj, string msg, params object[] objects)
        {
            ObjLog(obj, LogLevel.Debug, msg, objects);
        }

        public static void Info(this object obj, string msg, params object[] objects)
        {
            ObjLog(obj, LogLevel.Info, msg, objects);
        }

        public static void Warning(this object obj, string msg, params object[] objects)
        {
            ObjLog(obj, LogLevel.Warning, msg, objects);
        }

        public static void Error(this object obj, string msg, params object[] objects)
        {
            ObjLog(obj, LogLevel.Error, msg, objects);
        }

        public static void Fatal(this object obj, string msg, params object[] objects)
        {
            ObjLog(obj, LogLevel.Fatal, msg, objects);
        }
    }
}