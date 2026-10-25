using System;
using System.IO;
using System.Runtime.Serialization.Json;
namespace OctoEarth.Core
{
    public static class MissionConfigurationIO
    {
        public static T Read<T>(string path)
        { using(var stream=File.OpenRead(path)) return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream); }
        public static T Clone<T>(T value)
        { using(var stream=new MemoryStream()) {var serializer=new DataContractJsonSerializer(typeof(T)); serializer.WriteObject(stream,value); stream.Position=0; return (T)serializer.ReadObject(stream);} }
        public static void Write<T>(string path,T value)
        { Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))); using(var stream=File.Create(path)) new DataContractJsonSerializer(typeof(T)).WriteObject(stream,value); }
        public static MissionConfiguration LoadOrCreate(string path)
        { if(!File.Exists(path)) Write(path,new MissionConfiguration()); var c=Read<MissionConfiguration>(path); c.Validate(); return c; }
    }
}
