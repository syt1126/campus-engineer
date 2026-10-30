using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CampusEngineer
{
    public static class CampusLocalization
    {
        [Serializable] class Entry{public string key,en,sc;}
        [Serializable] class Catalog{public Entry[] entries;}
        static Dictionary<string,Entry> entries;
        static Regex fragments;
        public const string PreferenceKey="CampusEngineer.Language";
        public static int Language {get;private set;}=1;
        public static int Revision {get;private set;}
        public static int Count{get{Init();return entries.Count;}}
        public static void Init()
        {
            if(entries!=null)return;
            entries=new Dictionary<string,Entry>();
            var asset=Resources.Load<TextAsset>("CampusLanguages");
            if(asset==null)throw new InvalidOperationException("Missing CampusLanguages catalog");
            foreach(var e in JsonUtility.FromJson<Catalog>(asset.text).entries)entries[e.key]=e;
            var keys=new List<string>(entries.Keys);keys.Sort((a,b)=>b.Length.CompareTo(a.Length));
            fragments=new Regex(string.Join("|",keys.ConvertAll(Regex.Escape).ToArray()));
            Language=Mathf.Clamp(PlayerPrefs.GetInt(PreferenceKey,1),0,2);
        }
        public static void SetLanguage(int language)
        {
            Init();Language=Mathf.Clamp(language,0,2);Revision++;
            PlayerPrefs.SetInt(PreferenceKey,Language);PlayerPrefs.Save();
        }
        public static string T(string source)
        {
            Init();if(string.IsNullOrEmpty(source) || Language==1)return source;
            Entry e;if(entries.TryGetValue(source,out e))return Language==0?e.sc:e.en;
            // Composite evidence strings stay in their source language on disk and translate on display.
            return fragments.Replace(source,m=>Language==0?entries[m.Value].sc:entries[m.Value].en);
        }
    }
}
