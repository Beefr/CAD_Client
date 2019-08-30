using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    public class GameObjectHelper
    {



        /// <summary>
        /// find the first child of the parent that has the given tag
        /// </summary>
        /// <param name="parent">the parent of the child</param>
        /// <param name="tag">the tag of the child</param>
        /// <returns>the child</returns>
        public static GameObject FindGameObjectInChildWithTag(GameObject parent, string tag)
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
        public static List<Transform> GetAllChilds(Canvas C)
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
