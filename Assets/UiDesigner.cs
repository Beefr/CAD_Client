using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    /// <summary>
    /// it modifies canvas
    /// </summary>
    public class UiDesigner: MonoBehaviour
    {
        private GameObject objPrinted; // the object we are interested in showing the caracteristics

        private TMP_Text txtPerso; // txt to show ? where ?
        private int lineNumber = 0; // current line of the canva
        private List<string> rightLines = new List<string>(); 
        private Canvas SelectableUI; // the ui 
        private Canvas Values; // ui of the right
        private Canvas NameOfTheValues; // ui of the left
        private GameObject newButton; // buttons in the ui
        private int spacing = 5; // spacement between lines
        private int canvaUpdated = 0; // sort of timer
        private int numberOfLines = 0; // number of lines TODO: verifies if it is useless
        private string canvasText; // txt to show
        private int lineCount = 0; // number of lines
        private string[] myLines=new string[0]; // the lines to show

        private int valueChanged = 0; // sort of timer

        /// <summary>
        /// initialize things
        /// </summary>
        void Start()
        {
            try
            {
                File.Delete(@"Content.txt");
            }
            catch (Exception e) { /* file not existing */ }
            

            txtPerso = GameObject.Find("CanvasPerso").GetComponent<TMP_Text>(); // TODO: verifies if it is useless
            SelectableUI = GameObject.Find("SelectableUI").GetComponent<Canvas>();
            Values = SelectableUI.transform.Find("Values").GetComponent<Canvas>();
            NameOfTheValues = SelectableUI.transform.Find("NameOfTheValues").GetComponent<Canvas>();
            Debug.Log(NameOfTheValues);
        }

        /// <summary>
        /// set the object that we want to show caracteristics
        /// </summary>
        /// <param name="obj"></param>
        public void setObj(GameObject obj) { objPrinted = obj; }

        /// <summary>
        /// each tick u can move the line that is highlighted, and change the value highlighted
        /// </summary>
        void Update()
        {

            // _____________ PANEL SELECTION TO UPDATE PROPERTIES __________________________________________________________
            canvaUpdated++;
            if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickDown) || Input.GetKeyDown(KeyCode.S))
            {
                if (canvaUpdated > 15)
                {
                    lineNumber++;
                    if (lineNumber >= rightLines.Count)
                    {
                        lineNumber = 0;
                    }
                    UpdateCanva();
                    canvaUpdated = 0;
                }
            }
            if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickUp) || Input.GetKeyDown(KeyCode.Z))
            {
                if (canvaUpdated > 15)
                {
                    lineNumber--;
                    if (lineNumber < 0)
                    {
                        var val = rightLines.Count - 1;
                        lineNumber = (val >= 0) ? val : 0;
                    }
                    UpdateCanva();
                    canvaUpdated = 0;
                }
            }
            // upgrade/downgrade caracteristics of the object
            valueChanged++;
            if (valueChanged > 15)
            {
                if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickLeft) || Input.GetKeyDown(KeyCode.Q))
                {
                    downgradeItem();
                    valueChanged = 0;
                }
                if (OVRInput.Get(OVRInput.Button.PrimaryThumbstickRight) || Input.GetKeyDown(KeyCode.D))
                {
                    upgradeItem();
                    valueChanged = 0;
                }
            }

            
        }


        /// <summary>
        /// the item selected gets downgraded
        /// </summary>
        private void downgradeItem()
        {

            List<Transform> lines = GetAllChilds(Values);
            string value = lines[lineNumber].GetComponentInChildren<TMP_Text>().text;

            if (int.TryParse(value, out int j) && lineNumber!=0)
            {
                lines[lineNumber].GetComponentInChildren<TMP_Text>().text = (j - 1).ToString();
            }
            UpdateObject();
        }

        /// <summary>
        /// the item selected gets upgraded
        /// </summary>
        private void upgradeItem()
        {
            
            List<Transform> lines = GetAllChilds(Values);
            string value =lines[lineNumber].GetComponentInChildren<TMP_Text>().text;

            if (int.TryParse(value, out int j) && lineNumber != 0)
            {
                lines[lineNumber].GetComponentInChildren<TMP_Text>().text = (j + 1).ToString();
            }
            UpdateObject();
        }




        /// <summary>
        /// get the string between the two given strings
        /// </summary>
        /// <param name="strSource">source you are interested in extract something</param>
        /// <param name="strStart">first string</param>
        /// <param name="strEnd">second string</param>
        /// <returns></returns>
        public static string GetBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        } // credit https://stackoverflow.com/questions/10709821/find-text-in-string-with-c-sharp

        /// <summary>
        /// take the string and update your canva
        /// </summary>
        public void convertStringToCanvas()
        {

            convertStringToCanvaString();
            UpdateCanvas();

        }

        /// <summary>
        /// read Content.txt and put it in a format to be able to update our canvas
        /// </summary>
        private void convertStringToCanvaString()
        {
            string content = "";
            try
            {
                content = System.IO.File.ReadAllText(@"Content.txt");

            }
            catch (Exception e) { Debug.Log("File still not created"); }


            string monString = "Name: " + GetBetween(content, "<a:name>", "</a:name>") + "\n\n";
            monString = monString + "<ID \n    " + GetBetween(content, "<ID>", "</ID>") + "\nID> \n\n";
            monString = monString + "<Position " + GetBetween(content, "<position xmlns:a=\"http://schemas.datacontract.org/2004/07/UnityEngine\">", "</position>") + "Position> \n\n";
            monString = monString + "<Scale " + GetBetween(content, "<scale xmlns:a=\"http://schemas.datacontract.org/2004/07/UnityEngine\">", "</scale>") + "Scale> \n\n";

            canvasText = monString;

        }

        /// <summary>
        /// update the canvas by wether changing their text or build buttons and so on
        /// </summary>
        private void UpdateCanvas()
        {

            TMP_Text txt = GameObject.Find("Canvas").GetComponent<TMP_Text>();
            if (txt != null)
                txt.text = canvasText;

            TMP_Text txtPerso = GameObject.Find("CanvasPerso").GetComponent<TMP_Text>();
            if (txtPerso != null)
                txtPerso.text = canvasText;


            SelectableUIBuilder();

        }

        
        /// <summary>
        /// build the SelectableUi
        /// </summary>
        private void SelectableUIBuilder()
        {
            lineCount = 0;

            // delete the previous ones
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("UiLine-Empty"))
                GameObject.Destroy(go);

            foreach (GameObject go in GameObject.FindGameObjectsWithTag("UiLine"))
                GameObject.Destroy(go);

            myLines = canvasText.Split("\n\r".ToCharArray());

            foreach (string line in myLines)
            {
                if (line.Length > 0)
                {
                    int i;
                    for (i = 0; i < line.Length; i++)
                    {
                        if (line[i] != ' ')
                            break; // we want to know what is the indentation of the text
                    }
                    switch (i) // depending on the indentation...
                    {
                        case 0:
                            AddLine(line, NameOfTheValues);
                            AddLine("", Values);
                            lineCount++;
                            break;
                        case 2:
                            AddLine(line, NameOfTheValues);
                            AddLine("", Values);
                            lineCount++;
                            break;
                        case 4:
                            AddLine("", NameOfTheValues);
                            string newLine = line.Contains("<") ? GetBetween(line, ">", "<") : line;
                            AddLine(newLine, Values);
                            rightLines.Add(newLine);
                            lineCount++;
                            break;
                        default:
                            break;
                    }
                }

            }
            
            if (rightLines.Count > 0)
                UpdateCanva();
            numberOfLines = myLines.Length;
        }
        
        /// <summary>
        /// add a line to the canva
        /// </summary>
        /// <param name="txt">the txt that will appear in the line of the canva</param>
        /// <param name="C">the canva getting updated</param>
        private void AddLine(string txt, Canvas C)
        {
            newButton = (GameObject)Instantiate(Resources.Load("MesPrefabs/UiLine"), new Vector3(0, -lineCount * spacing, 0), Quaternion.identity);
            newButton.transform.SetParent(C.transform, false);


            newButton.GetComponentInChildren<TMP_Text>().text = txt;
            newButton.GetComponentInChildren<TMP_Text>().fontSize = 2;
            newButton.GetComponentInChildren<TMP_Text>().color = Color.red;

            newButton.tag = txt=="" ? "UiLine-Empty" : "UiLine";

        }

       

        /// <summary>
        ///  change the color of the two canvas included in C
        /// </summary>
        /// <param name="C">the canva that we want to update</param>
        private void UpdateCanva() // (Canva C)
        {
            //TODO select Values by his tag

            int currentLine = 0;

            if (myLines.Length != 0 && myLines != null)
            {

                for (int i = 0; i < Values.transform.childCount; i++) // for all the childs
                {
                    if (Values.transform.GetChild(i).tag == "UiLine") // but only thoses who r a button with text
                    {
                        if (currentLine == lineNumber) // is this the highlighted one's ?
                        {
                            var colors = Values.transform.GetChild(i).GetComponent<Button>().colors;
                            colors.normalColor = Color.red;
                            Values.transform.GetChild(i).GetComponent<Button>().colors = colors;
                            
                            Values.transform.GetChild(i).GetComponentInChildren<TMP_Text>().color = Color.white;
                        }
                        else
                        {
                            var colors = Values.transform.GetChild(i).GetComponent<Button>().colors;
                            colors.normalColor = Color.white;
                            Values.transform.GetChild(i).GetComponent<Button>().colors = colors;

                            Values.transform.GetChild(i).GetComponentInChildren<TMP_Text>().color = Color.red;
                        }
                        currentLine++;
                    }


                }



            }
        }
        

        /// <summary>
        /// update the object with the caracteristics you updated
        /// </summary>
        private void UpdateObject()
        {

            int[] prop = new int[6];
            List<Transform> lines = GetAllChilds(Values);
            for(int currentLine=0; currentLine < lines.Count; currentLine++)
            {
                string value = lines[currentLine].GetComponentInChildren<TMP_Text>().text;
                
                if (int.TryParse(value, out int j) && currentLine != 0)
                {
                    prop[currentLine-1] = j;
                }
            }

            objPrinted.transform.position = new Vector3(prop[0], prop[1], prop[2]);
            objPrinted.transform.localScale = new Vector3(prop[3], prop[4], prop[5]);

        }
        
        /// <summary>
        /// get the childs but only the childs tagged with UiLine
        /// </summary>
        /// <param name="C"></param>
        /// <returns></returns>
        private List<Transform> GetAllChilds(Canvas C)
        {
            List<Transform> list = new List<Transform>();


            for (int currentLine = 0; currentLine < C.transform.childCount; currentLine++) // for all the childs
            {
                if (Values.transform.GetChild(currentLine).tag == "UiLine") // but only thoses who r a button with text
                {
                    list.Add(Values.transform.GetChild(currentLine));
                }


            }
            return list;
        }








    }
}
