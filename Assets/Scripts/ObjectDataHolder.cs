
using JSONCreator;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Prefabs/Create Prefab Data", fileName = "PrefabDataHolder")]
public class ObjectDataHolder : ScriptableObject
{
	public RectTransformDataOfObject objectData;
}

namespace JSONCreator
{
	[Serializable]
	public class RectTransformDataOfObject
	{
		[NonSerialized] public int instanceId;
		public string name;
		public int layer;
		public string tag;
		public Vector3 position;
		public Vector3 rotation;
		public Vector3 scale;
		public Vector2 pivot;
		public AnchorData anchors;
		public Rect rect;

		public ImageData imageData;
		public ImageData rawImageData;
		public SliderData sliderData;
		public List<RectTransformDataOfObject> childData;

		[Serializable]
		public class ImageData
		{
			public int instanceId;
			public bool doesExist;
			public Color32 color;
			public string imagePath;
		}

		[Serializable]
		public class AnchorData
		{
			public Vector2 min;
			public Vector2 max;
		}

		[Serializable]
		public class SliderData
		{
			public bool doesExist;
			public Selectable.Transition transitionType;
			public ColorBlockData colorBlockData;
			public SpriteStateData spriteStateData;
			public AnimationTriggers animationTriggersData;

			public int targetGraphicInstanceId;

			public float value;
			public float minValue;
			public float maxValue;
			public bool isWholeNumber;
			public Slider.Direction direction;
			public int fillRectInstanceId;
			public int handleRectInstanceId;

			[Serializable]
			public class ColorBlockData
			{
				public Color normalColor;
				public Color highlightedColor;
				public Color pressedColor;
				public Color selectedColor;
				public Color disabledColor;
				public float colorMultiplier;
				public float fadeDuration;

				public ColorBlockData(ColorBlock colorBlock)
				{
					normalColor = colorBlock.normalColor;
					highlightedColor = colorBlock.highlightedColor;
					pressedColor = colorBlock.pressedColor;
					selectedColor = colorBlock.selectedColor;
					disabledColor = colorBlock.disabledColor;
					colorMultiplier = colorBlock.colorMultiplier;
					fadeDuration = colorBlock.fadeDuration;
				}

				public ColorBlock GetColorBlock()
				{
					ColorBlock colorBlock = new ColorBlock();
					colorBlock.normalColor = normalColor;
					colorBlock.highlightedColor = highlightedColor;
					colorBlock.pressedColor = pressedColor;
					colorBlock.selectedColor = selectedColor;
					colorBlock.disabledColor = disabledColor;
					colorBlock.colorMultiplier = colorMultiplier;
					colorBlock.fadeDuration = fadeDuration;

					return colorBlock;
				}
			}

			[Serializable]
			public class SpriteStateData
			{
				public string disabledSprite;
				public string selectedSprite;
				public string pressedSprite;
				public string highlightedSprite;
				public SpriteStateData(SpriteState spriteState)
				{

					if (!string.IsNullOrEmpty(disabledSprite))
						disabledSprite = AssetDatabase.GetAssetPath(spriteState.disabledSprite.GetHashCode());

					if (!string.IsNullOrEmpty(selectedSprite))
						selectedSprite = AssetDatabase.GetAssetPath(spriteState.selectedSprite.GetHashCode());

					if (!string.IsNullOrEmpty(pressedSprite))
						pressedSprite = AssetDatabase.GetAssetPath(spriteState.pressedSprite.GetHashCode());

					if (!string.IsNullOrEmpty(highlightedSprite))
						highlightedSprite = AssetDatabase.GetAssetPath(spriteState.highlightedSprite.GetHashCode());
				}

				public SpriteState GetSpriteState()
				{
					SpriteState spriteState = new SpriteState();

					if (!string.IsNullOrEmpty(disabledSprite))
						spriteState.disabledSprite = AssetDatabase.LoadAssetAtPath(disabledSprite, typeof(Sprite)) as Sprite;

					if (!string.IsNullOrEmpty(selectedSprite))
						spriteState.selectedSprite = AssetDatabase.LoadAssetAtPath(selectedSprite, typeof(Sprite)) as Sprite;

					if (!string.IsNullOrEmpty(pressedSprite))
						spriteState.pressedSprite = AssetDatabase.LoadAssetAtPath(pressedSprite, typeof(Sprite)) as Sprite;

					if (!string.IsNullOrEmpty(highlightedSprite))
						spriteState.highlightedSprite = AssetDatabase.LoadAssetAtPath(highlightedSprite, typeof(Sprite)) as Sprite;

					return spriteState;

				}
			}

		}

	}
}