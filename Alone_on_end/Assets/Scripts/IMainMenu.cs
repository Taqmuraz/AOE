using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public enum MenuState
{
	Main,
	Play,
	Load,
	Quit,
	Opts,
	None
}
[System.Serializable]
public class Settings
{
	public int grafic = 4;
	public float sens = 0.5f;
	public int maxZombie = 8;

	public static Settings current = new Settings();
}
[System.Serializable]
public class Saver
{
	public static string path
	{
		get {
			return Application.persistentDataPath;
		}
	}

	public static void SaveSettings () {
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Create (path + "/Sets.cfg");
		bf.Serialize (file, Settings.current);
		file.Close ();
		Debug.Log ("Saved settings sucsess");
	}
	public static void LoadSettings () {
		if (File.Exists (path + "/Sets.cfg")) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (path + "/Sets.cfg", FileMode.Open);
			Settings.current = (Settings)bf.Deserialize (file);
			file.Close ();
		} else {
			Debug.Log ("Has no file!");
		}
	}
}

public class IMainMenu : MonoBehaviour {

	public Renderer rend;
	public float offsetSpeed = 1;

	public MenuState state;
	public GUISkin ui;

	public void SetState (MenuState s) {
		state = s;
	}

	private void StartGame (int level_index) {
		AsyncOperation a = SceneManager.LoadSceneAsync (level_index);
		a.priority = 15;
		state = MenuState.None;
	}
	private void Quit () {
		Application.Quit ();
	}
	public static void ToMenu () {
		AsyncOperation a = SceneManager.LoadSceneAsync (0);
		a.priority = 15;
	}
	private void FixedUpdate () {
		rend.material.mainTextureOffset += Vector2.up * Time.fixedDeltaTime * offsetSpeed;
	}
	private void Start () {
		Cursor.lockState = CursorLockMode.Confined;
		Cursor.visible = true;
		Saver.LoadSettings ();
		QualitySettings.SetQualityLevel (Settings.current.grafic);
		zMax = Settings.current.maxZombie;
	}

	private void OnGUI () {
		GUI.skin = ui;
		if (state == MenuState.Main) {
			DrawMain ();
		}
		if (state == MenuState.Quit) {
			DrawQuit ();
		}
		if (state == MenuState.Play) {
			DrawPlay ();
		}
		if (state == MenuState.Opts) {
			DrawOpts ();
		}
	}

	private void DrawMain () {
		Rect r = new Rect (Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 130);
		GUI.Box (r, "Главное меню");
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 70, 160, 20);
		if (GUI.Button(r, "Начать игру")) {
			SetState (MenuState.Play);
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 40, 160, 20);
		if (GUI.Button(r, "Настройки")) {
			SetState (MenuState.Opts);
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 10, 160, 20);
		if (GUI.Button(r, "Выйти в Windows")) {
			SetState (MenuState.Quit);
		}
	}
	private void DrawQuit () {
		Rect r = new Rect (Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 100);
		GUI.Box (r, "Выйти из игры?");
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 70, 160, 20);
		if (GUI.Button(r, "Да")) {
			Quit ();
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 40, 160, 20);
		if (GUI.Button(r, "Нет")) {
			SetState (MenuState.Main);
		}
	}
	private void DrawPlay () {
		Rect r = new Rect (Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 150);
		GUI.Box (r, "Выберите уровень");
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 70, 160, 20);
		if (GUI.Button(r, "Уровень 1")) {
			StartGame (1);
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 40, 160, 20);
		if (GUI.Button(r, "Уровень 2")) {
			StartGame (2);
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 10, 160, 20);
		if (GUI.Button(r, "Уровень 3")) {
			StartGame (3);
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 + 20, 160, 20);
		if (GUI.Button(r, "Назад")) {
			SetState (MenuState.Main);
		}
	}
	private float zMax
	{
		get {
			return zMaxGetter;
		}
		set {
			zMaxGetter = value;
			Settings.current.maxZombie = (int)zMaxGetter;
		}
	}
	private float zMaxGetter = 4;
	public static int GetUpdateFramesPerSecond () {
		float t = 1f / Time.deltaTime;
		return (int)t;
	}
	private void DrawOpts () {
		Rect r = new Rect (Screen.width / 2 - 150, Screen.height / 2 - 190, 300, 390);
		GUI.Box (r, "Текущие настройки графики : " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 160, 160, 20);
		if (GUI.Button(r, "Для чайника")) {
			Settings.current.grafic = 7;
			QualitySettings.SetQualityLevel (7);
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 130, 160, 20);
		if (GUI.Button(r, "Мыльный ад")) {
			Settings.current.grafic = 0;
			QualitySettings.SetQualityLevel (0);
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 100, 160, 20);
		if (GUI.Button(r, "Очень низкие")) {
			Settings.current.grafic = 1;
			QualitySettings.SetQualityLevel (1);
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 70, 160, 20);
		if (GUI.Button(r, "Низкие")) {
			Settings.current.grafic = 2;
			QualitySettings.SetQualityLevel (2);
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 40, 160, 20);
		if (GUI.Button(r, "Средние")) {
			Settings.current.grafic = 4;
			QualitySettings.SetQualityLevel (4);
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 - 10, 160, 20);
		if (GUI.Button(r, "Высокие")) {
			Settings.current.grafic = 5;
			QualitySettings.SetQualityLevel (5);
		}
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 + 20, 160, 20);
		if (GUI.Button(r, "Ультра (PC)")) {
			Settings.current.grafic = 6;
			QualitySettings.SetQualityLevel (6);
		}
		r = new Rect (Screen.width / 2 - 120, Screen.height / 2 + 50, 240, 20);
		GUI.Label (r, "Чувствительность касания : " + ((int)(Settings.current.sens * 100f)) + "%");
		r = new Rect (Screen.width / 2 - 100, Screen.height / 2 + 80, 200, 20);
		Settings.current.sens = GUI.HorizontalScrollbar (r, Settings.current.sens, 0.25f, 0, 1);
		r = new Rect (Screen.width / 2 - 150, Screen.height / 2 + 110, 300, 20);
		GUI.Label (r, "Количество зомби : " + Settings.current.maxZombie);
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 + 140, 160, 20);
		zMax = GUI.HorizontalScrollbar (r, zMax, 5, 4, 80);
		r = new Rect (Screen.width / 2 - 80, Screen.height / 2 + 170, 160, 20);
		if (GUI.Button(r, "Готово")) {
			Saver.SaveSettings ();
			SetState (MenuState.Main);
		}
	}
}
