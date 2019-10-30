using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// User data.
/// </summary>
public class UserData {
	static public UserData instance{ get { return m_Instance; } }
	static protected UserData m_Instance;

	public int SaveCount{
		get{
			return saveCount;
		}
	}

	public bool NewUser{
		get{
			return newUser;
		}
	}

	public Dictionary<string, string> GraphIds;
	public Dictionary<string, string> SavedGameIds;
	public Dictionary<string, string> ReplayIds;
	public bool SoundOn;

	protected string fileName = "";
	protected int saveCount;
	protected bool newUser;


	/// <summary>
	/// Create this instance.
	/// </summary>
	static public void Create(){
		if (m_Instance == null)
			m_Instance = new UserData ();

		m_Instance.newUser = false;
		m_Instance.fileName = Application.persistentDataPath + "/UserData.bin";

		if (File.Exists (m_Instance.fileName)) {
			m_Instance.Read ();
		} else {
			NewSave ();
		}
	}


	/// <summary>
	/// News the save.
	/// </summary>
	static public void NewSave (){
		m_Instance.saveCount = 0;
		m_Instance.GraphIds = new Dictionary<string, string> ();
		m_Instance.SavedGameIds = new Dictionary<string, string> ();
		m_Instance.ReplayIds = new Dictionary<string, string> ();

		m_Instance.SoundOn = true;
		m_Instance.newUser = true;

		m_Instance.Save ();
	}


	/// <summary>
	/// Read this instance.
	/// </summary>
	public void Read(){
		BinaryReader br = new BinaryReader (new FileStream (fileName, FileMode.OpenOrCreate));

		saveCount = br.ReadInt32 ();

		GraphIds = new Dictionary<string, string> ();
		var temp = br.ReadInt32 ();
		for (int i = 0; i < temp; i++)
			GraphIds.Add (br.ReadString (), br.ReadString());

		SavedGameIds = new Dictionary<string, string> ();
		temp = br.ReadInt32 ();
		for (int i = 0; i < temp; i++)
			SavedGameIds.Add (br.ReadString (), br.ReadString());

		ReplayIds = new Dictionary<string, string> ();
		temp = br.ReadInt32 ();
		for (int i = 0; i < temp; i++)
			ReplayIds.Add (br.ReadString (), br.ReadString());

		SoundOn = br.ReadBoolean ();

		br.Close ();
	}


	/// <summary>
	/// Save this instance.
	/// </summary>
	public void Save(){
		BinaryWriter bw = new BinaryWriter (new FileStream (fileName, FileMode.OpenOrCreate));

		bw.Write (saveCount);

		bw.Write (GraphIds.Count);
		foreach (string k in GraphIds.Keys) {
			bw.Write (k);
			bw.Write (GraphIds [k]);
		}

		bw.Write (SavedGameIds.Count);
		foreach (string k in SavedGameIds.Keys) {
			bw.Write (k);
			bw.Write (SavedGameIds [k]);
		}

		bw.Write (ReplayIds.Count);
		foreach (string k in ReplayIds.Keys) {
			bw.Write (k);
			bw.Write (ReplayIds [k]);
		}

		bw.Write (SoundOn);

		bw.Close ();
		UserData.instance.saveCount += 1;
	}


	/// <summary>
	/// Deletes the saved games.
	/// </summary>
	public void DeleteSavedGames(){
		for (int i = 0; i < SavedGameIds.Count; i++)
			File.Delete (Application.persistentDataPath + "/Game" + i + ".gam");

		SavedGameIds.Clear ();
		Save ();
	}


	/// <summary>
	/// Deletes the graphs.
	/// </summary>
	public void DeleteGraphs(){
		for (int i = 0; i < GraphIds.Count; i++)
			File.Delete (Application.persistentDataPath + "/Graph" + i + ".map");

		GraphIds.Clear ();
		Save ();
	}


	/// <summary>
	/// Deletes the replays.
	/// </summary>
	public void DeleteReplays(){
		for (int i = 0; i < ReplayIds.Count; i++)
			File.Delete (Application.persistentDataPath + "/Replay" + i + ".rly");

		ReplayIds.Clear ();
		Save ();
	}


	/// <summary>
	/// Deletes the saved game.
	/// </summary>
	/// <param name="gameName">Game name.</param>
	public void DeleteSavedGame(string gameName){
		if (SavedGameIds.ContainsKey (gameName)) {
			File.Delete (SavedGameIds[gameName]);
			SavedGameIds.Remove (gameName);
		}

		Save ();
	}


	/// <summary>
	/// Deletes the graph.
	/// </summary>
	/// <param name="graphName">Graph name.</param>
	public void DeleteGraph(string graphName){
		if (GraphIds.ContainsKey (graphName)) {
			File.Delete (GraphIds[graphName]);
			GraphIds.Remove (graphName);
		}

		Save ();
	}


	/// <summary>
	/// Deletes the replay.
	/// </summary>
	/// <param name="replay">Replay.</param>
	public void DeleteReplay(string replay){
		if (ReplayIds.ContainsKey (replay)) {
			File.Delete (ReplayIds[replay]);
			ReplayIds.Remove (replay);
		}

		Save ();
	}
}
