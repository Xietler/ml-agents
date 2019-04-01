#if UNITY_CLOUD_BUILD

using System;
using System.Collections.Generic;
using System.Linq;
using MLAgents;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace MLAgents
{
	
	public class Builder
	{
		[MenuItem("ML-Agents/Run PreExport Method")]
		public static void PreExport()
		{
			var scenePath = Environment.GetEnvironmentVariable("SCENE_PATH"); 
			SwitchAllLearningBrainToControlMode();
			PutSceneOnTop(scenePath);
		}

		protected static void PutSceneOnTop(string scenePath)
		{
			EditorBuildSettingsScene targetScene = null;
			
			List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

			// Find the target scenes to be inserted as the first scene, all the rest to the list
			for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
			{
				if (EditorBuildSettings.scenes[i].path == scenePath)
				{
					targetScene = EditorBuildSettings.scenes[i];
				}
				else
				{
					scenes.Add(EditorBuildSettings.scenes[i]);
				}
			}
			
			scenes.Insert(0, targetScene);
			
			EditorBuildSettings.scenes = scenes.ToArray();
			
			Debug.Log("Switched to scene " + targetScene.path + "as the first in build settings");
		}
		
		[MenuItem("ML-Agents/Switch All Learning Brain To Control Mode")]
		protected static void SwitchAllLearningBrainToControlMode()
		{
			string[] scenePaths = Directory.GetFiles("Assets/ML-Agents/Examples/", "*.unity", SearchOption.AllDirectories);

			foreach (string scenePath in scenePaths)
			{
				var curScene = EditorSceneManager.OpenScene(scenePath);
				var aca = SceneAsset.FindObjectOfType<Academy>();
				if (aca != null)
				{
					var learningBrains = aca.broadcastHub.broadcastingBrains.Where(
						x => x != null && x is LearningBrain);
		
					foreach (Brain brain in learningBrains)
					{
						if (!aca.broadcastHub.IsControlled(brain))
						{
							Debug.Log("Switched brain in scene " + scenePath);
							aca.broadcastHub._brainsToControl.Add(brain);
						}
					}
					EditorSceneManager.SaveScene(curScene);
				}
				else
				{
					Debug.Log("scene " + scenePath + " doesn't have a Academy in it");
				}
			}
		}

	}
}

#endif