using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SLua;
using LuaInterface;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
[SLua.CustomLuaClass]
public class Lua
{
    public delegate void OnLuaMessage(string data);
    private OnLuaMessage _onluaMessage = null;
    private static Lua ms_Instance = null;
    private static int preTimeCount = 0;
    public static Lua Instance
    {
        get
        {
            if (ms_Instance == null)
            {
                ms_Instance = new Lua();
            }
            return ms_Instance;
        }
    }

    public LuaState luaState;

    private LuaSvr luasvr;
    
    public IntPtr handle
    {
        get
        {
            return luaState.handle;
        }
    }

    public static void OnMessage(string data)
    {
        if(Lua.Instance._onluaMessage != null)
        {
            Lua.Instance._onluaMessage(data);
        }
        //Debug.Log(msg);
    }

    public void SetLuaCallback()
    {
        LuaDLL.register_callback(OnMessage);
    }


    public void Init()
    {
        luasvr = new LuaSvr();
        luasvr.init(null, null, LuaSvrFlag.LSF_BASIC);

        luaState = luasvr.luaState;

#if TENCENT_CHANNEL
#else
        // lua profiler
        LuaDLL.init_profiler(luaState.L);
#endif
        UIDef.m_strPath = UIDef.m_strPath + "/" + UIDef.m_strTime;
        //Debug.Log(LuaScriptPath);
        DirectoryInfo myDirectoryInfo = new DirectoryInfo(UIDef.m_strPath);
        if (!myDirectoryInfo.Exists)
        {
            Directory.CreateDirectory(UIDef.m_strPath);
        }

        DoFile(UIDef.LuaScriptPath);
    }

    public void ReLoad()
    {
        if (luasvr == null || luaState == null)
        {
            SimpleLogger.ERROR(UIDef.LOG, "当前lua的环境已经被破坏无法重新加载脚本");
            return;
        }
        DoFile(UIDef.LuaScriptPath);
    }

    public void GC()
    {
        LuaDLL.lua_gc(luaState.L, LuaGCOptions.LUA_GCCOLLECT, 0);
    }
    public object DoFile(string fn)
    {
        if (fn == null) return null;
        return luaState.doFile(fn);
    }
    public object DoString(string str)
    {
        if (string.IsNullOrEmpty(str)) return null;
        return luaState.doString(str);
    }

    //public object DoLuaFile(string szPath)
    //{
    //    if (szPath == null) return null;
    //    return luaState.doFile(szPath);
    //}

    public LuaTable DictToLuaTable<T1, T2>(Dictionary<T1, T2> dict)
    {
        LuaTable table = new LuaTable(luaState);
        foreach (var o in dict)
        {
            if (o.Key.GetType() == typeof(string))
            {
                table[o.Key.ToString()] = o.Value;
            }
            else if(o.Key.GetType() == typeof(int))
            {
                int nKey = System.Convert.ToInt32(o.Key);
                table[nKey] = o.Value;
            }
            
        }
        return table;
    }
    public LuaTable ListToLuaTable<T>(List<T> list)
    {
        LuaTable table = new LuaTable(luaState);
        if (list == null) return table;
        for(int i = 0; i < list.Count; ++i)
        {
            table[i + 1] = list[i];
        }
        return table;
    }
    public LuaTable AryToLuaTable(params object[] Params)
    {
        LuaTable table = new LuaTable(luaState);
        for (int i = 0; i < Params.Length; ++i )
        {
            table[i+1] = Params[i];
        }
        return table;
    }

    public object this[string path]
    {
        get
        {
            return luaState[path];
        }
        set
        {
            luaState[path] = value;
        }
    }

    string[] GetSysDirector(string dir)
    {
        return System.IO.Directory.GetFileSystemEntries(dir);
    }

    public string[] GetProfilerFolders()
    {
        return GetSysDirector(Application.temporaryCachePath);
    }

    public string[] GetProfilerFiles(string path)
    {
        return GetSysDirector(path);
    }


    public void StopLuaProfiler()
    {
        object o = Instance.luaState.getFunction("profiler_stop").call();
#if UNITY_EDITOR
        EditorWindow w = EditorWindow.GetWindow<EditorWindow>(UIDef.g_editorWindow);
        if (w.GetType().Name == UIDef.g_editorWindow)
        {
            w.SendEvent(EditorGUIUtility.CommandEvent("AppStoped"));
        }
#endif
    }
    
    public bool IsRegisterLuaProfilerCallback()
    {
        return LuaDLL.isregister_callback();
    }

    public void RegisterLuaProfilerCallback(OnLuaMessage callback)
    {
        //LuaDLL.register_callback(callback);
        if (callback != null)
            _onluaMessage = callback;
        else
            Debug.LogError("callback can't null");
        SetLuaCallback();
    }

    public void RegisterLuaProfilerCallback2(string obj, string method)
    {
        LuaDLL.register_callback2(obj, method);
    }

    public void UnRegisterLuaProfilerCallback()
    {
        _onluaMessage = null;
        LuaDLL.unregister_callback();
    }
    

    public void SetFrameInfo()
    {
        int frameCount = Time.frameCount;
        preTimeCount = frameCount;
        LuaDLL.frame_profiler(frameCount, System.DateTime.Now.Millisecond);
    }

    public LuaFunction GetFunction(string fn)
    {
        return luaState.getFunction(fn);
    }

    public void ExcelToLua()
    {
        try
        {
            System.Diagnostics.Process pro = new System.Diagnostics.Process();
            pro.StartInfo.WorkingDirectory = Application.dataPath + "/JX3Game/Source/File/ToLuaTool/";
            pro.StartInfo.FileName = Application.dataPath + "/JX3Game/Source/File/ToLuaTool/run.bat";
            pro.StartInfo.UseShellExecute = true;
            pro.Start();
            pro.WaitForExit();
        }
        catch (System.Exception)
        {

        }
    }
}

