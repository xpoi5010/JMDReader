using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Raycity.File;

namespace Raycity.Test
{
    public class Editor
    {
        public Editor()
        {

        }

        
        public void Testing()
        {
            JMDFile jf = new JMDFile(@"D:\M-etel\RayCity\Data\sound\bgm\main\mu_bgm_org_driversparadise.jmd");
            FileStream fs = new FileStream(@"D:\M-etel\RayCity\Data\sound\bgm\main\mu_bgm_org_driversparadise.jmd.decrypt", FileMode.Open);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            data = Crypt.JMDCrypt.Decrypt(data, Crypt.KeyGenerator.GetDictionaryDataKey(jf.HeaderKey));
            fs.Write(data, 0, data.Length);
            fs.Close();
        }
    }
}
