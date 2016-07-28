﻿using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;

public enum eHanoiCallType
{
    None,
    C,
    Lua,
}

public class HanoiRoot
{
    public string objectName = "";
    public string programName = "";
    public int totalCalls = 0;
    public float timeConsuming = 0.0f;

    public HanoiNode callStats;
}

public class HanoiNode
{
    public static int s_count = 0;
    public HanoiNode(HanoiNode parent)
    {
        s_count++;

        Parent = parent;
    }

    public string moduleName = "";
    public string funcName = "";

    public int stackLevel = HanoiConst.BAD_NUM;

    public int currentLine = HanoiConst.BAD_NUM;
    public int lineDefined = HanoiConst.BAD_NUM;

    public eHanoiCallType callType = eHanoiCallType.None;
    public double timeConsuming = 0.0f;
    public double beginTime = 0.0f;
    public double endTime = 0.0f;
    public double interval = 0.0f;

    public HanoiNode Parent;
    public List<HanoiNode> Children = new List<HanoiNode>();

    // rendering properties
    public bool HasValidRect() { return renderRect.width > 0 && renderRect.height > 0; }
    public Rect renderRect;

    public bool highlighted = false;


    public Color GetNodeColor()
    {
        if (callType == eHanoiCallType.C)
            return HanoiConst.GetDyeColor(DyeType.CFunc);

        if (moduleName.StartsWith("@Lua"))
            return HanoiConst.GetDyeColor(DyeType.LuaInFile);

        if (moduleName.StartsWith("\\n"))
            return HanoiConst.GetDyeColor(DyeType.LuaMemBytes);

        return HanoiConst.GetDyeColor(DyeType.Default);
    }
}

public class HanoiData 
{
    public HanoiRoot Root { get { return m_hanoiData; } }

    public int MaxStackLevel { get { return m_maxStackLevel; } }
    int m_maxStackLevel = 0;

    JSONObject m_json;
    HanoiRoot m_hanoiData;

    public bool Load(string filename)
    {
        try
        {
            string text = System.IO.File.ReadAllText(filename);
            m_json = new JSONObject(text);

            if (m_json.type != JSONObject.Type.OBJECT)
                return false;

            if (m_json.list.Count != 1)
                return false;

            HanoiNode.s_count = 0;

            m_hanoiData = new HanoiRoot();
            JSONObject jsonRoot = (JSONObject)m_json.list[0];
            if (!readRoot(jsonRoot, m_hanoiData))
            {
                Debug.LogErrorFormat("reading {0} failed.", filename);
                return false;
            }

            Debug.LogFormat("reading {0} objects.", HanoiNode.s_count);

        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return false;
        }

        return true;
    }

    bool readRoot(JSONObject obj, HanoiRoot root)
    {
        if (obj.type != JSONObject.Type.OBJECT)
            return false;

        for (int i = 0; i < obj.list.Count; i++)
        {
            string key = (string)obj.keys[i];
            JSONObject j = (JSONObject)obj.list[i];
            if (key == "objectName" && j.type == JSONObject.Type.STRING)
            {
                root.objectName = j.str;
            }
            if (key == "programName" && j.type == JSONObject.Type.STRING)
            {
                root.programName = j.str;
            }
            if (key == "totalCalls" && j.type == JSONObject.Type.NUMBER)
            {
                root.totalCalls = (int)j.n;
            }
            if (key == "timeConsuming" && j.type == JSONObject.Type.NUMBER)
            {
                root.timeConsuming = j.n;
            }
            if (key == "callStats" && j.type == JSONObject.Type.OBJECT)
            {
                HanoiNode node = new HanoiNode(null);
                if (readObject(j, node))
                {
                    root.callStats = node;
                    root.callStats.endTime = root.callStats.endTime - root.callStats.beginTime;
                    root.callStats.beginTime = 0.0f;
                }
            }
        }

        return true;
    }

    bool readObject(JSONObject obj, HanoiNode node)
    {
        if (obj.type != JSONObject.Type.OBJECT)
            return false;

        for (int i = 0; i < obj.list.Count; i++)
        {
            string key = (string)obj.keys[i];
            JSONObject j = (JSONObject)obj.list[i];

            if (key == "moduleName" && j.type == JSONObject.Type.STRING)
            {
                node.moduleName = j.str;
            }
            if (key == "funcName" && j.type == JSONObject.Type.STRING)
            {
                node.funcName = j.str;
            }
            if (key == "callType" && j.type == JSONObject.Type.STRING)
            {
                switch (j.str)
                {
                    case "C":
                        node.callType = eHanoiCallType.C;
                        break;
                    case "Lua":
                        node.callType = eHanoiCallType.Lua;
                        break;
                }
            }
            if (key == "stackLevel" && j.type == JSONObject.Type.NUMBER)
            {
                node.stackLevel = (int)j.n;

                if (node.stackLevel > m_maxStackLevel)
                {
                    m_maxStackLevel = node.stackLevel;
                }
            }
            if (key == "begintime" && j.type == JSONObject.Type.NUMBER)
            {
                node.beginTime = j.n;
            }
            if (key == "endtime" && j.type == JSONObject.Type.NUMBER)
            {
                node.endTime = j.n;
            }
            if (key == "timeConsuming" && j.type == JSONObject.Type.NUMBER)
            {
                node.timeConsuming = j.n;
            }
            if (key == "interval" && j.type == JSONObject.Type.NUMBER)
            {
                node.interval = j.n;
            }
            if (key == "currentLine" && j.type == JSONObject.Type.NUMBER)
            {
                node.currentLine = (int)j.n;
            }
            if (key == "lineDefined" && j.type == JSONObject.Type.NUMBER)
            {
                node.lineDefined = (int)j.n;
            }
            if (key == "children" && j.type == JSONObject.Type.ARRAY)
            {
                foreach (JSONObject childJson in j.list)
                {
                    HanoiNode child = new HanoiNode(node);
                    if (readObject(childJson, child))
                    {
                        node.Children.Add(child);
                    }
                }
            }
        }

        return true;
    }
}
