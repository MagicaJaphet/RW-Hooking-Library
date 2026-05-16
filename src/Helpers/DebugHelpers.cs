using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MagicaHookingLibrary.Helpers;
public class DebugHelpers
{
	// FEATURE: Debug lines (pointers / circles / line grapher)
	public class DebugRect
	{
		private FSprite[] sprites;
		private FSprite anchor;
		private Vector2 position;
		private Vector2 size;

		public DebugRect(Vector2 position, Vector2 size, FSprite anchor = null, Color color = default, bool filled = false, float fillAlpha = 0.5f)
		{
			if (color == default) color = Color.white;
			this.anchor = anchor;
			this.position = position;
			this.size = size;

			sprites = [
			new("pixel") {color = color, scaleX = size.x, anchorX = 0f}, // top
			new("pixel") {color = color, scaleX = size.x, anchorX = 0f}, // bottom
			new("pixel") {color = color, scaleY = size.y, anchorY = 0f}, // left
			new("pixel") {color = color, scaleY = size.y, anchorY = 0f}, // right
			filled ? new("pixel") {color = color, scaleX = size.x, scaleY = size.y, anchorX = 0f, anchorY = 1f, alpha = fillAlpha} : null
			];

			foreach (var sprite in sprites)
			{
				if (sprite != null)
					Futile.stage.AddChild(sprite);
			}

			Update();
		}

		public void Update()
		{
			Vector2 addPos = position + (anchor == null ? new() : anchor.GetPosition());
			for (int i = 0; i < sprites.Length; i++)
			{
				var sprite = sprites[i];
				if (sprite != null)
				{
					Vector2 spritePos = addPos;
					if (i == 1) // bottom horizontal line
					{
						spritePos.y += size.y;
					}
					if (i == 3) // right vertical line
					{
						spritePos.x += size.x;
					}
					sprite.SetPosition(spritePos);
				}
			}
		}

		public void SetLineWidth(float scale)
		{
			for (int i = 0; i < sprites.Length; i++)
			{
				var sprite = sprites[i];
				if (sprite != null)
				{
					if (i == 2 || i == 3) // vertical lines
					{
						sprite.scaleX = scale;
					}
					else
					{
						sprite.scaleY = scale;
					}
				}
			}
		}

		public void SetVisibility(bool hide)
		{
			for (int i = 0; i < sprites.Length; i++)
			{
				var sprite = sprites[i];
				if (sprite != null)
				{
					sprite.isVisible = !hide;
				}
			}
		}

		public void MoveToFront()
		{
			for (int i = 0; i < sprites.Length; i++)
			{
				sprites[i]?.MoveToFront();
			}
		}

		public void Destroy()
		{
			foreach (var sprite in sprites)
			{
				sprite?.RemoveFromContainer();
			}
			sprites = null;
		}
	}
}
