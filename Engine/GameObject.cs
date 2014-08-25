using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Wormmy
{
	public class BaseGameObject
	{
		public string ID { get; set; }

		public BaseGameObject()
		{
		}
	}

	public class GameObject2DGroup
	{
		public List<GameObject2D> ObjectsList { get; private set; }

		public string name { get; set; }

		public GameObject2DGroup(string name = "group")
		{
			this.name = name;
			ObjectsList = new List<GameObject2D> ();
		}

		public void AddObject(GameObject2D newMember)
		{
			if (!ObjectsList.Contains (newMember)) {
				ObjectsList.Add (newMember);
			} else if (!newMember.Groups ().Contains (this)) {
				newMember.AddGroup (this);
			}
		}
	}

	public class GameObject2D : BaseGameObject
	{
		private List<GameObject2DGroup> _groups;

		public Vector2 Origin { get; set; }
		public Vector2 LocalPosition { get; set; }
		public Vector2 GlobalPosition { get { return LocalPosition + Origin; } }
		private Vector2 LinearVelocity { get; set; }
		public Texture2D Texture { get; set; }
		public Color DrawColor { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public float Scale { get; set; }
		public float Rotation { get; set; }
		public bool Visible { get; set; }
		public bool Active { get; set; }
		public GameObject2D Parent { get; set; }
		public List<GameObject2D> Children { get; set; }
		public Rectangle CollisionBox {
			get { return new Rectangle ((int)GlobalPosition.X, (int)GlobalPosition.Y, (int)(Width * Scale), (int)(Height *Scale) ); }
		}

		// public float Mass { get; set; }

		public GameObject2D(string ID, Vector2 origin, int width = 0, int height = 0, float scale = 1.0f)
		{
			this.ID = ID;
			this.Origin = origin;
			this.Width = width;
			this.Height = height;
			this.Scale = scale;
			LocalPosition = Vector2.Zero;
			LinearVelocity = Vector2.Zero;
			DrawColor = Color.White;
			Children = new List<GameObject2D> ();
			_groups = new List<GameObject2DGroup> ();
		}

		// Methods

		public void Update()
		{
			if (Active) 
			{
				LocalPosition += LinearVelocity;
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			if (Visible && Texture != null) 
			{
				spriteBatch.Draw (Texture, GlobalPosition, DrawColor);
			}
		}

		public void Move(Vector2 factor, float deltaTime)
		{
			LinearVelocity = factor * deltaTime;
		}

		public void AddGroup(GameObject2DGroup newGroup)
		{
			if (!_groups.Contains (newGroup)) 
			{
				_groups.Add (newGroup);
			} else if (!newGroup.ObjectsList.Contains (this)) {
				newGroup.AddObject (this);
			}
		}

		public List<GameObject2DGroup> Groups()
		{
			return _groups;
		}
	}
}

