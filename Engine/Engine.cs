using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Wormmy
{
	//	enum Events { EV_NEXT, EV_PREVIOUS, EV_CLICKED, EV_HOVER }
	public enum Align { Left, Right, Center, Top, TopLeft, TopRight, Bottom, BottomLeft, BottomRight }

	static class Utils
	{
		public class Timer
		{
			private DateTime _startTime;
			public enum TimerTypes { Milliseconds, Seconds, Minutes, Hours, Days, Year }
			private TimerTypes _type;

			public Timer(TimerTypes type)
			{
				_type = type;
			}

			public void Start()
			{
				_startTime = DateTime.Now;
			}

			public void Reset()
			{
				_startTime = DateTime.Now;
			}

			public double ElapsedTime(){
				switch (_type) {
					case TimerTypes.Milliseconds:
					return DateTime.Now.Subtract (_startTime).TotalMilliseconds;
					case TimerTypes.Seconds:
					return DateTime.Now.Subtract (_startTime).TotalSeconds;
					case TimerTypes.Minutes:
					return DateTime.Now.Subtract (_startTime).TotalMinutes;
					case TimerTypes.Hours:
					return DateTime.Now.Subtract (_startTime).TotalHours;
					case TimerTypes.Days:
					return DateTime.Now.Subtract (_startTime).TotalDays;
					default:
					return 0;
				}
			}
		}

		public static class SavesParser
		{
			public static void Save (string path, object serializableObject)
			{
				// Saves class as binary file
				FileStream stream = File.Create (path);
				BinaryFormatter formatter = new BinaryFormatter ();
				formatter.Serialize (stream, serializableObject);
				stream.Close ();
			}

			public static object Load(string path)
			{
				if (File.Exists (path)) {
					// Reads from file
					FileStream stream = File.OpenRead (path);
					BinaryFormatter formatter = new BinaryFormatter ();
					object readFromFile = formatter.Deserialize (stream);
					stream.Close ();

					return readFromFile;

				} else { return null; }
			}
		}

		public static class ConfigParser
		{
			public static string ReadFile(string path)
			{
				if (File.Exists (path)) {
					string fileContent = File.ReadAllText (path);
					fileContent = fileContent.Replace (" ", String.Empty);
					fileContent = fileContent.ToLower ();
					return fileContent;
				} else {
					return null;
				}
			}

			public static void SaveFile(string path, Dictionary<string, string> dict){

				if ( File.Exists (path) )
					File.Delete (path);

				StreamWriter stream = File.CreateText (path);

				foreach (string key in dict.Keys) {
					stream.WriteLine (key + "=" + dict [key]);
				}

				stream.Close ();
			}

			public static Dictionary<string, string> GetDictionary(string fullText)
			{
				if (fullText != null) {
					string[] lines = fullText.Split ('\n'); // Puts every line in an array
					Dictionary<string, string> allValues = new Dictionary<string, string> ();

					foreach (string line in lines) {

						if (line.Contains ("[") || line.Contains ("]")) {
							continue; // Section does not goes in dicionary
						} else {

							string[] pair = line.Split ('=', ';'); // Separates on equal and comment sign

							if (pair.Length > 1 && !allValues.ContainsKey (pair [0])) {
								allValues.Add (pair [0], pair [1]); // First string as key, second as value
							}
						}
					}

					return allValues;
				} else {
					throw new Exception ("Configuration file empty or not found");
				}

			}
		}

		public static void FillTexture(Texture2D texture, Color tint)
		{
			Color[] newColor = new Color[texture.Width * texture.Height];
			for (int i = 0; i < newColor.Length; i++) {
				newColor [i] = tint;
			}
			texture.SetData (newColor);
		}

		public static void FillRoundedTexture(Texture2D texture, int borderRadius, Color tint){

			Color[] colors1D = new Color[texture.Width * texture.Height]; // color to use as setData

			// Trough each horizontal pixels, go trough all it's y
			for (int x = 0; x < texture.Width; x++) {
				for (int y = 0; y < texture.Height; y++) {

					int width = texture.Width;
					int height = texture.Height;
					bool topLeft = x < borderRadius && y < borderRadius;
					bool topRight = x > width - borderRadius && y < borderRadius;
					bool bottomLeft = x < borderRadius && y > height - borderRadius;
					bool bottomRight = x > width - borderRadius && y > height - borderRadius;

					Color none = Color.Transparent;

					Vector2 presentPosition = new Vector2 (x, y); // current target pixel of texture

					bool topLeftOut = Vector2.Distance (
						presentPosition, new Vector2 (borderRadius, borderRadius)) > borderRadius;
					bool topRightOut = Vector2.Distance (
						new Vector2 (width - borderRadius, borderRadius), presentPosition) > borderRadius;
					bool bottomLeftOut = Vector2.Distance (
						presentPosition, new Vector2 (borderRadius, height - borderRadius)) > borderRadius;
					bool bottomRightOut = Vector2.Distance(
						presentPosition, new Vector2(width - borderRadius, height - borderRadius)) > borderRadius;

					if (topLeft) {
						if (topLeftOut) {
							colors1D [x + y * width] = none;
						} else {
							colors1D [x + y * width] = tint;
						}
					} 

					else if (topRight) {
						if (topRightOut) {
							colors1D [x + y * width] = none;
						} else {
							colors1D [x + y * width] = tint;
						}
					}

					else if (bottomLeft) {
						if (bottomLeftOut) {
							colors1D [x + y * width] = none;
						} else {
							colors1D [x + y * width] = tint;
						}
					}

					else if (bottomRight) {
						if (bottomRightOut) {
							colors1D [x + y * width] = none;
						} else {
							colors1D [x + y * width] = tint;
						}
					}

					else {
						colors1D [x + y * width] = tint;
					}
				}
			}

			texture.SetData<Color> (colors1D);
		}

	}

	public class UIManager
	{
		public Dictionary<string, UIButton> Buttons { get; private set; }
		public Dictionary<string, UIFrame> Frames { get; private set; }
		public Dictionary<string, UISliderBase> Sliders { get; private set; }
		public Dictionary<string, UISwitch> Switches { get; private set; }
		public Dictionary<string, FontFile> Fonts { get; set; }
		private Game1 game { get; set; }
		public SpriteBatch spriteBatch { get; set; }
		private Viewport defaultViewport;
		public bool MouseActive { get; set; }
		public bool AlwaysUpdate { get; set; }
		public MouseState LastMouseState;

		/// <summary>
		/// Initializes a new instance of the <see cref="UI_study.UIManager"/> class. Initalize this in LoadContent after SpriteBatch creation
		/// </summary>
		/// <param name="game">Game.</param>
		public UIManager (Game1 game)
		{
			Buttons = new Dictionary<string, UIButton> ();
			Frames = new Dictionary<string, UIFrame> ();
			Sliders = new Dictionary<string, UISliderBase> ();
			Switches = new Dictionary<string, UISwitch> ();
			Fonts = new Dictionary<string, FontFile> ();
			this.game = game;
			this.spriteBatch = game.spriteBatch;
			this.defaultViewport = game.GraphicsDevice.Viewport;
			AlwaysUpdate = false;
			LastMouseState = Mouse.GetState ();
		}

		public class UIElement // : ICloneable
		{
			private Vector2 _origin, _position;

			public string ID { get; set; }
			public UIManager Manager { get; set; }
			public Vector2 Position 
			{ 
				get { return _position; } 
				set { _position = value; } 
			}
			public Vector2 Origin 
			{ 
				get { return _origin; } 
				set { _origin = value; } 
			}
			public Vector2 GlobalPosition 
			{
				get { return _origin + _position; }
				private set { _origin = value; }
			}
			public UIFrame Window { get; set; }
			public float Rotation { get; set; }
			public float Scale { get; set; }
			public Color Color { get; set; }
			public Texture2D Texture { get; set; }
			public Texture2D TextureHover { get; set; }
			public Texture2D TextureOnSelection { get; set; }
			public Texture2D TextureVisited { get; set; }
			public Vector2 LinearVelocity { get; set; }
			public virtual Rectangle Rectangle 
			{ 
				get { return new Rectangle ((int)GlobalPosition.X, (int)GlobalPosition.Y, Width, Height); } 
			}
			public int Width { get; set; }
			public int Height { get; set; }
			public UIElement Parent { get; set; }
			public List<UIElement> Children { get; set; }
			public virtual bool Active { get; set; }
			public bool Visible { get; set; }
			public bool CanGrab { get; set; }
			public bool CanScroll { get; set; }
			public bool Focus { get; set; }
			public bool Selected { get; set; }
			public bool MouseGrab { get; set; }

			public virtual void Update(){

				if (Active) {
					// Checks for mouse press
					var mouseState = Mouse.GetState ();

					if (mouseState.LeftButton == ButtonState.Pressed) {
						Selected = true;
					} 
					if (mouseState.LeftButton == ButtonState.Released) {
						Selected = false;
					}

					// Checks for mouse hovering over object
					Focus = this.Rectangle.Contains( new Point( (int)mouseState.X, (int)mouseState.Y) );

					// Checks click inside object
					if (CanGrab) {
						if (Focus && Selected) {
							MouseGrab = true; // User wants to move this element
						} else if (!Focus && Selected && MouseGrab) {
							MouseGrab = true;
						} else {
							MouseGrab = false;
						}

						if (MouseGrab) {
							Vector2 mouseDifference = new Vector2 (mouseState.X - Manager.LastMouseState.X, 
							                                       mouseState.Y - Manager.LastMouseState.Y);
							LinearVelocity = new Vector2 (mouseDifference.X, mouseDifference.Y);
						}

						if (!MouseGrab && CanGrab) {
							LinearVelocity = Vector2.Zero;
						}

					} else {
						MouseGrab = false; // User does not want to move this element
					}

					// Applies linear velocity
					Position += LinearVelocity;

					UpdateChildren ();
				}
			}

			public virtual void Draw()
			{
				if (Active) {

					// Hover
					if (TextureHover != null && Focus) {
						Manager.spriteBatch.Draw (TextureHover, this.Origin, Color.White);
					} else if (TextureOnSelection != null && Focus && Selected) {
						Manager.spriteBatch.Draw (TextureOnSelection, this.Origin, Color.White);
					} else {
						Manager.spriteBatch.Draw (Texture, this.Origin, Color.White);
					}

					DrawChildren ();
				}
			}

			public virtual void UpdateChildren() 
			{
				if (this.Children.Count > 0) {
					foreach (UIElement child in Children) {
						child.LinearVelocity += this.LinearVelocity;
						child.Update ();
						child.LinearVelocity -= this.LinearVelocity;
					}
				}
			}

			public virtual void DrawChildren()
			{
				if (this.Children.Count > 0) {
					foreach (UIElement child in Children) {
						child.Draw ();
					}
				}
			}		
		}

		public class UIFrame : UIElement
		{
			private Vector2 _offset, _maxOffset, _minOffset;
			private Camera2D _camera;

			public override Rectangle Rectangle 
			{
				get { return new Rectangle ((int)GlobalPosition.X, (int)GlobalPosition.Y, Width, Height); } 
			}
			public Rectangle DragArea { 
				get { return new Rectangle( (int)GlobalPosition.X, (int)GlobalPosition.Y, Width, Height / 10 ); } 
			}
			public Viewport viewport {
				get { return new Viewport(this.Rectangle); }
			}
			public Vector2 Offset { get {return _offset; } set { _offset = value; } }
			public Vector2 MaxOffset { get { return _maxOffset; } set { _maxOffset = value; } }
			public Vector2 MinOffset { get { return _minOffset; } set { _minOffset = value; } }

			public UIFrame(UIManager manager, string ID, int x=0, int y=0, int width=0, int height=0)
			{
				Active = true;
				Visible = true;
				this._camera = new Camera2D(this.viewport);
				this.Color = Color.White;
				this.ID = ID;
				this.Width = width;
				this.Height = height;
				Origin = Vector2.Zero;
				Offset = Vector2.Zero;
				Position = new Vector2(x, y);
				LinearVelocity = Vector2.Zero;
				Focus = false;
				MouseGrab = false;
				Selected = false;
				CanGrab = false;
				CanScroll = false;
				Focus = false;
				this.Manager = manager;
				Children = new List<UIElement>();
				Manager.Frames.Add(ID, this);
			}		

			public override void Update ()
			{
				if (Active) {
					// Checks for mouse press
					var mouseState = Mouse.GetState ();

					if (mouseState.LeftButton == ButtonState.Pressed) {
						Selected = true;
					} 
					if (mouseState.LeftButton == ButtonState.Released) {
						Selected = false;
					}

					// Checks for mouse hovering over object
					Focus = this.DragArea.Contains( new Point( (int)mouseState.X, (int)mouseState.Y) );

					// Checks click inside object
					if (CanGrab) {
						if (Focus && Selected) {
							MouseGrab = true; // User wants to move this element
						} else if (!Focus && Selected && MouseGrab) {
							MouseGrab = true;
						} else {
							MouseGrab = false;
						}

						if (MouseGrab) {
							Vector2 mouseDifference = new Vector2 (mouseState.X - Manager.LastMouseState.X, 
							                                       mouseState.Y - Manager.LastMouseState.Y);
							LinearVelocity = new Vector2 (mouseDifference.X, mouseDifference.Y);
						}

						if (!MouseGrab && CanGrab) {
							LinearVelocity = Vector2.Zero;
						}

					} else {
						MouseGrab = false; // User does not want to move this element
					}

					// Applies linear velocity
					Position += LinearVelocity;
					_camera.Position = Offset;

					UpdateChildren ();
				}
			}

			public override void Draw() {

				if (Visible) {
					Manager.game.GraphicsDevice.Viewport = this.viewport; // Gets viewport for clipping

					Manager.spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, _camera.Transformed());
					Manager.spriteBatch.Draw (this.Texture, Vector2.Zero, this.Color); // Draws window at zero inside its own viewport
					this.DrawChildren ();

					Manager.spriteBatch.End ();

					Manager.game.GraphicsDevice.Viewport = Manager.defaultViewport;
				}
			}
		}

		public class UIButton : UIElement
		{
			public UIFrame Window { get; private set; }
			public string Label { get; set; }
			public Color LabelColor { get; set; }
			public string TypeFace { get; set; }
			public override Rectangle Rectangle 
			{ 
				get { return new Rectangle ((int)Origin.X + (int)Window.Rectangle.Left - (int)Window.Offset.X, (int)Origin.Y + (int)Window.Rectangle.Top - (int)Window.Offset.Y, Width, Height); } 
			}
			public Rectangle LocalRectangle 
			{
				get { return new Rectangle ((int)Origin.X, (int)Origin.Y, Width, Height); }
			}
			public override bool Active {
				get {
					if (Window.Width < Width || Window.Height < Height || !Window.Active) {
						return false;
					} else {
						return base.Active;
					}
				}
				set { base.Active = value; }
			}

			public UIButton(UIManager manager, UIFrame window, string ID, UIElement parent, int x=0, int y=0, int width=0, int height=0 )
			{
				Active = true;
				Visible = true;
				this.Color = Color.White;
				this.Parent = parent;
				this.Window = window;
				this.Manager = manager;
				Label = null;
				TypeFace = null;
				UIManager.ParentTo(parent, this);
				this.ID = ID;
				Origin = new Vector2(x, y);
				Position = Vector2.Zero;
				LinearVelocity = Vector2.Zero;
				this.Width = width;
				this.Height = height;
				Texture = new Texture2D(manager.game.GraphicsDevice, width, height);
				Utils.FillTexture(this.Texture, this.Color);
				Focus = false;
				MouseGrab = false;
				Selected = false;
				CanGrab = false;
				CanScroll = false;
				Focus = false;
				this.Manager = manager;
				Children = new List<UIElement>();
				manager.Buttons.Add(ID, this);
			}

			public override void Update(){

				if (Active) {
					// Checks for mouse press
					var mouseState = Mouse.GetState ();

					if (mouseState.LeftButton == ButtonState.Pressed) {
						Selected = true;
					} 
					if (mouseState.LeftButton == ButtonState.Released) {
						Selected = false;
					}

					// Checks for mouse hovering over object
					Focus = this.Rectangle.Contains( new Point( (int)mouseState.X, (int)mouseState.Y) ) && 
						this.Window.viewport.Bounds.Contains( new Point( (int)mouseState.X, (int)mouseState.Y) );

					// Checks click inside object
					if (CanGrab) {
						if (Focus && Selected) {
							MouseGrab = true; // User wants to move this element
						} else if (!Focus && Selected && MouseGrab) {
							MouseGrab = true;
						} else {
							MouseGrab = false;
						}

						if (MouseGrab) {
							Vector2 mouseDifference = new Vector2 (mouseState.X - Manager.LastMouseState.X, 
							                                       mouseState.Y - Manager.LastMouseState.Y);
							LinearVelocity = new Vector2 (mouseDifference.X, mouseDifference.Y);
						}

						if (!MouseGrab && CanGrab) {
							LinearVelocity = Vector2.Zero;
						}

					} else {
						MouseGrab = false; // User does not want to move this element
					}

					// Applies linear velocity
					Position += LinearVelocity;

					UpdateChildren ();
				}
			}

			public override void Draw(){
				if (Visible) {
					// Hover, click, else draws normal texture.
					if (TextureHover != null && Focus && !Selected) {
						Manager.spriteBatch.Draw (TextureHover, LocalRectangle, this.Color);
					} else if (TextureOnSelection != null && Focus && Selected) {
						Manager.spriteBatch.Draw (TextureOnSelection, LocalRectangle, this.Color);
					} else {
						Manager.spriteBatch.Draw (Texture, LocalRectangle, this.Color);
					}

					if (Label != null && TypeFace != null) {
						Vector2 buttonCenter = new Vector2 (Origin.X + Width / 2, Origin.Y + Height / 2);
						TextManager.DrawText (Manager.Fonts [TypeFace], Label, LabelColor, buttonCenter, Manager.spriteBatch, Align.Center);
					}

					DrawChildren ();
				}
			}
		}

		public class UISliderBase : UIElement
		{
			private float _currentValue;

			public UIFrame Window { get; set; }
			public override Rectangle Rectangle { 
				get { return new Rectangle ((int)Origin.X + (int)Window.Rectangle.Left - (int)Window.Offset.X, (int)Origin.Y + (int)Window.Rectangle.Top - (int)Window.Offset.Y, Width, Height); } 
			}
			public Rectangle LocalSliderRectangle 
			{
				get { return new Rectangle ((int)SliderPosition.X, (int)SliderPosition.Y, Height, Height); }
			}
			public float CurrentValue 
			{
				get { return _currentValue; }
				set { _currentValue = MathHelper.Clamp (value, 0.0f, 1.0f); }
			}
			public Vector2 SliderPosition { 
				get { return new Vector2 ( this.Origin.X + (this.Width * CurrentValue) - (this.Height / 2), this.Origin.Y) ; }
			}
			public Texture2D SliderTexture { get; set; }
		}

		public class UIHSlider : UISliderBase {

			public UIHSlider(UIManager manager, UIFrame window, string ID, UIElement parent, int x=0, int y=0, int width=0, int height=0) {
				Active = true;
				Visible = true;
				this.Color = Color.White;
				this.Manager = manager;
				this.Window = window;
				this.ID = ID;
				this.Parent = parent;
				CurrentValue = 0.0f;
				Origin = new Vector2(x, y);
				Position = Vector2.Zero;
				this.Width = width;
				this.Height = height;
				Children = new List<UIElement>();
				Manager.Sliders.Add(ID, this);
				UIManager.ParentTo(parent, this);
			}

			public override void Update(){

				if (Active) {
					var mouseState = Mouse.GetState ();
					if (mouseState.LeftButton == ButtonState.Pressed) {
						this.Selected = true;
					} 
					if (mouseState.LeftButton == ButtonState.Released) {
						this.Selected = false;
					}

					Focus = this.Rectangle.Contains( new Point( (int)mouseState.X, (int)mouseState.Y) ) && 
						this.Window.viewport.Bounds.Contains( new Point( (int)mouseState.X, (int)mouseState.Y) );

					if (CanGrab) {
						if (Focus && Selected) {
							MouseGrab = true; // User wants to move this element
						} else if (!this.Focus && this.Selected && this.MouseGrab) {
							MouseGrab = true;
						} else {
							MouseGrab = false;
						}

						if (this.MouseGrab) {
							CurrentValue += ((float)mouseState.X - (float)Manager.LastMouseState.X) / 100;
						}

					} else {
						MouseGrab = false; // User does not want to move this element
					}

					this.Position += this.LinearVelocity;

					//				UpdateChildren ();
				}
			}

			public override void Draw ()
			{
				if (Visible) {
					Manager.spriteBatch.Draw (Texture, this.Origin, this.Color);
					if (TextureHover != null && Focus) {
						Manager.spriteBatch.Draw (TextureHover, SliderPosition, this.Color);
					} else if (SliderTexture != null) {
						Manager.spriteBatch.Draw (SliderTexture, SliderPosition, this.Color);
					}
				}
			}
		}

		public class UIVSlider : UISliderBase 
		{
			public new Vector2 SliderPosition { 
				get { return new Vector2 ( this.Origin.X, this.Origin.Y + (this.Height * CurrentValue) - (this.Width / 2) ); }
			}

			public UIVSlider(UIManager manager, UIFrame window, string ID, UIElement parent, int x=0, int y=0, int width=0, int height=0) {
				Active = true;
				Visible = true;
				this.Color = Color.White;
				this.Manager = manager;
				this.Window = window;
				this.ID = ID;
				this.Parent = parent;
				CurrentValue = 0.0f;
				Origin = new Vector2(x, y);
				Position = Vector2.Zero;
				this.Width = width;
				this.Height = height;
				Children = new List<UIElement>();
				Manager.Sliders.Add(ID, this);
				UIManager.ParentTo(parent, this);
			}

			public override void Update(){

				if (Active) {
					var mouseState = Mouse.GetState ();
					if (mouseState.LeftButton == ButtonState.Pressed) {
						this.Selected = true;
					} 
					if (mouseState.LeftButton == ButtonState.Released) {
						this.Selected = false;
					}

					Focus = this.Rectangle.Contains( new Point( (int)mouseState.X, (int)mouseState.Y) ) && 
						this.Window.viewport.Bounds.Contains( new Point( (int)mouseState.X, (int)mouseState.Y) );

					if (CanGrab) {
						if (Focus && Selected) {
							MouseGrab = true; // User wants to move this element
						} else if (!this.Focus && this.Selected && this.MouseGrab) {
							MouseGrab = true;
						} else {
							MouseGrab = false;
						}

						if (this.MouseGrab) {
							CurrentValue += ((float)mouseState.Y - (float)Manager.LastMouseState.Y) / 100;
						}

					} else {
						MouseGrab = false; // User does not want to move this element
					}

					this.Position += this.LinearVelocity;
				}
			}

			public override void Draw ()
			{
				if (Visible) {
					Manager.spriteBatch.Draw (Texture, this.Origin, this.Color);
					if (TextureHover != null && Focus) {
						Manager.spriteBatch.Draw (TextureHover, SliderPosition, this.Color);
					} else if (SliderTexture != null) {
						Manager.spriteBatch.Draw (SliderTexture, SliderPosition, this.Color);
					}
				}
			}
		}

		public class UISwitch : UIElement {

			private UIFrame _window;

			public bool Toggle { get; set; }
			public string Label { get; set; }
			public string TypeFace { get; set; }
			public Color LabelColor { get; set; }
			public Texture2D OnTexture { get; set; }
			public Texture2D OffTexture { get; set; }
			public override Rectangle Rectangle { 
				get { return new Rectangle ((int)Origin.X + (int)_window.Rectangle.Left - (int)_window.Offset.X, (int)Origin.Y + (int)_window.Rectangle.Top - (int)_window.Offset.Y, Width, Height); } 
			}

			public UISwitch(UIManager manager, UIFrame window, string ID, UIElement parent, int x=0, int y=0, int width=0, int height=0){
				Active = true;
				Visible = true;
				this.Color = Color.White;
				this.Manager = manager;
				this._window = window;
				this.ID = ID;
				Origin = new Vector2(x, y);
				Position = Vector2.Zero;
				this.Width = width;
				this.Height = height;
				Toggle = false;
				Children = new List<UIElement>();
				UIManager.ParentTo(parent, this);
			}

			public override void Update ()
			{
				if (Active) {
					var mouseState = Mouse.GetState ();
					if (mouseState.LeftButton == ButtonState.Pressed && Manager.LastMouseState.LeftButton != ButtonState.Pressed) {
						this.Selected = true;
					} else {
						this.Selected = false;
					}

					Focus = this.Rectangle.Contains( new Point( (int)mouseState.X, (int)mouseState.Y) ) && 
						this._window.viewport.Bounds.Contains( new Point( (int)mouseState.X, (int)mouseState.Y) );

					if (Focus && Selected)
						Toggle = !Toggle;
				}
			}

			public override void Draw ()
			{
				if (Visible) {
					if (this.Toggle && OnTexture != null) {
						Manager.spriteBatch.Draw (OnTexture, Origin, this.Color);
					} else {
						if (OffTexture != null) {
							Manager.spriteBatch.Draw (OffTexture, Origin, this.Color);
						}
					}

					if (Label != null && TypeFace != null) {
						Vector2 labelPosition = new Vector2 (GlobalPosition.X + Width + Manager.Fonts [TypeFace].BoxWidth, GlobalPosition.Y + Manager.Fonts [TypeFace].BoxHeight / 2);
						TextManager.DrawText (Manager.Fonts [TypeFace], Label, LabelColor, labelPosition, Manager.spriteBatch);
					}
				}

			}
		}

		public class UIPopup
		{
			private UIManager _uimanager;
			private string _font;
			private int _border;

			public string ID { get; set; }
			public string text { get; set; }
			public bool Active { get; set; }
			public Color textColor { get; set; }
			public Texture2D Background { get; set; }
			public int Width { get { return Background.Width; } }
			public int Height { get { return Background.Height; } }

			public UIPopup(UIManager manager, string ID, string text, string font, Color backgroundColor, Color textColor, int border=0 ){
				this.ID = ID;
				this.Active = false;
				this.text = text;
				this.textColor = textColor;
				this._uimanager = manager;
				this._font = font;
				this._border = border;
				int textureWidth = (int)TextManager.GetStringSize(manager.Fonts[font], text)[0] + border;
				int textureHeight = (int)TextManager.GetStringSize(manager.Fonts[font], text)[1] + border;
				this.Background = new Texture2D(manager.game.GraphicsDevice, textureWidth, textureHeight);
				Utils.FillTexture(Background, backgroundColor);
			}

			public void Draw(SpriteBatch spriteBatch, Vector2 drawPosition, Align align){
				if (Active) {
					switch (align) {
						case Align.Top:
						spriteBatch.Draw (Background, new Vector2 (drawPosition.X - Width / 2, drawPosition.Y - Height), Color.White);
						TextManager.DrawText (_uimanager.Fonts [_font], text, textColor, new Vector2 (drawPosition.X, drawPosition.Y - Height + _border / 2), _uimanager.spriteBatch, Align.Center);
						break;
						case Align.Bottom:
						spriteBatch.Draw (Background, new Vector2 (drawPosition.X - Width / 2, drawPosition.Y + 20), Color.White);
						TextManager.DrawText (_uimanager.Fonts [_font], text, textColor, new Vector2(drawPosition.X + _border / 2, drawPosition.Y + 20 + _border / 2), _uimanager.spriteBatch, Align.Center);
						break;
						case Align.Left:
						spriteBatch.Draw (Background, new Vector2 (drawPosition.X - Width, drawPosition.Y - Height / 2), Color.White);
						TextManager.DrawText (_uimanager.Fonts [_font], text, textColor, new Vector2(drawPosition.X - Width / 2 , drawPosition.Y - Height / 2 + _border / 2), _uimanager.spriteBatch, Align.Center);
						break;
						case Align.Right:
						spriteBatch.Draw (Background, new Vector2 (drawPosition.X + 20, drawPosition.Y - Height / 2), Color.White);
						TextManager.DrawText (_uimanager.Fonts [_font], text, textColor, new Vector2(drawPosition.X + 20 + _border / 2 + Width / 2, drawPosition.Y - Height / 2 + _border / 2), _uimanager.spriteBatch, Align.Center);
						break;
					}
				}
			}
		}
		// ----- UIManager Methods -----

		public static void ParentTo(UIElement parent, UIElement child){

			if (child.Parent != null) {
				child.Parent.Children.Remove (child); // Removes it from current parent's list
				child.Parent = null;
			}

			child.Parent = parent; // Adoption

			if (!parent.Children.Contains (child)) {
				parent.Children.Add (child); // Adds to new parent's list
			}
		}

		public void CheckInput(){

			bool mouseMoving = Mouse.GetState ().X != LastMouseState.X || Mouse.GetState ().Y != LastMouseState.Y;
			bool buttonPressed = Mouse.GetState ().LeftButton == ButtonState.Pressed || 
				Mouse.GetState ().RightButton == ButtonState.Pressed ||
					Mouse.GetState ().MiddleButton == ButtonState.Pressed;

			if (mouseMoving || buttonPressed) {
				this.MouseActive = true;
			}
			else {
				this.MouseActive = false;
			}
		}

		public void UpdateUI() {
			CheckInput ();
			if (this.MouseActive || AlwaysUpdate) {
				foreach (UIFrame window in this.Frames.Values) {
					window.Update ();
				}
			}
			LastMouseState = Mouse.GetState ();
		}

		public void DrawUI(){
			foreach (UIFrame window in this.Frames.Values) {
				window.Draw ();
			}
		}
	}

	public static class TextManager {

		// TextManager methods

		/// <summary>
		/// Gets the size of the string.
		/// </summary>
		/// <returns>The string size.</returns>
		/// <param name="font">FontFile Object.</param>
		/// <param name="text">String to measure.</param>
		/// <param name="scale">Scale of the font.</param>
		/// <param name="lineheight">Lineheight.</param>
		public static float[] GetStringSize(FontFile font, string text, float scale=1, float lineheight=0) {

			float width = 0; 
			float height = 0;
			float maxWidth = 0;
			float maxHeight = 0;
			bool specialArgument = false;

			foreach( char c in text ) {

				// Checks for scape character and set next character cheking as 'special argument'.
				if (c == '\\') {
					specialArgument = true;
					continue;
				} else if (specialArgument == true) {
					if (c == 'n') { 
						if (maxWidth < width) {
							maxWidth = width;
						}
						width = 0;
						maxHeight += height;
					}
					specialArgument = false;
					continue;
				} else if (c == '\n') {
					if (maxWidth < width) {
						maxWidth = width;
					}
					width = 0;
					maxHeight += height;
				}
				else {
					Rectangle sourceRect = font.CharacterRectangle [c.ToString ()] ;
					float ascender = font.CharacterValue [c.ToString ()] ["ascenderValue"];
					width += sourceRect.Width * scale;
					height = (sourceRect.Height + ascender + lineheight) * scale;
					if( maxHeight < height ) { 
						maxHeight += height; 
					}
				}
			}

			if( maxWidth < width ) { maxWidth = width;}
			if( maxHeight < height ) { maxHeight += height; }

			return new float[] { maxWidth, maxHeight };
		}

		/// <summary>
		/// Reverses the string.
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="text">Text.</param>
		public static string ReverseString(string text) {
			char[] charArray = text.ToCharArray ();
			Array.Reverse (charArray);
			return new string (charArray);
		}

		/// <summary>
		/// Draws the text line in a single line.
		/// </summary>
		/// <param name="font">FontFile.</param>
		/// <param name="stringToDraw">String to draw.</param>
		/// <param name="tint">Text's Color.</param>
		/// <param name="textOrigin">Text origin position.</param>
		/// <param name="textManager">Text manager.</param>
		/// <param name="scale">Scale.</param>
		public static void DrawTextLine(FontFile font, string stringToDraw, Color tint, Vector2 textOrigin, SpriteBatch spriteBatch, float letterSpacing=1) {

			float offsetX = 0;

			foreach (char c in stringToDraw) {

				int ascender = font.CharacterValue [c.ToString ()] ["ascenderValue"];
				Rectangle sourceRect = font.CharacterRectangle [c.ToString ()];

				Vector2 currentPosition = new Vector2 (
					textOrigin.X + offsetX * letterSpacing, 
					textOrigin.Y - ascender * letterSpacing
					);

				spriteBatch.Draw (
					font.FontAtlas, 
					currentPosition,
					sourceRect,
					tint
					);

				offsetX += sourceRect.Width * letterSpacing;
			}
		}

		/// <summary>
		/// Draws a dynamic text that supports line breaks.
		/// </summary>
		/// <param name="font">FontFile.</param>
		/// <param name="textToDraw">Text to draw.</param>
		/// <param name="tint">Text's Color.</param>
		/// <param name="textOrigin">Text origin.</param>
		/// <param name="textManager">Text manager.</param>
		/// <param name="align">Align can be: Align.Left, Align.Right or Align.Center.</param>
		/// <param name="maxLineSize">Max line size.</param>
		/// <param name="scale">Scale.</param>
		/// <param name="lineheight">Lineheight.</param>
		public static void DrawText (FontFile font, string textToDraw, Color tint, Vector2 textOrigin, SpriteBatch spriteBatch, Align align = Align.Left, int maxLineSize=0, float letterSpacing=1, float lineSpacing=0)
		{
			int lineSize = 0; // Current line width size, for linewrapping calculations
			char separator = ' '; // space as split
			string outputText = textToDraw;
			string[] splitText = outputText.Split (new char[] { separator }, StringSplitOptions.None);
			StringBuilder formattedText = new StringBuilder ();

			// Linewrapping handling, if lines have max size in pixels.
			if (maxLineSize > font.BoxWidth) {

				foreach (string word in splitText) {

					int wordSize = (int)GetStringSize (font, word) [0];

					if (lineSize + wordSize < maxLineSize) {
						formattedText.Append (word + separator);
						lineSize += wordSize;
					} else {
						formattedText.Append ('\n' + word + separator);
						lineSize = wordSize;
					}
				}

				outputText = formattedText.ToString ();
			}

			string[] textLines = outputText.Split (new char[] { '\n' }, StringSplitOptions.None);
			Vector2 linePosition = new Vector2 (textOrigin.X, textOrigin.Y);

			switch (align) {
				case Align.Left:

				foreach (string word in textLines) {
					DrawTextLine (font, word, tint, linePosition, spriteBatch, letterSpacing);
					linePosition.Y += font.BoxHeight + lineSpacing;
				}

				break;

				case Align.Right:

				foreach (string word in textLines) {
					linePosition.X = textOrigin.X - GetStringSize (font, word, letterSpacing)[0];
					DrawTextLine (font, word, tint, linePosition, spriteBatch, letterSpacing);
					linePosition.Y += font.BoxHeight + lineSpacing;
				}

				break;

				case Align.Center:

				foreach (string word in textLines) {
					linePosition.X = (int)(textOrigin.X - GetStringSize (font, word, letterSpacing)[0] * 0.5f);
					DrawTextLine (font, word, tint, linePosition, spriteBatch, letterSpacing);
					linePosition.Y += font.BoxHeight + lineSpacing;
				}

				break;

				default:
				foreach (string word in textLines) {
					DrawTextLine (font, word, tint, textOrigin, spriteBatch, letterSpacing);
					linePosition.Y += font.BoxHeight;
				}
				break;
			}
		}
	}

	public class FontFile
	{
		public string FontName { get; protected set; }
		public Texture2D FontAtlas { get; protected set; }
		public int BoxWidth, BoxHeight, Rows, Cols, LineHeight;
		public Dictionary<string, Dictionary<string, int> > CharacterValue { get; private set; }
		public Dictionary< string, Rectangle > CharacterRectangle { get; private set;}
		public string XmlPath { get; protected set; }
		public string ImgPath { get; protected set; }
		private XmlDocument XmlFile;
		private XmlNode XmlFontNode;

		/// <summary>
		/// Initializes a new instance of the <see cref="TextManagerProject.FontFile"/> class.
		/// </summary>
		/// <param name="CM">Content Manager to handle the font files.</param>
		/// <param name="filePath">File path, starting from content folder, not including extension. Must have png and xml.</param>
		/// <param name="fontColor">Color to render the text, if null, takes color from png file</param>
		public FontFile(ContentManager cm, string filePath)
		{
			XmlPath = cm.RootDirectory + Path.DirectorySeparatorChar + filePath + ".xml"; // Path to xml file
			ImgPath = cm.RootDirectory + Path.DirectorySeparatorChar +  filePath + ".png"; // Path to png file
			XmlFile = new XmlDocument (); // Creates XmlDocument to "host" file after next line
			XmlFile.Load (XmlPath);
			XmlFontNode = XmlFile.GetElementsByTagName("font").Item(0); // Gets first 'font' element from xml

			FontName = XmlFontNode.Attributes ["name"].InnerText; // Gets 'name' attribute from 'font' element
			FontAtlas = cm.Load<Texture2D> (filePath); // Content manager loads png as texture
			TintVisiblePixels (FontAtlas, Color.White);

			Rows = Convert.ToInt32 (XmlFontNode.Attributes ["rows"].Value); // Number of rows
			Cols = Convert.ToInt32 (XmlFontNode.Attributes ["cols"].Value); // Number of collumns
			BoxWidth = FontAtlas.Width / Cols; // Width / How many characters are horizontally
			BoxHeight = FontAtlas.Height / Rows; // Height / How many vertically

			CharacterValue = new Dictionary<string, Dictionary<string, int>> (); // Initiates new instance
			CharacterRectangle = new Dictionary<string, Rectangle>(); // Initiates new instance
			Vector2 elementPosition = new Vector2 (-1, 0); // Has to start at -1, because checking is made at the beginning adding +1, thus initial value = 0 on X

			// foreach font character, register them with their attributes in a dictionary
			foreach (XmlNode node in XmlFontNode.ChildNodes) {

				if (node.Name == "char") {

					// Updates current character position
					if (elementPosition.X < Cols - 1) {
						elementPosition.X++;
					} else {
						elementPosition.X = 0;
						elementPosition.Y++;
					}


					string id = null; // id starts as null, only assigned if an id is found
					bool isChar = (node.Name == "char");
					int lenght = node.Attributes.Count; // how many attributes are in the node

					// Creates a dictionary to hold temporary attributes values in the form ( id or valueName : value )
					Dictionary<string, int> tempDict = new Dictionary<string, int> ();

					// Puts all attributes, if any, in a dicitonary
					for(int i = 0; i < lenght; i++) {

						int value;
						string attribute = node.Attributes [i].Name;

						if ( attribute != "id" && isChar) {
							value = Convert.ToInt32 (node.Attributes [i].InnerXml);
							tempDict.Add (attribute, value);
						}
						if ( attribute == "id" ) {
							id = node.Attributes [i].InnerXml;
						}

						// Handling of special characters

						switch (id) {
							case "&apos;":
							id = "'";
							break;
							case "&quot;":
							id = "\"";
							break;
							case "&lt;":
							id = "<";
							break;
							case "&gt;":
							id = ">";
							break;
							case "&amp;":
							id = "&";
							break;
						}
					}

					// Checks if character has left padding, if not, applies default padding
					if (isChar && !tempDict.ContainsKey("paddingLeft")) {
						string attribute = "paddingLeft";
						int padding = Convert.ToInt32(XmlFontNode.Attributes["defaultpadding"].Value);
						tempDict.Add (attribute, padding);
					}

					// Checks if character has right padding, if not, applies default padding
					if (isChar && !tempDict.ContainsKey("paddingRight")) {
						string attribute = "paddingRight";
						int padding = Convert.ToInt32(XmlFontNode.Attributes["defaultpadding"].Value);
						tempDict.Add (attribute, padding);
					}

					// Checks character has ascender, if not, applies default
					if (isChar && !tempDict.ContainsKey("ascenderValue")) {
						string attribute = "ascenderValue";
						int ascender = Convert.ToInt32(XmlFontNode.Attributes["defaultascender"].Value);
						tempDict.Add (attribute, ascender);
					}

					// Adds values in the character to dictionary
					if( id != null) {
						CharacterValue.Add( id, tempDict );
					}					
					else {
						CharacterValue.Add (node.Name, tempDict );
						Debug.WriteLine (node.Name);
					}

					if (isChar) {
						int x = (int)elementPosition.X * BoxWidth + tempDict["paddingLeft"];
						int y = (int)elementPosition.Y * BoxHeight;
						int width = BoxWidth - tempDict["paddingRight"] - tempDict["paddingLeft"];
						int height = BoxHeight;
						Rectangle sourceRect = new Rectangle (x, y, width, height);
						CharacterRectangle.Add (id, sourceRect);
					}
				}
			}
		}

		// Methods

		private static void TintVisiblePixels(Texture2D texture, Color tint){
			Color[] newColor = new Color[texture.Width * texture.Height];
			texture.GetData (newColor);
			for (int i = 0; i < newColor.Length; i++) {
				if (newColor [i] != Color.Transparent) {
					newColor [i] = tint;
				}
			}
			texture.SetData<Color> (newColor);
		}
	}

	public class Camera2D
	{
		// Variables
		protected Matrix _tranformationMatrix;
		private Vector2 _position, _mousePosition, _origin;
		private float _zoom, _maxZoom, _minZoom;
		private float _rotation;

		public Camera2D (Viewport viewport)
		{
			_position = new Vector2 (viewport.Width * 0.5f, viewport.Height * 0.5f);
			_origin = new Vector2 (viewport.Width * 0.5f, viewport.Height * 0.5f);
			_zoom = 1.0f;
			_maxZoom = 100.0f;
			_minZoom = 0.1f;
			_rotation = 0;
		}

		// Methods

		public void Move(Vector2 amount){
			_position -= amount;
		}

		public Matrix Transformed(){
			_tranformationMatrix =
				Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
					Matrix.CreateRotationZ(Rotation) *
					Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
					Matrix.CreateTranslation(new Vector3(_origin.X, _origin.Y, 0));

			return _tranformationMatrix;
		}

		public Vector2 GetScreenPosition(Vector2 worldPosition)
		{
			return worldPosition - this.Position;
		}

		public Vector2 GetWorldPosition(Vector2 screenPosition)
		{
			return screenPosition + this.Position;
		}

		// Properties

		public Vector2 MousePosition
		{
			get {// return new Vector2 (Mouse.GetState ().X / Zoom, Mouse.GetState ().Y / Zoom) + (Position - _origin / Zoom); 
				Vector2 mousePos = new Vector2 (Mouse.GetState ().X / Zoom, Mouse.GetState ().Y / Zoom) + (Position - _origin / Zoom);
				return Vector2.Transform (mousePos, Matrix.CreateRotationZ (Rotation) );
			}
		}

		public float Zoom 
		{
			get { return _zoom; }
			set 
			{ 
				_zoom = value; 
				if (_zoom < _minZoom) _zoom = _minZoom; 
				if (_zoom > _maxZoom) _zoom = _maxZoom;
			}
		}

		public float Rotation
		{
			get { return _rotation; }
			set { _rotation = value; }
		}

		public Vector2 Position
		{
			get { return _position; }
			set { _position = value; }
		}

		public float MaxZoom
		{
			get { return _maxZoom; }
			set { _maxZoom = value; }
		}
	}


}

