using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DevLocker.Tools
{
	/// <summary>
	/// Helpful menu items like:
	/// - Copy selected GUIDs
	/// - Edit With Notepad++ or Sublime
	/// </summary>
	public static class MiscUtils 
	{
		[MenuItem("Assets/Copy selected GUIDs", false, -990)]
		private static void CopySelectedGuid()
		{
			List<string> guids = new List<string>(Selection.objects.Length);

			foreach (var obj in Selection.objects) {
				string guid;
				long localId;
				if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out localId))
					continue;

				if (AssetDatabase.IsMainAsset(obj)) {
					guids.Add(guid);
				} else {
					guids.Add($"{guid}-{localId}");
				}
			}

			var result = string.Join(", ", guids);
			Debug.Log($"Guids copied: {result}");
			
			var te = new TextEditor();
			te.text = result;
			te.SelectAll();
			te.Copy();
		}


		private static string[] _notepadPaths = new string[] {
			@"C:\Program Files\Notepad++\notepad++.exe",
			@"C:\Program Files (x86)\Notepad++\notepad++.exe",
			@"C:\Programs\Notepad++\notepad++.exe",
		};
		private static string[] _sublimePaths = new string[] {
			@"C:\Program Files\Sublime Text 3\subl.exe",
			@"C:\Program Files\Sublime Text 3\sublime_text.exe",
			@"C:\Program Files\Sublime Text 2\sublime_text.exe",
			@"C:\Program Files (x86)\Sublime Text 3\subl.exe",
			@"C:\Program Files (x86)\Sublime Text 3\sublime_text.exe",
			@"C:\Program Files (x86)\Sublime Text 2\sublime_text.exe",
			@"C:\Programs\Sublime Text 3\subl.exe",
			@"C:\Programs\Sublime Text 3\sublime_text.exe",
			@"C:\Programs\Sublime Text 2\sublime_text.exe",
		};

		[MenuItem("Assets/Edit With/Notepad++", false, -980)]
		private static void EditWithNotepadPlusPlus()
		{
			var args = string.Join(" ", GetPathsOfAssets(Selection.objects, false));
			EditWithApp(_notepadPaths, args);
		}
		
		[MenuItem("Assets/Edit With/Notepad++ Metas", false, -980)]
		private static void EditWithNotepadPlusPlusMetas()
		{
			var args = string.Join(" ", GetPathsOfAssets(Selection.objects, true));
			EditWithApp(_notepadPaths, args);
		}
		
		[MenuItem("Assets/Edit With/Sublime", false, -980)]
		private static void EditWithSublime()
		{
			var args = string.Join(" ", GetPathsOfAssets(Selection.objects, false));
			EditWithApp(_sublimePaths, args);
		}
		
		[MenuItem("Assets/Edit With/Sublime Metas", false, -980)]
		private static void EditWithSublimeMetas()
		{
			var args = string.Join(" ", GetPathsOfAssets(Selection.objects, true));
			EditWithApp(_sublimePaths, args);
		}

		private static IEnumerable<string> GetPathsOfAssets(Object[] objects, bool metas) {
			
			return objects
					.Select(AssetDatabase.GetAssetPath)
					.Where(p => !string.IsNullOrEmpty(p))
					.Select(p => metas ? AssetDatabase.GetTextMetaFilePathFromAssetPath(p) : p)
					.Select(p => '"' + p + '"')
				;
		}

		private static void EditWithApp(string[] appPaths, string filePath)
		{
			var editorPath = appPaths.FirstOrDefault(File.Exists);
			if (string.IsNullOrEmpty(editorPath)) {
				EditorUtility.DisplayDialog("Error", $"Program is not found.", "Sad");
				return;
			}


			System.Diagnostics.Process.Start(editorPath, filePath);
		}
	}
}
