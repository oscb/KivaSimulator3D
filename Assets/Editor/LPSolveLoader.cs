using System.IO;
using System.Xml;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;


public class LPSolveLoaderPost {

	static string libPath = System.IO.Path.Combine (Path.GetDirectoryName(Application.dataPath), 
	                                                "Assets/Scripts/libs/liblpsolve55.dylib");
	static string unityConfigPath = "/Applications/Unity/Unity.app/Contents/Frameworks/Mono/etc/mono/config";

//	Not working at all, add dll manually
//	[PostProcessScene]
//	public static void OnPostprocessScene() {
//		string appPath = Path.GetDirectoryName(Application.dataPath);
//		if (Debug.isDebugBuild) {
//			LPSolveLoaderPost.loadLPSolveLib (unityConfigPath, libPath);
//		}
//	}

	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
		// Copy DYLIB to Package
		string targetPath = System.IO.Path.Combine (pathToBuiltProject, 
		                                            "Contents/Frameworks/LPSolve/");
		string destPath = System.IO.Path.Combine (targetPath, "liblpsolve55.dylib");

		if (!System.IO.Directory.Exists(targetPath)) {
			System.IO.Directory.CreateDirectory(targetPath);
        }
		System.IO.File.Copy(LPSolveLoaderPost.libPath, destPath, true);

		// Load XML
		string dllConfigFileName = pathToBuiltProject+"/Contents/Data/Managed/etc/mono/config";
		Debug.Log (dllConfigFileName);
		LPSolveLoaderPost.loadLPSolveLib (dllConfigFileName, destPath);
	}

	public static void loadLPSolveLib(string xmlFile, string libFile) {
		XmlDocument doc = new XmlDocument();
		doc.Load(xmlFile);
		XmlElement dllmap = (XmlElement) doc.CreateNode(XmlNodeType.Element, "dllmap", "");
		dllmap.SetAttribute("dll", "lpsolve55.dll");
		dllmap.SetAttribute("target", libFile);
		doc.DocumentElement.AppendChild((XmlNode) dllmap);
		doc.Save(xmlFile);

	}
}