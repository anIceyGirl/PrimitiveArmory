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
			Debug.Log("Creating Texture based on " + pngName + ".png");
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipmap: false)
            {
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0,
                filterMode = FilterMode.Point
            };
            Stream manifestResourceStream = typeof(Main).Assembly.GetManifestResourceStream("PrimitiveArmory.resources." + pngName + ".png");
			byte[] array = new byte[manifestResourceStream.Length];
			manifestResourceStream.Read(array, 0, (int)manifestResourceStream.Length);
			texture2D.LoadImage(array);

			return texture2D;
		}

		public static string ReadTXT(string txtName)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string name = "PrimitiveArmory.resources." + txtName + ".txt";
			using Stream stream = executingAssembly.GetManifestResourceStream(name);
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd();
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
