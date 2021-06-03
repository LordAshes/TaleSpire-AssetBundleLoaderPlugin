using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaleSpireAssetBundleLoaderTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, LordAshes.AssetBundleLoaderPlugin.AssetBundleInfo> bundles = new Dictionary<string, LordAshes.AssetBundleLoaderPlugin.AssetBundleInfo>();
            bundles.Add("Android", new LordAshes.AssetBundleLoaderPlugin.AssetBundleInfo() { source = @"D:\Hello\Bob", preload = false });
            bundles.Add("Kiki", new LordAshes.AssetBundleLoaderPlugin.AssetBundleInfo() { source = "C:/Hello/Bob", preload = true });

            string json = JsonConvert.SerializeObject(bundles);
        }
    }
}
