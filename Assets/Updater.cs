using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{

    public delegate void MyUpdaterHandler(object source, MyUpdateArgs e);

    /// <summary>
    ///  its purpose is to update objects
    /// </summary>
    public class Updater: MonoBehaviour
    {

        public event MyUpdaterHandler Updating;


        private GameObject objUpdated; // the object we are interested in showing the caracteristics
        public void SetObjUpdated(GameObject obj) { this.objUpdated = obj; }

        /// <summary>
        /// add the handler
        /// </summary>
        void Start()
        {
            // Now lets test the event contained in the above class.
            this.Updating += new MyUpdaterHandler(UpdateInProgress);
        }

        /// <summary>
        /// tries to update the object with the given parameters
        /// </summary>
        /// <param name="param"></param>
        public void TryUpdating(List<float> param)
        {
            if (objUpdated != null)
                Updating(this, new MyUpdateArgs(objUpdated, param));
        }
        /// <summary>
        /// tries to update with the parameters given in a string
        /// </summary>
        /// <param name="content">the parameters as string</param>
        public void TryUpdating(string content)
        {

            if (objUpdated != null)
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
                Updating(this, new MyUpdateArgs(objUpdated, param));
            }
        }
        /// <summary>
        /// updates with the parameters
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        void UpdateInProgress(object source, MyUpdateArgs e)
        {
            UpdateObject(e.GetParam());
        }



        
        /// <summary>
        /// update the object with the characteristics you updated
        /// </summary>
        private void UpdateObject(List<float> param)
        {
            if (param.Count==6)
            {
                objUpdated.transform.position = new Vector3(param[0], param[1], param[2]);
                objUpdated.transform.localScale = new Vector3(param[3], param[4], param[5]);
            }
            else if(param.Count==7) // the first is the ID
            {
                objUpdated.transform.position = new Vector3(param[1], param[2], param[3]);
                objUpdated.transform.localScale = new Vector3(param[4], param[5], param[6]);
            }
        }


    }

    /// <summary>
    /// contains parameters to update our object
    /// </summary>
    public class MyUpdateArgs : EventArgs
    {
        private List<float> param; 
        private GameObject obj;

        public MyUpdateArgs(GameObject obj, List<float> param)
        {
            this.obj = obj;
            this.param = param;
        }
        public GameObject GetObj() { return obj; }
        public List<float> GetParam() { return param; }

    }
}
