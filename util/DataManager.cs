using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace PrimitiveArmory
{
    public static class DataManager
    {
        public static Texture2D ReadPNG(string pngName)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string name = "PrimitiveArmory." + pngName + ".txt";
            string s;
            using (Stream stream = executingAssembly.GetManifestResourceStream(name))
            {
                using StreamReader streamReader = new StreamReader(stream);
                s = streamReader.ReadToEnd();
            }
            byte[] data = Convert.FromBase64String(s);
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipmap: false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            texture2D.LoadImage(data);
            return texture2D;
        }

        public static byte[] ReadBytes(string pngName)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string name = "PrimitiveArmory.PrimitiveArmoryResources." + pngName + ".txt";
            string s;
            using (Stream stream = executingAssembly.GetManifestResourceStream(name))
            {
                using StreamReader streamReader = new StreamReader(stream);
                s = streamReader.ReadToEnd();
            }
            return Convert.FromBase64String(s);
        }

        public static void EncodeExternalPNG()
        {
            string[] files = Directory.GetFiles(Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "encode") + Path.DirectorySeparatorChar, "*.png", SearchOption.TopDirectoryOnly);
            string[] array = files;
            foreach (string text in array)
            {
                try
                {
                    byte[] inArray = File.ReadAllBytes(text);
                    string contents = Convert.ToBase64String(inArray);
                    string path = text.Replace("png", "txt");
                    File.WriteAllText(path, contents);
                }
                catch (Exception message)
                {
                    Debug.LogError("Error while encoding " + text);
                    Debug.LogError(message);
                }
            }
        }

        public static string ReadTXT(string txtName)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string name = "PrimitiveArmory.PrimitiveArmoryResources." + txtName + ".txt";
            using Stream stream = executingAssembly.GetManifestResourceStream(name);
            using StreamReader streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        public static void LogAllResources()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
            Debug.Log("Log All Resources: ");
            string[] array = manifestResourceNames;
            foreach (string message in array)
            {
                Debug.Log(message);
            }
        }
    }
}
