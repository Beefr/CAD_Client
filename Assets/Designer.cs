using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using TMPro;
using UnityEngine;

namespace Assets
{
    public delegate void MyDesignerHandler(object source, MyDesignArgs e);

    /// <summary>
    /// should be added to the SelectableUI to update it
    /// its purpose is to update the selectableUI 
    /// </summary>
    public class Designer: MonoBehaviour
    {
        public event MyDesignerHandler Designing;

        private GameObject objPrinted; // the object we are interested in showing the caracteristics
        public void SetObjPrinted(GameObject obj) { this.objPrinted = obj; }
        
        private int lineNumber = 0; // current line of the canva
        public void SetLineNumber(int lineNumber) { this.lineNumber = lineNumber; }
        
        private Canvas Values; // ui of the right
        private Canvas NameOfTheValues; // ui of the left

        public float spacing = 5; // spacement between lines
        public float tailleCaracteres = 2;
        public float widthButton = 30;
        public float heightButton = 5;

        string content = "";
        public string GetContent() { return content; }

        /// <summary>
        /// initializes the canvas inside the UI and add an handler
        /// </summary>
        void Start()
        {
            Values = GameObjectHelper.FindGameObjectInChildWithTag(gameObject, "Values").GetComponent<Canvas>();
            NameOfTheValues = GameObjectHelper.FindGameObjectInChildWithTag(gameObject, "NameOfTheValues").GetComponent<Canvas>();

            // Now lets test the event contained in the above class.
            this.Designing += new MyDesignerHandler(Design);
        }

        /// <summary>
        /// updates the UI with the objPrinted
        /// </summary>
        public void TryDesigning()
        {
            if (objPrinted != null)
                Designing(this, new MyDesignArgs(this.gameObject, objPrinted));
        }
        /// <summary>
        /// updates the UI with the new parameters
        /// </summary>
        /// <param name="param"></param>
        public void TryDesigning(List<float> param)
        {
            try
            {
                List<Transform> lines = GameObjectHelper.GetAllChilds(Values);
                for (int i = 1; i < lines.Count; i++) // not 0 because we don't modify the ID
                {
                    lines[i].GetComponentInChildren<TMP_Text>().text = param[i].ToString();
                }
            } catch (Exception) { }
        }
        /// <summary>
        /// updates the UI with the new parameters given as a string
        /// </summary>
        /// <param name="content"></param>
        public void TryDesigning(string content)
        {
            // deserialize the object 
            string position = StringHelper.GetBetween(content, "<Position", "Position>");
            string spax = StringHelper.GetBetween(position, "<a:x>", "</a:x>");
            Int32.TryParse(spax, out int pax);

            string spay = StringHelper.GetBetween(position, "<a:y>", "</a:y>");
            Int32.TryParse(spay, out int pay);

            string spaz = StringHelper.GetBetween(position, "<a:z>", "</a:z>");
            Int32.TryParse(spaz, out int paz);
            //_____________________
            string scale = StringHelper.GetBetween(content, "<Scale", "Scale>");
            string ssax = StringHelper.GetBetween(scale, "<a:x>", "</a:x>");
            Int32.TryParse(ssax, out int sax);

            string ssay = StringHelper.GetBetween(scale, "<a:y>", "</a:y>");
            Int32.TryParse(ssay, out int say);

            string ssaz = StringHelper.GetBetween(scale, "<a:z>", "</a:z>");
            Int32.TryParse(ssaz, out int saz);

            // update the object
            List<float> param = new List<float> { pax, pay, paz, sax, say, saz };

            TryDesigning(param);
        }
        /// <summary>
        /// set text but not about an object
        /// </summary>
        /// <param name="content"></param>
        public void SetText(string content)
        {
            int deletedValues = DeleteAll(Values);
            int deletedNames = DeleteAll(NameOfTheValues);

            AddLine(content, NameOfTheValues, deletedNames);
        }
        /// <summary>
        /// performs the update of the UI
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        void Design(object source, MyDesignArgs e)
        {
            convertStringToCanvas(e.GetUI(), e.GetObjPrintedString());
        }
        
        /// <summary>
        /// take the string and update your canva
        /// </summary>
        private void convertStringToCanvas(GameObject UI, string content)
        {

            this.content = convertStringToCanvaString(content);
            UpdateCanva(UI, this.content);

        }
        
        /// <summary>
        /// put the string in a format that we are able to use to update our canvas
        /// </summary>
        private string convertStringToCanvaString(string content)
        {
            string monString = "Name: " + StringHelper.GetBetween(content, "<a:name>", "</a:name>") + "\n\n";
            monString = monString + "<ID \n    " + StringHelper.GetBetween(content, "<ID>", "</ID>") + "\nID> \n\n";
            monString = monString + "<Position " + StringHelper.GetBetween(content, "<position xmlns:a=\"http://schemas.datacontract.org/2004/07/UnityEngine\">", "</position>") + "Position> \n\n";
            monString = monString + "<Scale " + StringHelper.GetBetween(content, "<scale xmlns:a=\"http://schemas.datacontract.org/2004/07/UnityEngine\">", "</scale>") + "Scale> \n\n";

            return monString;

        }

