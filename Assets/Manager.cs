using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets
{
    public class Manager: MonoBehaviour
    {
        private GameObject obj; // the object we are interested in showing the caracteristics
        public void SetObj(GameObject obj) { this.obj = obj; }

        private int lineNumber = 0; // current line of the canva
        public int GetLineNumber() { return lineNumber; }


        private List<GameObject> selectablesUI; // containing the Designer
        public Updater updater;

        /// <summary>
        /// get the UI and the updater to "manage" them
        /// </summary>
        void Start()
        {
            if (updater==null)
                updater = GameObject.Find("Updater").GetComponent<Updater>();

            selectablesUI = GameObject.FindGameObjectsWithTag("SelectableUI").ToList<GameObject>();
        }

        /// <summary>
        /// updates the UI if there are any
        /// </summary>
        void Update()
        {
            if (selectablesUI.Count>0)
            {
                // _____________ PANEL SELECTION TO UPDATE PROPERTIES __________________________________________________________
                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown) || Input.GetKeyDown(KeyCode.S))
                {
                    lineNumber++;
                    int linecount = FindNumberOfChildsWithTag(FindGameObjectInChildWithTag(selectablesUI[0], "Values"), "UiLine");
                    if (lineNumber >= linecount) { lineNumber = 0; }

                    foreach(GameObject SelectableUI in selectablesUI)
                    {
                        SelectableUI.GetComponent<Designer>().SetLineNumber(lineNumber);
                        SelectableUI.GetComponent<Designer>().ModifyCanva();
                    }
                }

                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp) || Input.GetKeyDown(KeyCode.Z))
                {
                    lineNumber--;
                    int linecount = FindNumberOfChildsWithTag(FindGameObjectInChildWithTag(selectablesUI[0], "Values"), "UiLine");
                    if (lineNumber < 0)
                    {
                        var val = linecount - 1;
                        lineNumber = (val >= 0) ? val : 0;
                    }

                    foreach (GameObject SelectableUI in selectablesUI)
                    {
                        SelectableUI.GetComponent<Designer>().SetLineNumber(lineNumber);
                        SelectableUI.GetComponent<Designer>().ModifyCanva();
                    }
                }

                // upgrade/downgrade caracteristics of the object
                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickLeft) || Input.GetKeyDown(KeyCode.Q))
                {
                    downgradeItem();
                }
                if (OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickRight) || Input.GetKeyDown(KeyCode.D))
                {
                    upgradeItem();
                }
            }
            


        }



        /// <summary>
        /// the item selected gets downgraded
        /// </summary>
        private void downgradeItem()
        {
            if (obj != null)
            {
                updater.SetObjUpdated(obj);
                List<float> param = GetCurrentParameters();
                param[lineNumber] = param[lineNumber] - 1;
                updater.TryUpdating(param);

                foreach (GameObject SelectableUI in selectablesUI)
                {
                    SelectableUI.GetComponent<Designer>().TryDesigning(param);
                }
            }

        }

        /// <summary>
        /// the item selected gets upgraded
        /// </summary>
        private void upgradeItem()
        {
            if (obj != null)
            {
                updater.SetObjUpdated(obj);
                List<float> param = GetCurrentParameters();
                param[lineNumber] = param[lineNumber] + 1;
                updater.TryUpdating(param);

                foreach (GameObject SelectableUI in selectablesUI)
                {
                    SelectableUI.GetComponent<Designer>().TryDesigning(param);
                }
            }
        }

        /// <summary>
        /// get the parameters inside the UI
        /// </summary>
        /// <returns>a list of parameters as List<float> </returns>
        private List<float> GetCurrentParameters()
        {
            List<float> param = new List<float>();
            List<Transform> lines = GetAllChilds(FindGameObjectInChildWithTag(selectablesUI[0], "Values").GetComponent<Canvas>());
            foreach (Transform line in lines)
            {
                string value = line.GetComponentInChildren<TMP_Text>().text;

                int.TryParse(value, out int j);

                param.Add(j);
            }
            return param;
        }

        /// <summary>
        /// tries to update each UI by calling their own designer
        /// </summary>
        public void TryDesigning()
        {
            foreach (GameObject selectableUI in selectablesUI)
            {
                Designer designer = selectableUI.GetComponent<Designer>();
                designer.SetObjPrinted(obj);
                designer.TryDesigning();
            }
        }
        /// <summary>
        /// tries to update each UI by calling their own designer with the given content
        /// </summary>
        /// <param name="content">a string containing the parameters of the object</param>
        public void TryDesigning(string content)
        {
            foreach (GameObject selectableUI in selectablesUI)
            {
                Designer designer = selectableUI.GetComponent<Designer>();
                designer.SetObjPrinted(obj);
                designer.TryDesigning(content);
            }
        }
        /// <summary>
        /// set text but not about an object
        /// </summary>
        /// <param name="content"></param>
        public void SetText(string content)
        {
            foreach (GameObject selectableUI in selectablesUI)
            {
                Designer designer = selectableUI.GetComponent<Designer>();
                designer.SetText(content);
            }
        }
        /// <summary>
        /// tries to update the object
        /// </summary>
        /// <param name="content"></param>
        public void TryUpdating(string content)
        {
            updater.SetObjUpdated(obj);
            updater.TryUpdating(content);
        }




        // functions to look into my instantiated prefabs______________________________________________________________________

        /// <summary>
        /// find the first child of the parent that has the given tag
        /// </summary>
        /// <param name="parent">the parent of the child</param>
        /// <param name="tag">the tag of the child</param>
        /// <returns>the child</returns>
        private static GameObject FindGameObjectInChildWithTag(GameObject parent, string tag)
        {
            Transform parentTransform = parent.transform;

            for (int i = 0; i < parentTransform.childCount; i++)
            {
                if (parentTransform.GetChild(i).gameObject.tag == tag)
                {
                    return parentTransform.GetChild(i).gameObject;
                }

            }

            return null;
        }

        /// <summary>
        /// find all the childs of the parent that have the given tag
        /// </summary>
        /// <param name="parent">the parent of the childs</param>
        /// <param name="tag">the tag of the childs</param>
        /// <returns>the childs</returns>
        public static List<GameObject> FindGameObjectsInChildWithTag(GameObject parent, string tag)
        {
            Transform parentTransform = parent.transform;
            List<GameObject> childs = new List<GameObject>();

            for (int i = 0; i < parentTransform.childCount; i++)
            {
                if (parentTransform.GetChild(i).gameObject.tag == tag)
                {
                    childs.Add(parentTransform.GetChild(i).gameObject);
                }

            }

            return childs;
        }

        /// <summary>
        /// retrieves the number of childs in the parent that have the given tag
        /// </summary>
        /// <param name="parent">the parent of the childs</param>
        /// <param name="tag">the tag of the childs</param>
        /// <returns>the number of childs</returns>
        public static int FindNumberOfChildsWithTag(GameObject parent, string tag)
        {
            return FindGameObjectsInChildWithTag(parent, tag).Count;
        }

        /// <summary>
        /// get the childs but only the childs tagged with UiLine
        /// </summary>
        /// <param name="C">the canva containing the childs</param>
        /// <returns>all the childs </returns>
        private List<Transform> GetAllChilds(Canvas C)
        {
            List<Transform> list = new List<Transform>();


            for (int currentLine = 0; currentLine < C.transform.childCount; currentLine++) // for all the childs
            {
                if (C.transform.GetChild(currentLine).tag == "UiLine") // but only thoses who r a button with text
                {
                    list.Add(C.transform.GetChild(currentLine));
                }


            }
            return list;
        }


    }
}
