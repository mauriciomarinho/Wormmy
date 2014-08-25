using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Wormmy
{
	public class SceneManager{

		private Game1 _game;
		private List<Scene> _scenes, _activeScenes;

		public Game1 Game { get {return _game;} }
		public List<Scene> Scenes { get {return _scenes;} }
		public List<Scene> ActiveScenes { get { return _activeScenes;} }

		public SceneManager(Game1 game)
		{
			_scenes = new List<Scene> ();
			_activeScenes = new List<Scene> ();
			_game = game;
		}

		public void ChangeScene(Scene newScene, bool reload=false){
			if (_activeScenes.Count > 0) {

				if (reload) {
					newScene.Initialize ();
					newScene.Load (this.Game.Content);
				}

				_activeScenes [0] = newScene;

			} else {
				throw new Exception ("Scene not in SceneManager's list, add it before change.");
			}
		}

		/// <summary>
		/// Activates a unique scene, clearing all overlays.
		/// </summary>
		public void ActivateScene(Scene toActivate, bool reload=false){
			if (!_activeScenes.Contains (toActivate)) {

				if (reload) {
					toActivate.Initialize ();
					toActivate.Load (this.Game.Content);
				}

				_activeScenes.Clear ();
				_activeScenes.Insert (0, toActivate); // Inserts as first item
			} 
		}

		public void AddSceneOverlay(Scene overlay, bool reload=false){
			if( !_activeScenes.Contains(overlay) ){
				if (reload) {
					overlay.Initialize ();
					overlay.Load (this.Game.Content);
				}
				_activeScenes.Add (overlay); // Inserts as last item
			}
		}

		public void CreateScene(Scene item){
			if( !_scenes.Contains(item) ){
				_scenes.Add (item); // Inserts as last item
				item.Manager = this;
			}
		}

		public void InitializeActive(){
			if (_activeScenes.Count > 0) {
				foreach (Scene item in _activeScenes) {
					item.Initialize ();
				}
			} else {
				throw new Exception ("No active scene in SceneManager");
			}
		}

		public void LoadActive(){
			if (_activeScenes.Count > 0) {
				foreach (Scene item in _activeScenes) {
					item.Load (this.Game.Content);
				}
			} else {
				throw new Exception ("No active scene in SceneManager");
			}
		}

		public void UpdateActive(){
			if (_activeScenes.Count > 0) {
				foreach (Scene item in _activeScenes) {
					item.Update ();
				}
			} else {
				throw new Exception ("No active scene in SceneManager");
			}
		}

		public void DrawActive(){
			if (_activeScenes.Count > 0) {
				foreach (Scene item in _activeScenes) {
					item.Draw (this.Game.spriteBatch);
				}
			} else {
				throw new Exception ("No active scene in SceneManager");
			}
		}
	}

	public class Scene
	{
		public SceneManager Manager { get; set; }
		public List<Camera2D> Cameras { get; set; }
		public Camera2D ActiveCamera { get; set; }
		public List<GameObject2D> Objects { get; set; }
		public List<GameObject2DGroup> ObjectGroups { get; set; }
		public float Gravity { get; set; }

		public Scene () // Game
		{
		}

		public virtual void AddObject(GameObject2D gameObject)
		{
			Objects.Add (gameObject);
		}

		public virtual void Initialize(){

		}

		public virtual void Load(ContentManager CM) // Content Manager
		{
		}

		public virtual void Unload(ContentManager CM) // Content Manager
		{
		}

		public virtual void Update()
		{
		}

		public virtual void Draw(SpriteBatch spriteBatch) // SpriteBatch
		{
		}
	}
}