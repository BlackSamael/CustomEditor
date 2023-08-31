using JSONCreator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PrefabJSONCreator : EditorWindow
{
    public ObjectDataHolder objectDataHolder;
    public string jsonText = "";
    public string fileName = string.Empty;
    public string filepath = string.Empty;
    private const string jsonFilePath = "Assets/Saved JSONs";

    [MenuItem("Create Template/Template Window")]
    public static void ShowWindow()
    {
        GetWindow<PrefabJSONCreator>(false, "Template Window Creator", true);
    }

    private void OnGUI()
    {
        GUILayout.Label("File Name: ");

        fileName = (GUILayout.TextField(fileName));

        if (GUILayout.Button("Create Json from GameObject"))
        {
            ConvertToJson(fileName);
            AssetDatabase.Refresh();
        }
        GUILayout.Space(10);

        GUILayout.Label("File Path To generate From: ");
        filepath = (GUILayout.TextField(filepath));
        if (GUILayout.Button("Generate From JSON"))
        {
            LoadDataFromTemplate(true);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Instantiate from Scriptable Object"))
        {
            LoadDataFromTemplate(false);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Convert the Json to ScriptableObject for modifications"))
        {
            if (string.IsNullOrEmpty(filepath))
            {
                EditorUtility.DisplayDialog("File not Found", "The given filePath is Invalid. Please check the filepath again.", "Ok");
            }
            else
            {
                jsonText = File.ReadAllText(filepath);

                objectDataHolder = Resources.Load("Template") as ObjectDataHolder;
                JsonUtility.FromJsonOverwrite(jsonText, objectDataHolder.objectData);
                EditorUtility.DisplayDialog("File Ready for Modification", "Please modify the file of type Scriptable Object Named \"Template\" from the Assets for modification.", "Ok");
            }
        }


        if (GUILayout.Button("Reset the Template"))
        {
            objectDataHolder = Resources.Load("Template") as ObjectDataHolder;
            objectDataHolder.objectData = null;
            objectDataHolder.objectData = new RectTransformDataOfObject();
        }
    }

    public void LoadDataFromTemplate(bool useJson)
    {
        var allTags = UnityEditorInternal.InternalEditorUtility.tags;

        if (useJson)
        {
            if (filepath.Contains(".json"))
            {
                if (File.Exists(filepath))
                {
                    objectDataHolder = CreateInstance<ObjectDataHolder>();

                    jsonText = File.ReadAllText(filepath);
                    objectDataHolder.objectData = new RectTransformDataOfObject();

                    try
                    {
                        JsonUtility.FromJsonOverwrite(jsonText, objectDataHolder.objectData);
                    }
                    catch (Exception e)
                    {

                        EditorUtility.DisplayDialog("Invalid JSON Exception", "Loaded JSON file is invalid. Please check", "Ok");
                        throw new InvalidDataException();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid File Exception", "The given filePath is invalid. Please check the filepath again.", "Ok");

                    throw new NullReferenceException("File Name is Invalid");
                }
            }

        }
        else
            objectDataHolder = Resources.Load("Template") as ObjectDataHolder;

        var canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            GameObject eventSys = new GameObject("EventSystem", typeof(StandaloneInputModule), typeof(EventSystem));

            GameObject gO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = gO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.vertexColorAlwaysGammaSpace = true;


        }

        var objectData = objectDataHolder.objectData;
        GameObject rootObject = new GameObject(objectData.name, typeof(RectTransform));
        rootObject.transform.SetParent(canvas.transform);
        var objRect = rootObject.GetComponent<RectTransform>();
        rootObject.layer = objectData.layer;


        if (string.IsNullOrEmpty(objectData.tag))
        {
            rootObject.tag = "Untagged";
        }
        else if (!allTags.Contains(objectData.tag))
        {
            Destroy(rootObject);
            EditorUtility.DisplayDialog("Tag Error", "\"" + objectData.tag + "\" This tag doesn't exist in Tag Manager. Kindly Add it in tag manager before creating prefab", "Okay");
            throw new InvalidDataException();
        }
        else
        {
            rootObject.tag = objectData.tag;
        }

        objRect.pivot = objectData.pivot;

        objRect.anchorMin = objectData.anchors.min;
        objRect.anchorMax = objectData.anchors.max;
        objRect.sizeDelta = new Vector2(objectData.rect.width, objectData.rect.height);

        objRect.anchoredPosition = objectData.position;
        objRect.localEulerAngles = objectData.rotation;
        objRect.localScale = objectData.scale;

        if (objectData.childData != null && objectData.childData.Count > 0)
            SetChildsData(rootObject, objectData);

        if (objectData.imageData != null && objectData.imageData.doesExist)
        {
            var imageData = objectData.imageData;
            var image = rootObject.AddComponent<Image>();
            image.color = imageData.color;
            image.sprite = AssetDatabase.LoadAssetAtPath(imageData.imagePath, typeof(Sprite)) as Sprite;

        }
        else if (objectData.rawImageData != null && objectData.rawImageData.doesExist)
        {

            var rawImageData = objectData.rawImageData;
            var image = rootObject.AddComponent<RawImage>();
            image.color = rawImageData.color;
            image.texture = AssetDatabase.LoadAssetAtPath(rawImageData.imagePath, typeof(Texture)) as Texture;

        }
        if (objectData.sliderData != null && objectData.sliderData.doesExist)
        {
            var sliderData = objectData.sliderData;
            var slider = rootObject.AddComponent<Slider>();

            slider.transition = sliderData.transitionType;
            switch (slider.transition)
            {
                case Selectable.Transition.None:
                    break;
                case Selectable.Transition.ColorTint:
                    slider.colors = sliderData.colorBlockData.GetColorBlock();
                    break;
                case Selectable.Transition.SpriteSwap:
                    slider.spriteState = sliderData.spriteStateData.GetSpriteState();
                    break;
                case Selectable.Transition.Animation:
                    slider.animationTriggers = sliderData.animationTriggersData;
                    break;
            }


            slider.direction = sliderData.direction;
            slider.value = sliderData.value;
            slider.minValue = sliderData.minValue;
            slider.maxValue = sliderData.maxValue;
            slider.wholeNumbers = sliderData.isWholeNumber;

            //slider.targetGraphic = sliderData.targetGraphicInstanceId;
            //slider.fillRect = sliderData.fillRectInstanceId;
            //slider.handleRect = sliderData.handleRectInstanceId;
        }

        void SetChildsData(GameObject rootGameObject, RectTransformDataOfObject parentObject)
        {
            var rootgoTransform = rootGameObject.GetComponent<Transform>();

            foreach (var item in parentObject.childData)
            {

                var childObjectData = item;
                var obj = new GameObject(childObjectData.name, typeof(RectTransform));
                obj.transform.SetParent(rootgoTransform);
                var objRect = obj.GetComponent<RectTransform>();
                obj.layer = childObjectData.layer;

                if (string.IsNullOrEmpty(childObjectData.tag))
                {
                    obj.tag = "Untagged";
                }
                else if (!allTags.Contains(childObjectData.tag))
                {
                    Destroy(rootObject);
                    EditorUtility.DisplayDialog("Tag Error", "\"" + objectData.tag + "\" This tag doesn't exist in Tag Manager. Kindly Add it in tag manager before creating prefab", "Okay");
                    return;
                }
                else
                {
                    obj.tag = childObjectData.tag;
                }

                objRect.pivot = childObjectData.pivot;

                objRect.anchorMin = childObjectData.anchors.min;
                objRect.anchorMax = childObjectData.anchors.max;

                objRect.sizeDelta = new Vector2(childObjectData.rect.width, childObjectData.rect.height);

                objRect.anchoredPosition = childObjectData.position;
                objRect.localEulerAngles = childObjectData.rotation;
                objRect.localScale = childObjectData.scale;


                if (item.childData != null && item.childData.Count > 0)
                    SetChildsData(obj, item);

                if (childObjectData.imageData != null && childObjectData.imageData.doesExist)
                {
                    var imageData = childObjectData.imageData;
                    var image = obj.AddComponent<Image>();
                    image.color = imageData.color;

                    if (imageData.imagePath.Contains("builtin"))
                    {

                    }
                    image.sprite = AssetDatabase.LoadAssetAtPath(imageData.imagePath, typeof(Sprite)) as Sprite;
                }
                else if (childObjectData.rawImageData != null && childObjectData.rawImageData.doesExist)
                {
                    var rawImageData = childObjectData.rawImageData;
                    var image = obj.AddComponent<RawImage>();
                    image.color = rawImageData.color;
                    image.texture = AssetDatabase.LoadAssetAtPath(rawImageData.imagePath, typeof(Texture)) as Texture;
                }

                if (childObjectData.sliderData != null && childObjectData.sliderData.doesExist)
                {
                    var sliderData = childObjectData.sliderData;
                    var slider = obj.AddComponent<Slider>();

                    slider.transition = sliderData.transitionType;
                    switch (slider.transition)
                    {
                        case Selectable.Transition.None:
                            break;
                        case Selectable.Transition.ColorTint:
                            slider.colors = sliderData.colorBlockData.GetColorBlock();
                            break;
                        case Selectable.Transition.SpriteSwap:
                            slider.spriteState = sliderData.spriteStateData.GetSpriteState();
                            break;
                        case Selectable.Transition.Animation:
                            slider.animationTriggers = sliderData.animationTriggersData;
                            break;
                    }

                    slider.direction = sliderData.direction;
                    slider.value = sliderData.value;
                    slider.minValue = sliderData.minValue;
                    slider.maxValue = sliderData.maxValue;
                    slider.wholeNumbers = sliderData.isWholeNumber;

                    //slider.targetGraphic = sliderData.targetGraphicInstanceId;
                    //slider.fillRect = sliderData.fillRectInstanceId;
                    //slider.handleRect = sliderData.handleRectInstanceId;
                }

            }


        }

    }

    public void ConvertToJson(string filePathForJson = null)
    {
        var allTags = UnityEditorInternal.InternalEditorUtility.tags;

        var objectData = CreateInstance<ObjectDataHolder>();
        var selectedObjectData = new RectTransformDataOfObject();
        UnityEngine.Object object1 = null;
        if (Selection.activeGameObject == null)
        {
            object1 = Selection.activeObject;
        }
        else
        {
            object1 = Selection.activeGameObject;
        }

        var gameObjectData = object1 as GameObject;
        var selectedRectTransform = gameObjectData.GetComponent<RectTransform>();
        if (selectedRectTransform != null)
        {
            selectedObjectData.instanceId = object1.GetHashCode();
            selectedObjectData.name = selectedRectTransform.name;
            selectedObjectData.layer = selectedRectTransform.gameObject.layer;

            selectedObjectData.tag = selectedRectTransform.tag;

            selectedObjectData.pivot = selectedRectTransform.pivot;

            selectedObjectData.anchors = new RectTransformDataOfObject.AnchorData();
            selectedObjectData.anchors.min = selectedRectTransform.anchorMin;
            selectedObjectData.anchors.max = selectedRectTransform.anchorMax;
            selectedObjectData.rect = selectedRectTransform.rect;

            selectedObjectData.position = selectedRectTransform.anchoredPosition;
            selectedObjectData.rotation = selectedRectTransform.localEulerAngles;
            selectedObjectData.scale = selectedRectTransform.localScale;

            if (selectedRectTransform.TryGetComponent(out Image image))
            {
                var imageData = new RectTransformDataOfObject.ImageData();
                imageData.color = image.color;
                imageData.imagePath = AssetDatabase.GetAssetPath(image.sprite.GetHashCode());
                imageData.doesExist = true;
                selectedObjectData.imageData = imageData;
            }
            else selectedObjectData.imageData = null;

            if (selectedRectTransform.TryGetComponent(out RawImage rawImage))
            {
                var rawImageData = new RectTransformDataOfObject.ImageData();
                rawImageData.doesExist = true;
                rawImageData.color = rawImage.color;
                rawImageData.imagePath = AssetDatabase.GetAssetPath(rawImage.texture.GetHashCode());
                selectedObjectData.rawImageData = rawImageData;
            }
            else selectedObjectData.rawImageData = null;

            if (selectedRectTransform.TryGetComponent(out Slider slider))
            {
                RectTransformDataOfObject.SliderData sliderData = new RectTransformDataOfObject.SliderData();
                sliderData.transitionType = slider.transition;
                sliderData.doesExist = true;
                switch (sliderData.transitionType)
                {
                    case Selectable.Transition.None:
                        break;
                    case Selectable.Transition.ColorTint:
                        sliderData.colorBlockData = new RectTransformDataOfObject.SliderData.ColorBlockData(slider.colors);
                        break;
                    case Selectable.Transition.SpriteSwap:
                        sliderData.spriteStateData = new RectTransformDataOfObject.SliderData.SpriteStateData(slider.spriteState);
                        break;
                    case Selectable.Transition.Animation:
                        sliderData.animationTriggersData = slider.animationTriggers;
                        break;
                }
                sliderData.maxValue = slider.maxValue;
                sliderData.minValue = slider.minValue;
                sliderData.value = slider.value;
                sliderData.isWholeNumber = slider.wholeNumbers;
                sliderData.direction = slider.direction;
                sliderData.fillRectInstanceId = slider.fillRect.GetHashCode();
                sliderData.handleRectInstanceId = slider.handleRect.GetHashCode();
                sliderData.targetGraphicInstanceId = slider.targetGraphic.GetHashCode();
                selectedObjectData.sliderData = sliderData;
            }
            else selectedObjectData.sliderData = null;
            SetChildsData(object1 as GameObject, selectedObjectData);
        }
        else throw new InvalidOperationException("List of child doesn't have rect transforms");

        void SetChildsData(GameObject rootGameObject, RectTransformDataOfObject parentObject)
        {
            parentObject.childData = new List<RectTransformDataOfObject>();

            var rootgoTransform = rootGameObject.GetComponent<Transform>();
            for (int i = 0; i < rootgoTransform.childCount; i++)
            {
                var rectTransform = rootgoTransform.GetChild(i).GetComponent<RectTransform>();

                RectTransformDataOfObject newChild = new RectTransformDataOfObject();

                newChild.instanceId = rectTransform.GetHashCode();
                newChild.name = rectTransform.name;
                newChild.layer = rectTransform.gameObject.layer;
                newChild.tag = rectTransform.tag;
                newChild.pivot = rectTransform.pivot;
                newChild.anchors = new RectTransformDataOfObject.AnchorData();
                newChild.anchors.min = rectTransform.anchorMin;
                newChild.anchors.max = rectTransform.anchorMax;
                newChild.position = rectTransform.anchoredPosition;
                newChild.rotation = rectTransform.localEulerAngles;
                newChild.scale = rectTransform.lossyScale;
                newChild.rect = rectTransform.rect;

                if (rectTransform.TryGetComponent(out Image image))
                {
                    var imageData = new RectTransformDataOfObject.ImageData();
                    imageData.color = image.color;
                    imageData.imagePath = AssetDatabase.GetAssetPath(image.sprite.GetHashCode());
                    imageData.doesExist = true;
                    newChild.imageData = imageData;
                }

                if (rectTransform.TryGetComponent(out RawImage rawImage))
                {
                    var rawImageData = new RectTransformDataOfObject.ImageData();
                    rawImageData.color = rawImage.color;
                    rawImageData.imagePath = AssetDatabase.GetAssetPath(rawImage.texture.GetHashCode());
                    rawImageData.doesExist = true;
                    newChild.rawImageData = rawImageData;
                }

                if (rectTransform.TryGetComponent(out Slider slider))
                {
                    RectTransformDataOfObject.SliderData sliderData = new RectTransformDataOfObject.SliderData();
                    sliderData.transitionType = slider.transition;
                    sliderData.doesExist = true;

                    switch (sliderData.transitionType)
                    {
                        case Selectable.Transition.None:
                            break;
                        case Selectable.Transition.ColorTint:
                            sliderData.colorBlockData = new RectTransformDataOfObject.SliderData.ColorBlockData(slider.colors);
                            break;
                        case Selectable.Transition.SpriteSwap:
                            sliderData.spriteStateData = new RectTransformDataOfObject.SliderData.SpriteStateData(slider.spriteState);
                            break;
                        case Selectable.Transition.Animation:
                            sliderData.animationTriggersData = slider.animationTriggers;
                            break;
                    }
                    sliderData.maxValue = slider.maxValue;
                    sliderData.minValue = slider.minValue;
                    sliderData.value = slider.value;
                    sliderData.isWholeNumber = slider.wholeNumbers;
                    sliderData.direction = slider.direction;
                    sliderData.fillRectInstanceId = slider.fillRect.GetHashCode();
                    sliderData.handleRectInstanceId = slider.handleRect.GetHashCode();
                    sliderData.targetGraphicInstanceId = slider.targetGraphic.GetHashCode();
                    newChild.sliderData = sliderData;
                }

                parentObject.childData.Add(newChild);

                if (rectTransform.childCount > 0)
                    SetChildsData(rectTransform.gameObject, newChild);
            }
        }

        objectData.objectData = selectedObjectData;

        var data = Resources.Load("Template") as ObjectDataHolder;
        data.objectData = objectData.objectData;

        if (!string.IsNullOrEmpty(filePathForJson))
        {
            var str = JsonUtility.ToJson(selectedObjectData, true);
            if (!Directory.Exists(jsonFilePath))
            {
                Directory.CreateDirectory(jsonFilePath);
            }

            File.WriteAllText(Path.Combine(jsonFilePath, filePathForJson + ".json"), str);
        }
        else
        {
            EditorUtility.DisplayDialog("Save Json Error", "Enter File Path for Saving Json", "okay");
        }

    }
}
