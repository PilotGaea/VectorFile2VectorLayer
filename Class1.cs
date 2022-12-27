using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PilotGaea.Geometry;
using PilotGaea.TMPEngine;
using PilotGaea.Serialize;

namespace VectorFile2VectorLayer
{
    public class Class1 : DoCmdBaseClass
    {
        private List<string> m_Cmds;
        public override void DeInit()
        {
            
        }

        public override bool DoCmd(CGeoDatabase db, string cmd, string sessionID, VarStruct inputParm, out VarStruct retParm)
        {
            bool Ret = false;
            retParm = null;
            switch (cmd)
            {
                case "VectorFile2VectorLayer":
                    Ret = VectorFile2VectorLayer(db, sessionID, inputParm, out retParm);
                    break;
                default:
                    break;
            }
            return Ret;
        }
        // 將來源VectorFile轉成VectorLayer加入Database中。
        // sourceUrl: string = 來源VectorFile路徑。
        // layerName: string = 要新增的圖層名稱。
        // epsg: int = 參考的EPSG。
        private bool VectorFile2VectorLayer(CGeoDatabase db, string sessionID, VarStruct inParm, out VarStruct outParm)
        {
            outParm = new VarStruct();
            outParm["success"].Set(false);
            outParm["error"].Set("");

            // 取得來源VectorFile路徑
            string sourceUrl = "";
            if (!inParm.TryGet("sourceUrl", ref sourceUrl))
            {
                outParm["error"].Set("no sourceUrl argument");
                return false;
            }
            // 取得要新增的圖層名稱
            string layerName = "";
            if (!inParm.TryGet("layerName", ref layerName))
            {
                outParm["error"].Set("no layerName argument");
                return false;
            }

            int epsg = 0;
            if (!inParm.TryGet("epsg", ref epsg))
            {
                outParm["error"].Set("no epsg argument");
                return false;
            }

            // 新增一個Vector DB
            CVectorFile DBFile = db.CreateVectorFile();

            if (!DBFile.Open($"C:\\ProgramData\\PilotGaea\\PGMaps\\{layerName}.DB"))
            {
                outParm["error"].Set("Open DB Error");
                return false;
            }
            // 將Vector File轉換為新圖層
            int LayerID = DBFile.NewLayerFromFile(layerName, epsg, LAYER_TYPE.VECTOR_BASE, sourceUrl, epsg);
            DBFile.Close();
            // 圖層設定
            var newLayerSetting = new VarStruct();
            newLayerSetting["Type"].Set("VECTOR_BASE");
            newLayerSetting["Show"].Set("1");
            newLayerSetting["Alpha"].Set("1");
            newLayerSetting["VisibleType"].Set("RESOLUTION");
            newLayerSetting["VisibleMin"].Set("0");
            newLayerSetting["VisibleMax"].Set("0");
            newLayerSetting["OffsetX"].Set("0");
            newLayerSetting["OffsetY"].Set("0");
            newLayerSetting["DistributeTileMatrixSet"].Set("EPSG:" + epsg);
            newLayerSetting["WMTS"].Set("1");
            newLayerSetting["WMS"].Set("1");
            newLayerSetting["WFS"].Set("0");
            newLayerSetting["CacheControl"].Set("604800");
            newLayerSetting["BkColor"].Set("#FFFFFFFF");
            newLayerSetting["AccessControl"].Clear();
            newLayerSetting["ViewportMode"].Clear();
            newLayerSetting["Metadata"].Clear();
            newLayerSetting["Name"].Set(layerName);
            newLayerSetting["EPSG"].Set(epsg.ToString());
            newLayerSetting["Url"].Set($"C:\\ProgramData\\PilotGaea\\PGMaps\\{layerName}.DB");
            newLayerSetting["ID"].Set(LayerID.ToString());
            // 將圖層加入Server
            if (db.NewLayer(newLayerSetting, null) == null)
            {
                outParm["error"].Set("Failed to add new layer.");
                return false;
            }
            // 儲存
            if (!db.Save())
            {
                outParm["error"].Set("Failed to save db.");
                return false;
            }

            outParm["success"].Set(true);
            return true;
        }

        public override string[] GetSupportCmds()
        {
            return m_Cmds.ToArray();
        }

        public override bool Init()
        {
            m_Cmds = new List<string>();
            m_Cmds.Add("VectorFile2VectorLayer");
            return true;
        }
    }
}
