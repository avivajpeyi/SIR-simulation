/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Window_Graph : MonoBehaviour {

    private static Window_Graph instance;

    [SerializeField] private Sprite dotSprite;
    private RectTransform graphContainer;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform dashContainer;
    private RectTransform dashTemplateX;
    private RectTransform dashTemplateY;
    private List<GameObject> gameObjectList;
    private Dictionary<string, List<IGraphVisualObject>> graphVisualObjectList;
    private GameObject tooltipGameObject;
    private List<RectTransform> yLabelList;

    // Cached values
    private Dictionary<string, List<int>> valueList;
    private IGraphVisual graphVisual;
    private int maxVisibleValueAmount;
    private Func<int, string> getAxisLabelX;
    private Func<float, string> getAxisLabelY;
    private float xSize;
    private bool startYScaleAtZero;
    private bool endYScaleAtHundred;

    private void Awake() {
        instance = this;
        // Grab base objects references
        graphContainer = transform.Find("graphContainer").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("labelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("labelTemplateY").GetComponent<RectTransform>();
        dashContainer = graphContainer.Find("dashContainer").GetComponent<RectTransform>();
        dashTemplateX = dashContainer.Find("dashTemplateX").GetComponent<RectTransform>();
        dashTemplateY = dashContainer.Find("dashTemplateY").GetComponent<RectTransform>();
        tooltipGameObject = graphContainer.Find("tooltip").gameObject;

        startYScaleAtZero = true;
        endYScaleAtHundred = true;
        gameObjectList = new List<GameObject>();
        yLabelList = new List<RectTransform>();
        graphVisualObjectList = new Dictionary<string, List<IGraphVisualObject>>();

        HideTooltip();
    }

    public static void ShowTooltip_Static(string tooltipText, Vector2 anchoredPosition) {
        instance.ShowTooltip(tooltipText, anchoredPosition);
    }

    private void ShowTooltip(string tooltipText, Vector2 anchoredPosition) {
        // Show Tooltip GameObject
        tooltipGameObject.SetActive(true);

        tooltipGameObject.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;

        Text tooltipUIText = tooltipGameObject.transform.Find("text").GetComponent<Text>();
        tooltipUIText.text = tooltipText;

        float textPaddingSize = 4f;
        Vector2 backgroundSize = new Vector2(
            tooltipUIText.preferredWidth + textPaddingSize * 2f, 
            tooltipUIText.preferredHeight + textPaddingSize * 2f
        );

        tooltipGameObject.transform.Find("background").GetComponent<RectTransform>().sizeDelta = backgroundSize;

        // UI Visibility Sorting based on Hierarchy, SetAsLastSibling in order to show up on top
        tooltipGameObject.transform.SetAsLastSibling();
    }

    public static void HideTooltip_Static() {
        instance.HideTooltip();
    }

    private void HideTooltip() {
        tooltipGameObject.SetActive(false);
    }

    public void ShowGraph(Dictionary<string, List<int>> valueList, IGraphVisual 
    graphVisual, 
    int  
    maxVisibleValueAmount = 20) {
        Debug.Log("Show Graph");
        this.valueList = valueList;
        this.graphVisual = graphVisual;
        
        this.maxVisibleValueAmount = valueList.First().Value.Count;
        maxVisibleValueAmount = valueList.First().Value.Count;
        getAxisLabelX = delegate (int _i) { return _i.ToString(); };
        getAxisLabelY = delegate (float _f) { return Mathf.RoundToInt(_f).ToString(); };
        

        // Clean up previous graph
        foreach (GameObject gameObject in gameObjectList) {
            Destroy(gameObject);
        }
        gameObjectList.Clear();
        yLabelList.Clear();

        foreach (var gvList in graphVisualObjectList)
        {
            foreach (var graphVisualObject in gvList.Value) {
                graphVisualObject.CleanUp();
            }
            gvList.Value.Clear();
        }
        graphVisualObjectList.Clear();

        if (graphVisual != null)
            graphVisual.CleanUp();

        // Grab the width and height from the container
        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;
        
        float yMinimum, yMaximum;
        float xMinimum, xMaximum;
        CalculateYScale(out yMinimum, out yMaximum);
        CalculateXScale(out xMinimum, out xMaximum);

        // Set the distance between each point on the graph 
        xSize = graphWidth / (maxVisibleValueAmount + 1);
    
        // Cycle through all visible data points
        foreach (var dataset in valueList)
        {
            graphVisualObjectList.Add(dataset.Key, new List<IGraphVisualObject>());
            int xIndex = 0;
            for (int i = 0; i < dataset.Value.Count; i++) 
            {
                float xPosition = xSize + xIndex * xSize;
                float yPosition = ((dataset.Value[i] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;
                if (float.IsNaN(yPosition))
                    yPosition = 0;
                
                // Add data point visual
                string tooltipText = getAxisLabelY(dataset.Value[i]);
                IGraphVisualObject graphVisualObject = graphVisual.CreateGraphVisualObject(new Vector2(xPosition, yPosition), xSize, tooltipText, dataset.Key);
                graphVisualObjectList[dataset.Key].Add(graphVisualObject);
                xIndex++;
            }
        }

        
        

        // Set up separators on the y axis
        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; i++) {
            
            // Duplicate the x label template
            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainer, false);
            labelX.gameObject.SetActive(true);
            float normalizedValue = i * 1f / separatorCount;
            labelX.anchoredPosition = new Vector2(normalizedValue * graphWidth, -7f);
            labelX.GetComponent<Text>().text = getAxisLabelX
            (Mathf.RoundToInt( normalizedValue*xMaximum));

            gameObjectList.Add(labelX.gameObject);
            
            // Duplicate the x dash template
            RectTransform dashX = Instantiate(dashTemplateX);
            dashX.SetParent(dashContainer, false);
            dashX.gameObject.SetActive(true);
            dashX.anchoredPosition = new Vector2(normalizedValue * graphWidth, -3f);
            gameObjectList.Add(dashX.gameObject);
            
            
            // Duplicate the label template
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            normalizedValue = i * 1f / separatorCount;
            labelY.anchoredPosition = new Vector2(-7f, normalizedValue * graphHeight);
            labelY.GetComponent<Text>().text = getAxisLabelY(yMinimum + (normalizedValue * (yMaximum - yMinimum)));
            yLabelList.Add(labelY);
            gameObjectList.Add(labelY.gameObject);

            // Duplicate the dash template
            RectTransform dashY = Instantiate(dashTemplateY);
            dashY.SetParent(dashContainer, false);
            dashY.gameObject.SetActive(true);
            dashY.anchoredPosition = new Vector2(-4f, normalizedValue * graphHeight);
            gameObjectList.Add(dashY.gameObject);
        }
    }

     public void UpdateValue(int index, Dictionary<string, int> value) {
        float yMinimum, yMaximum;
        CalculateYScale(out yMinimum, out yMaximum);
        
                
        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;


        foreach (var dataset in valueList)
        {
            dataset.Value[index] = value[dataset.Key];
             
            // Y Scale did not change, update only this value
            float xPosition = xSize + index * xSize;
            float yPosition = ((value[dataset.Key] - yMinimum) / (yMaximum - yMinimum)) * graphHeight;

            // Add data point visual
            string tooltipText = getAxisLabelY(value[dataset.Key]);
            graphVisualObjectList[dataset.Key][index].SetGraphVisualObjectInfo(new Vector2(xPosition, yPosition), xSize, tooltipText);
            if (yPosition >= 0)
                graphVisualObjectList[dataset.Key][index].Show();
        }
     }

    private void CalculateYScale(out float yMinimum, out float yMaximum) {
        yMinimum = 0f;
        yMaximum = 100f;
    }


    private void CalculateXScale(out float xMinimum, out float xMaximum) {
        xMaximum = valueList.First().Value.Count;
        xMinimum = 0;
    }
    
    
    /*
     * Interface definition for showing visual for a data point
     * */
    public interface IGraphVisual {

        IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float 
        graphPositionWidth, string tooltipText, string id);
        void CleanUp();

    }

    /*
     * Represents a single Visual Object in the graph
     * */
    public  interface IGraphVisualObject {

        void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText);

        void Show();
        void Hide();
        void CleanUp();

    }
    

    /*
     * Displays data points as a Line Graph
     * */
    public class LineGraphVisual : IGraphVisual {

        private RectTransform graphContainer;
        private Sprite dotSprite;
        private LineGraphVisualObject lastLineGraphVisualObject;
        private Dictionary<String, Color> dotColor;
        private Dictionary<String, Color>  dotConnectionColor;
        

        public LineGraphVisual(RectTransform graphContainer, Sprite dotSprite, 
            Dictionary<String, Color> dotColor, Dictionary<String, Color>  dotConnectionColor) {
            this.graphContainer = graphContainer;
            this.dotSprite = dotSprite;
            this.dotColor = dotColor;
            this.dotConnectionColor = dotConnectionColor;
            
            lastLineGraphVisualObject = null;
        }

        public void CleanUp() {
            lastLineGraphVisualObject = null;
        }




        public IGraphVisualObject CreateGraphVisualObject(Vector2 graphPosition, float 
        graphPositionWidth, string tooltipText, string id) {
            GameObject dotGameObject = CreateDot(graphPosition, id);
            GameObject dotConnectionGameObject = null;
            if (lastLineGraphVisualObject != null) {
                if (lastLineGraphVisualObject.CompareId(id))
                {
                    Vector2 curPos = dotGameObject.GetComponent<RectTransform>().anchoredPosition;
                    Vector2 lastPos = lastLineGraphVisualObject.GetGraphPosition();
                    dotConnectionGameObject = CreateDotConnection(lastPos, curPos, id);
                }
                
            }
            
            LineGraphVisualObject lineGraphVisualObject = new LineGraphVisualObject(dotGameObject, dotConnectionGameObject, lastLineGraphVisualObject, id);
            lineGraphVisualObject.SetGraphVisualObjectInfo(graphPosition, graphPositionWidth, tooltipText);
            
            if  (graphPosition.y < 0)
                lineGraphVisualObject.Hide();
            else
                lineGraphVisualObject.Show();
            
            lastLineGraphVisualObject = lineGraphVisualObject;
            
            
            
            return lineGraphVisualObject;
        }

        private GameObject CreateDot(Vector2 anchoredPosition, string id) {
            GameObject gameObject = new GameObject("dot", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().sprite = dotSprite;
            gameObject.GetComponent<Image>().color = dotColor[id];
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(5, 5);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            
            // Add Button_UI Component which captures UI Mouse Events
            Button_UI dotButtonUI = gameObject.AddComponent<Button_UI>();

            return gameObject;
        }

        private GameObject CreateDotConnection(Vector2 dotPositionA, Vector2 
        dotPositionB, string id) {
            GameObject gameObject = new GameObject("dotConnection", typeof(Image));
            gameObject.transform.SetParent(graphContainer, false);
            gameObject.GetComponent<Image>().color = dotConnectionColor[id];
            gameObject.GetComponent<Image>().raycastTarget = false;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            Vector2 dir = (dotPositionB - dotPositionA).normalized;
            float distance = Vector2.Distance(dotPositionA, dotPositionB);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(distance, 3f);
            rectTransform.anchoredPosition = dotPositionA + dir * distance * .5f;
            rectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
            return gameObject;
        }


        public class LineGraphVisualObject : IGraphVisualObject {

            public event EventHandler OnChangedGraphVisualObjectInfo;

            private GameObject dotGameObject;
            private GameObject dotConnectionGameObject;
            private string id;
            private LineGraphVisualObject lastVisualObject;
            
            public bool CompareId(string id)
            {
                return String.Compare(id, this.id) == 0;
            }

            public LineGraphVisualObject(GameObject dotGameObject, GameObject 
            dotConnectionGameObject, LineGraphVisualObject lastVisualObject, string id) {
                this.dotGameObject = dotGameObject;
                this.dotConnectionGameObject = dotConnectionGameObject;
                this.lastVisualObject = lastVisualObject;
                this.id = id;

                if (lastVisualObject != null) {
                    lastVisualObject.OnChangedGraphVisualObjectInfo += LastVisualObject_OnChangedGraphVisualObjectInfo;
                }
            }
            
            public void Hide()
            {
                dotGameObject.GetComponent<Button_UI>().enabled = false;
                dotGameObject.GetComponent<Image>().enabled = false;
                if (dotConnectionGameObject!=null)
                    dotConnectionGameObject.GetComponent<Image>().enabled = false;
            }

            public void Show()
            {
                dotGameObject.GetComponent<Button_UI>().enabled = true;
                dotGameObject.GetComponent<Image>().enabled = true;
                if (dotConnectionGameObject!=null)
                    dotConnectionGameObject.GetComponent<Image>().enabled = true;

            }

            private void LastVisualObject_OnChangedGraphVisualObjectInfo(object sender, EventArgs e) {
                UpdateDotConnection();
            }

            public void SetGraphVisualObjectInfo(Vector2 graphPosition, float graphPositionWidth, string tooltipText) {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = graphPosition;

                UpdateDotConnection();

                Button_UI dotButtonUI = dotGameObject.GetComponent<Button_UI>();

                // Show Tooltip on Mouse Over
                dotButtonUI.MouseOverOnceFunc = () => {
                    ShowTooltip_Static(tooltipText, graphPosition);
                };
            
                // Hide Tooltip on Mouse Out
                dotButtonUI.MouseOutOnceFunc = () => {
                    HideTooltip_Static();
                };

                if (OnChangedGraphVisualObjectInfo != null) OnChangedGraphVisualObjectInfo(this, EventArgs.Empty);
            }

            public void CleanUp() {
                Destroy(dotGameObject);
                Destroy(dotConnectionGameObject);
            }

            public Vector2 GetGraphPosition() {
                RectTransform rectTransform = dotGameObject.GetComponent<RectTransform>();
                return rectTransform.anchoredPosition;
            }



            private void UpdateDotConnection() {
                if (dotConnectionGameObject != null) {
                    RectTransform dotConnectionRectTransform = dotConnectionGameObject.GetComponent<RectTransform>();
                    Vector2 dir = (lastVisualObject.GetGraphPosition() - GetGraphPosition()).normalized;
                    float distance = Vector2.Distance(GetGraphPosition(), lastVisualObject.GetGraphPosition());
                    dotConnectionRectTransform.sizeDelta = new Vector2(distance, 3f);
                    dotConnectionRectTransform.anchoredPosition = GetGraphPosition() + dir * distance * .5f;
                    dotConnectionRectTransform.localEulerAngles = new Vector3(0, 0, UtilsClass.GetAngleFromVectorFloat(dir));
                }
            }

        }

    }

}