        /// <summary>
        /// update the canvas by wether changing their text or build buttons and so on
        /// </summary>
        private void UpdateCanva(GameObject UI, string content)
        {
            bool gotThere = false;

            int deletedValues = DeleteAll(Values);
            int deletedNames = DeleteAll(NameOfTheValues);

            string[] myLines = content.Split("\n\r".ToCharArray());

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
                            AddLine(line, NameOfTheValues, deletedNames);
                            AddLine("", Values, deletedValues);
                            break;
                        case 2:
                            AddLine(line, NameOfTheValues, deletedNames);
                            AddLine("", Values, deletedValues);
                            break;
                        case 4:
                            AddLine("", NameOfTheValues, deletedNames);
                            string newLine = line.Contains("<") ? StringHelper.GetBetween(line, ">", "<") : line;
                            AddLine(newLine, Values, deletedValues);
                            gotThere = true;
                            break;
                        default:
                            break;
                    }
                }

            }

            if (gotThere)
                ModifyCanva();

        }
        
        private int DeleteAll(Canvas C)
        {
            int deletedItems = 0;

            // delete the previous ones
            foreach (GameObject go in GameObjectHelper.FindGameObjectsInChildWithTag(C.gameObject, "UiLine-Empty"))
            {
                GameObject.Destroy(go);
                deletedItems++;
            }

            foreach (GameObject go in GameObjectHelper.FindGameObjectsInChildWithTag(C.gameObject, "UiLine"))
            {
                GameObject.Destroy(go);
                deletedItems++;
            }

            return deletedItems;

        }

        /// <summary>
        /// add a line to the canva
        /// </summary>
        /// <param name="txt">the txt that will appear in the line of the canva</param>
        /// <param name="C">the canva getting updated</param>
        private void AddLine(string txt, Canvas C, int deletedItems)
        {
            GameObject parent = C.gameObject;
            int linecount = GameObjectHelper.FindNumberOfChildsWithTag(parent, "UiLine-Empty") + GameObjectHelper.FindNumberOfChildsWithTag(parent, "UiLine") - deletedItems;
            GameObject newButton = (GameObject)Instantiate(Resources.Load("MesPrefabs/UiLine"), new Vector3(0, -linecount * spacing, 0), Quaternion.identity); 
            RectTransform rt = newButton.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(widthButton, heightButton);
            newButton.transform.SetParent(C.transform, false);


            newButton.GetComponentInChildren<TMP_Text>().text = txt;
            newButton.GetComponentInChildren<TMP_Text>().fontSize = tailleCaracteres;
            newButton.GetComponentInChildren<TMP_Text>().color = Color.red;

            newButton.tag = txt == "" ? "UiLine-Empty" : "UiLine";//*/

        }
        
        /// <summary>
        ///  change the color of the two canvas included in C
        /// </summary>
        /// <param name="C">the canva that we want to update</param>
        public void ModifyCanva() 
        {
            //TODO select Values by their tag

            int currentLine = 0;
            
            if (GameObjectHelper.FindNumberOfChildsWithTag(Values.gameObject, "UiLine")>0)
            {

                for (int i = 0; i < Values.transform.childCount; i++) // for all the childs
                {
                    if (Values.transform.GetChild(i).tag == "UiLine") // but only thoses who r a button with text
                    {
                        if (currentLine == lineNumber) // is this the highlighted one's ?
                        {
                            var colors = Values.transform.GetChild(i).GetComponent<UnityEngine.UI.Button>().colors;
                            colors.normalColor = Color.red;
                            Values.transform.GetChild(i).GetComponent<UnityEngine.UI.Button>().colors = colors;

                            Values.transform.GetChild(i).GetComponentInChildren<TMP_Text>().color = Color.white;
                        }
                        else
                        {
                            var colors = Values.transform.GetChild(i).GetComponent<UnityEngine.UI.Button>().colors;
                            colors.normalColor = Color.white;
                            Values.transform.GetChild(i).GetComponent<UnityEngine.UI.Button>().colors = colors;

                            Values.transform.GetChild(i).GetComponentInChildren<TMP_Text>().color = Color.red;
                        }
                        currentLine++;
                    }


                }



            }
            
        }
        
    }

    public class MyDesignArgs : EventArgs
    {
        private GameObject objPrinted; // the object we are interested in showing the caracteristics
        private string objPrintedString;
        private GameObject UI;

        public MyDesignArgs(GameObject UI, GameObject objPrinted)
        {
            this.UI = UI;
            this.objPrinted = objPrinted;

            // serialize the object
            MyGameObject myObj = new MyGameObject(objPrinted);
            DataContractSerializer DCS = new DataContractSerializer(myObj.GetType());
            MemoryStream streamer = new MemoryStream();
            DCS.WriteObject(streamer, myObj);
            streamer.Seek(0, SeekOrigin.Begin);
            objPrintedString = XElement.Parse(Encoding.ASCII.GetString(streamer.GetBuffer()).Replace("\0", "")).ToString();
        }
        public string GetObjPrintedString() { return objPrintedString; }
        public GameObject GetUI() { return UI; }

    }

   


}
